import React from "react"
import { Button, Input } from "./Components"
import { useDataModelState } from "../Data/DataModelContext"
import type { Item } from "../Data";
import { ItemService } from "../Data/ItemService";

//FIXME: not updating when state changes
export function Sidebar() {
    const {modelState, dataModel} = useDataModelState(); 
    const inputRef = React.useRef<HTMLInputElement>(null!)

    const HandleItemAdd = () => {
        // TODO: do search for item
        if(!inputRef.current || inputRef.current.value.trim() == "") return; 

        dataModel.AddItem(inputRef.current.value.trim())
        inputRef.current.value = ""; 
    }

    const HandleItemRemove = (item:Item) => {
        dataModel.RemoveItem(item.Token); 
    }

    const HandleItemSelect = (item:Item) => {
        dataModel.SetSelectedItem(item.Token); 
    }
    
    return <div className={styles._}>
        <div className={styles.header}>
            <Input ref={inputRef} onKeyDown={e => e.key == 'Enter' && HandleItemAdd()} />
            <Button children={"Add"} onClick={HandleItemAdd} />
        </div>

        <div className={styles.list}>
            {Object.keys(modelState.ItemDict).map(k => 
                <ListItem key={k} 
                    data={modelState.ItemDict[k]} 
                    isSelected={modelState.ItemDict[k].Token == modelState.SelectedItemToken}
                    onClose={() => HandleItemRemove(modelState.ItemDict[k])} 
                    onSelect={() => HandleItemSelect(modelState.ItemDict[k])} 
                />)}
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

export function ListItem(props:{data:Item, isSelected:boolean, onClose:()=>void, onSelect:()=>void}){
    const selectStyle = React.useMemo(() => {
        return props.isSelected ? 
            {
                color: 'blue', 
            } as React.CSSProperties 
        : {} as React.CSSProperties;
    }, [props.isSelected]); 

    const iconPath = React.useMemo(() => {
        return ItemService.GetItemIconPath(props.data.Token); 
    }, [props.data])

    return <div style={selectStyle} className={styles.listItem._} onClick={props.onSelect}>
        <img className={styles.listItem.img} src={iconPath} />
        <div className={styles.listItem.labelGroup}>
            <p>{props.data.Label}</p>
            <p>{props.data.Token}</p>
        </div>
        <Button onClick={props.onClose}>X</Button>
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
            hover:bg-gray-200
            active:bg-gray-100
            selected:bg-blue-100
        `, 
        img:`max-h-15 max-w-15`,
        labelGroup: `grid grid-cols-1 grid-rows-2`, 
    }, 
    ButtonGroup: {
        _: `grid grid-cols-1 grid-rows-auto gap-1`, 
    }
}