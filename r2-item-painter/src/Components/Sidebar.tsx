import React from "react"
import { Button, Search } from "./Components"
import { useDataModelState } from "../Data/DataModelContext"
import type { DataModelState, Item } from "../Data";
import { ItemService } from "../Data/ItemService";
import { useNotificationContext } from "./Notificaton/NotificationService";

//FIXME: not updating when state changes
export function Sidebar() {
    const notifService = useNotificationContext(); 
    const {modelState, dataModel} = useDataModelState(); 
    const [curFiler, setCurFilter] = React.useState<"all"|"equip"|"items">("all")
    const inputRef = React.useRef<HTMLInputElement>(null!)

    const HandleItemAdd = () => {
        // TODO: do search for item
        if(!inputRef.current || inputRef.current.value.trim() == "") return; 

        dataModel.AddItem(inputRef.current.value.trim())
        inputRef.current.value = ""; 
    }

    const HandleSearchItemSelected = (token:string) => {
        dataModel.AddItem(token)
    }

    const HandleItemRemove = (item:Item) => {
        dataModel.RemoveItem(item.Token); 
    }

    const HandleItemSelect = (item:Item) => {
        dataModel.SetSelectedItem(item.Token); 
    }
    
    const HandleExportToClipboard = () => {
        navigator.clipboard.writeText(JSON.stringify(modelState.ItemDict, null, 2)); 
        notifService.PushNotificaton({
            title: "Added to Clipboard", 
            body:null, 
            status: "info", 
        })
    }

    const HandleListFilter = (filter:'all'|'items'|'equip') => {
        setCurFilter(filter);
    }

    const filteredItems = React.useMemo(() => {
        switch(curFiler) {
            case 'all':     
                return modelState.ItemDict; 
            case "equip": 
                return Object.keys(modelState.ItemDict).reduce((filtered, key) => {
                    if(modelState.ItemDict[key].Token.includes("EQUIP")) filtered[key] = modelState.ItemDict[key]; 
                    return filtered;
                }, {} as DataModelState['ItemDict'])
            case "items": 
                return Object.keys(modelState.ItemDict).reduce((filtered, key) => {
                    if(modelState.ItemDict[key].Token.includes("ITEM")) filtered[key] = modelState.ItemDict[key]; 
                    return filtered;
                }, {} as DataModelState['ItemDict'])
        }
    }, [curFiler, modelState])


    return <div className={styles._}>
        <div className={styles.header}>
            {/* <Input ref={inputRef} onKeyDown={e => e.key == 'Enter' && HandleItemAdd()} /> */}
            <Search ref={inputRef} OnItemSelected={HandleSearchItemSelected} />
            <Button children={"Add"} onClick={HandleItemAdd} />
        </div>

        <div className={styles.filterGroup}>
            <Button className={`${styles.filterGroupButton} ${curFiler == 'all' ? "selected" : ""}`} onClick={() => HandleListFilter('all')}>All</Button>
            <Button className={`${styles.filterGroupButton} ${curFiler == 'items' ? "selected" : ""}`} onClick={() => HandleListFilter('items')}>Items</Button>
            <Button className={`${styles.filterGroupButton} ${curFiler == 'equip' ? "selected" : ""}`} onClick={() => HandleListFilter('equip')}>Equipment</Button>
        </div>

        <div className={styles.list}>
            {Object.entries(filteredItems).map(([k, val]) => 
                <ListItem key={k} 
                    data={val} 
                    isSelected={val.Token == modelState.SelectedItemToken}
                    onClose={() => HandleItemRemove(val)} 
                    onSelect={() => HandleItemSelect(val)} 
                />)}
        </div>

        <div className={styles.ButtonGroup._}>
            <Button onClick={HandleExportToClipboard}>Export to Clipboard</Button>
            {/* <Button>Export to File</Button> */}
            {/* <Button>Import from File</Button> */}
            {/* <Button>Import from Clipboard</Button> */}
            {/* <Button>Settings</Button> */}
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
            <p className={styles.listItem.headerLabel}>{props.data.Label}</p>
            <p className={styles.listItem.tokenLabel}>{props.data.Token}</p>
        </div>
        <Button onClick={props.onClose}>X</Button>
    </div>
}

const styles = {
    _: `
        grid grid-cols-1 grid-rows-[auto_auto_1fr_auto] gap-4 p-2
        border
    `, 
    filterGroup: `grid grid-cols-3 grid-rows-1 gap-2 p-2`, 
    filterGroupButton: `truncate [.selected]:font-bold [.selected]:underline`, 
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
        headerLabel: `font-bold`, 
        tokenLabel: `text-[70%] truncate`, 
    }, 
    ButtonGroup: {
        _: `grid grid-cols-1 grid-rows-auto gap-1`, 
    }
}