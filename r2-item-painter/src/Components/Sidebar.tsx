import { Button, Input } from "./Components"


export function Sidebar() {
    return <div className={styles._}>
        <div className={styles.header}>
            <Input />
            <Button children={"Add"} />
        </div>

        <div className={styles.list}>
            <ListItem />
            <ListItem />
            <ListItem />
            <ListItem />
            <ListItem />
        </div>

        <div className={styles.ButtonGroup._}>
            <Button>Export to File</Button>
            <Button>Export to Clipboard</Button>
            <Button>Import from File</Button>
            <Button>Import from Clipboard</Button>
            <Button>Settings</Button>
        </div>
    </div>
}

export function ListItem(){
    return <div className={styles.listItem._}>
        <img className={styles.listItem.img} src="/src/Assets/57_Leaf_Clover.webp" />
        <p>Some Item</p>
        <Button>Close</Button>
    </div>
}

const styles = {
    _: `
        grid grid-cols-1 grid-rows-[auto_1fr_auto] gap-4 p-2
        border
    `, 
    header: `
        w-full grid grid-cols-[1fr_auto] grid-rows-1 gap-2
    `, 
    list: `
        flex flex-col gap-1
        overflow-y-scroll
    `, 
    listItem:{
        _: `
            min-h-20 max-h-30 p-2
            border 
            grid grid-cols-[auto_1fr_auto] grid-rows-1 gap-1
            items-center
        `, 
        img:`max-h-15 max-w-15`
    }, 
    ButtonGroup: {
        _: `grid grid-cols-1 grid-rows-auto gap-1`, 
    }
}