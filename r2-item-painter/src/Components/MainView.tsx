import React, { useMemo } from "react"
import { Button } from "./Components"

export function MainView() {
    const [gridRadius,] = React.useState(3); 

    const effectiveDiam = useMemo(() => {
        if(gridRadius <= 1) return 1; 
        return (gridRadius * 2) - 1; 
    }, [gridRadius])

    const gridstyle = useMemo(() => {
        return {
            display: 'grid', 
            gridTemplateColumns: `repeat(${effectiveDiam}, auto)`,
            gridTemplateRows: `repeat(${effectiveDiam}, auto)`
        } as React.CSSProperties; 
    }, [effectiveDiam]);

    console.log(effectiveDiam, gridRadius)
    
    return <div className={styles._}>
        <div className={styles.header._}>
            <img className={styles.header.img} src="/src/Assets/57_Leaf_Clover.webp"  />
            <h1>Some Item</h1>
        </div>

        <div className={`${styles.paintArea._}`} style={gridstyle}>
            {Array(effectiveDiam ** 2).fill(0).map((_, i) => <div className={styles.paintArea.cell} key={i} />)}
        </div>

        <div className={styles.palette._}>
            <Button className={styles.palette.tool}>Node</Button>
            <Button className={styles.palette.tool}>Active</Button>
            <Button className={styles.palette.tool}>Eraser</Button>
        </div>
    </div>
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
        img: `max-h-15 max-w-15`, 
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
        tool:`aspect-3/1 h-10`
    }
}