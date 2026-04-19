import React, { useCallback, useMemo } from "react"
import { Button } from "./Components"
import { ItemService } from "../Data/ItemService";
import { useDataModelState } from "../Data/DataModelContext";
import { PaintType, type GridPosition } from "../Data";



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

    const HandlePaint = (nodes:GridPosition[]) => {
        console.log("Painting Nodes: ", nodes);  
        dataModel.PaintItem(selectedItem ? selectedItem.Token : "", nodes, selectedTool); 
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

                <PaintArea OnPaint={HandlePaint} tool={selectedTool} activeNodes={selectedItem.ActiveOrigin} baseNodes={selectedItem.NodeOrigin} />

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

export function PaintArea(props:{tool:PaintType, baseNodes:GridPosition[], activeNodes:GridPosition[], OnPaint:(nodes:GridPosition[])=>void}) {
    const [gridRadius,] = React.useState(5); 
    const [pendingPaintCells, setPendingPaintCells] = React.useState([] as number[]); 
    const [isMouseDown, setIsMouseDown] = React.useState(false); 
    
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

    const cellToGrid = useCallback((pos:GridPosition) => {
        return (pos.Row + gridRadius - 1) * effectiveDiam + pos.Col + gridRadius - 1; 
    }, [gridRadius, effectiveDiam]); 

    const baseCells = props.baseNodes.map(n => cellToGrid(n)); 
    const activeCells = props.activeNodes.map(n => cellToGrid(n)); 


    const HandleMouseEnter = (cellIndex:number) => {
        setPendingPaintCells(p => [...p, cellIndex])
    }

    // eslint-disable-next-line react-hooks/preserve-manual-memoization
    const HandleMouseUp = React.useCallback(() => {
        setIsMouseDown(false)
        props.OnPaint(pendingPaintCells.map(cellI => ({
            Row: Math.floor(cellI / effectiveDiam) - (gridRadius - 1) , 
            Col: cellI % effectiveDiam - (gridRadius - 1)
        }))); 
        setPendingPaintCells([]); 
    }, [effectiveDiam, gridRadius, pendingPaintCells, props])

    const HandleMouseDown = () => {
        setIsMouseDown(true)
    }

    React.useEffect(() => {
        document.addEventListener('mouseup', HandleMouseUp); 
        return () => {
            document.removeEventListener('mouseup', HandleMouseUp); 
        }
    }, [HandleMouseUp]); 

    return (
        <div className={`${styles.paintArea._}`} style={gridstyle} onMouseDown={HandleMouseDown} onMouseUp={HandleMouseUp}>
            {Array(effectiveDiam ** 2).fill(0).map((_, i) => {
                let targetColor = PaintColors[PaintType.ERASER]; 
                if(pendingPaintCells.includes(i)) targetColor = PaintColors[props.tool]; 
                else if(baseCells.includes(i)) targetColor = PaintColors[PaintType.BASE]; 
                else if (activeCells.includes(i)) targetColor = PaintColors[PaintType.ACTIVE]; 

                return <div 
                    style={{backgroundColor: targetColor}}
                    className={styles.paintArea.cell} 
                    key={i} 
                    onMouseDown={() => HandleMouseEnter(i)} onMouseEnter={() => isMouseDown && HandleMouseEnter(i)} 
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
    paintArea: {
        _: `
            border
            w-full h-full
        `, 
        cell: `border`, 
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