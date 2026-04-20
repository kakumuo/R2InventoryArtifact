import React, { useCallback, useMemo } from "react"
import { Button} from "./Components"
import { ItemService } from "../Data/ItemService";
import { useDataModelState } from "../Data/DataModelContext";
import { PaintType, type GridPosition } from "../Data";
import { StarIcon } from "./Icons";



// const PaintType = {
//     BASE:0, ACTIVE:1, ERASER:2
// }
// type PaintType = (typeof PaintType)[keyof typeof PaintType]

const PaintToolLabels = {
    [PaintType.BASE]: "Node",
    [PaintType.ACTIVE]: "Active",
    [PaintType.ERASER]: "Eraser",
}

const PaintColors = {
    [PaintType.BASE]: "#3e7524",
    [PaintType.ACTIVE]: "#fab73c",
    [PaintType.ERASER]: "#ffffff5e",
}

export function MainView() {
    const {modelState, dataModel } = useDataModelState(); 
    const [selectedTool, setSelectedTool] = React.useState(PaintType.ACTIVE); 

    const selectedItem = React.useMemo(() => {
        return modelState.SelectedItemToken != null ? modelState.ItemDict[modelState.SelectedItemToken] : null; 
    }, [modelState]);   

    const iconPath = React.useMemo(() => {
        return ItemService.GetItemIconPath(selectedItem ? selectedItem.Token : ""); 
    }, [selectedItem]); 

    const HandlePaint = (nodes:GridPosition[], tool:PaintType) => {
        // console.log("Painting Nodes: ", nodes, "selectedTool:",  PaintToolLabels[selectedTool]);  
        dataModel.PaintItem(selectedItem ? selectedItem.Token : "", nodes.filter(n => !(n.Row == 0 && n.Col == 0)), tool); 
    }

    const HandleSelectPaintTool = (tool:number) => {
        setSelectedTool(tool); 
    }

    return <div className={styles._}>
        {selectedItem ? 
            <>
                <div className={styles.header._}>
                    <img className={styles.header.img} src={iconPath}  />
                    <h1>{selectedItem?.Label}</h1>
                </div>

                <PaintArea 
                    OnPaint={(nodes) => HandlePaint(nodes, selectedTool)} 
                    OnErase={(nodes) => HandlePaint(nodes, PaintType.ERASER)}
                    tool={selectedTool} 
                    activeNodes={selectedItem.ActiveOrigin} 
                    baseNodes={selectedItem.NodeOrigin}
                />

                <div className={styles.palette._}>
                    {Object.values(PaintToolLabels).map((label, type) => 
                        <Button 
                            style={{backgroundColor: PaintColors[type as PaintType]}}
                            className={`${selectedTool == type ? 'selected' : ''} ${styles.palette.tool}`} 
                            onClick={() => HandleSelectPaintTool(type)} key={type}>{label}</Button>
                    )}
                </div>
            </> : 
            <>Select an item</>
        }
    </div>
}


const MouseButtonType = {
    LEFT:1, RIGHT:2
}
type MouseButtonType = (typeof MouseButtonType)[keyof typeof MouseButtonType]

export function PaintArea(props:{tool:PaintType, baseNodes:GridPosition[], activeNodes:GridPosition[], OnPaint:(nodes:GridPosition[])=>void, OnErase:(nodes:GridPosition[])=>void}) {
    const [gridRadius,] = React.useState(5); 
    const [pendingPaintCells, setPendingPaintCells] = React.useState([] as number[]); 
    // const [isMouseDown, setIsMouseDown] = React.useState(false);
    const [mouseButtonDown, setMouseButtonDown] = React.useState(0); //0:none, 1:left, 2:right 
    
    const effectiveDiam = useMemo(() => {
        if(gridRadius <= 1) return 1; 
        return (gridRadius * 2) - 1; 
    }, [gridRadius])

    const gridstyle = useMemo(() => {
        return {
            display: 'grid', 
            gridTemplateColumns: `repeat(${effectiveDiam}, auto)`,
            gridTemplateRows: `repeat(${effectiveDiam}, auto)`, 
            userSelect: 'none'
        } as React.CSSProperties; 
    }, [effectiveDiam]);

    const GridToCell = useCallback((pos:GridPosition) => {
        return (pos.Row + gridRadius - 1) * effectiveDiam + pos.Col + gridRadius - 1; 
    }, [gridRadius, effectiveDiam]); 

    const CellToGrid = useCallback((i:number) => {
        return {
            Row: Math.floor(i / effectiveDiam) - (gridRadius - 1) , 
            Col: i % effectiveDiam - (gridRadius - 1)
        } as GridPosition
    }, [effectiveDiam, gridRadius])

    const baseCells = props.baseNodes.map(n => GridToCell(n)); 
    const activeCells = props.activeNodes.map(n => GridToCell(n)); 


    const HandleMouseEnter = (cellIndex:number) => {
        setPendingPaintCells(p => [...p, cellIndex])
    }

    // eslint-disable-next-line react-hooks/preserve-manual-memoization
    const HandleMouseUp = React.useCallback((ev:MouseEvent) => {
        if(pendingPaintCells.length == 0) return; 
        
        if(mouseButtonDown == MouseButtonType.LEFT)
            props.OnPaint(pendingPaintCells.map(cellI => CellToGrid(cellI))); 
        else if (mouseButtonDown == MouseButtonType.RIGHT)
            props.OnErase(pendingPaintCells.map(cellI => CellToGrid(cellI))); 
        setPendingPaintCells([]); 

        if(ev.buttons == 0) {
            setMouseButtonDown(0) 
        }
    }, [CellToGrid, pendingPaintCells, props])

    const HandleMouseDown = (ev:MouseEvent) => {
        if(mouseButtonDown != 0) return; 
        setMouseButtonDown(ev.buttons & (MouseButtonType.LEFT | MouseButtonType.RIGHT))    
    }

    React.useEffect(() => {
        document.addEventListener('mouseup', HandleMouseUp); 
        document.addEventListener('contextmenu', (ev) => {ev.preventDefault()}); 
        return () => {
            document.removeEventListener('mouseup', HandleMouseUp); 
        }
    }, [HandleMouseUp]); 

    return (
        <div className={`${styles.paintArea._}`} style={gridstyle} onMouseDown={(ev) => HandleMouseDown(ev.nativeEvent)} onMouseUp={(ev) => HandleMouseUp(ev.nativeEvent)}>
            {Array(effectiveDiam ** 2).fill(0).map((_, i) => {
                let targetColor = PaintColors[PaintType.ERASER]; 
                if(pendingPaintCells.includes(i)) targetColor = mouseButtonDown == MouseButtonType.LEFT ? PaintColors[props.tool] : PaintColors[PaintType.ERASER]; 
                else if(baseCells.includes(i)) targetColor = PaintColors[PaintType.BASE]; 
                else if (activeCells.includes(i)) targetColor = PaintColors[PaintType.ACTIVE]; 

                return <div 
                    style={{backgroundColor: targetColor}}
                    className={styles.paintArea.cell} 
                    key={i} 
                    onMouseDown={() => HandleMouseEnter(i)} onMouseEnter={() => mouseButtonDown > 0 && HandleMouseEnter(i)} 
                    children={CellToGrid(i).Col == 0 && CellToGrid(i).Row == 0 && <StarIcon className={styles.paintArea.star} />}
                />
            })}
        </div>
    )
}


const styles = {
    _: `
        border 
        flex flex-col
        justify-between
        gap-4
        w-full h-full
    `, 
    header: {
        _: `
            border
            grid grid-cols-[auto_1fr]
            p-2 gap-2
            items-center
        `, 
        img: `
            w-15 h-15
            max-h-15 max-w-15
        `, 
    },
    // TODO: scale paint area with size of container
    paintArea: {
        _: `
            border
            my-auto mx-auto
            w-auto h-auto
        `, 
        cell: `border grid min-w-15 min-h-15`, 
        star: `fill-yellow-200 w-auto h-auto self-center justify-self-center`, 
    }, 
    palette: {
        _:`
            border 
            grid grid-cols-3 grid-rows-1
            p-4 px-8 mx-auto
            gap-2
        `, 
        tool:`aspect-3/1 h-10 [.selected]:font-bold [.selected]:underline`
    }
}