
export type GridPosition = {
    Row:number, 
    Col:number,
}

export type ItemProps = {
    Label:string, 
    // Token:string, 
    IconString:string,
}

export type Item = {
    Label:string, 
    Token:string, 
    NodeOrigin:GridPosition[], 
    ActiveOrigin:GridPosition[], 
    MaxStackCount:number
}

export type DataModelState = {
    ItemDict: {[key:string]: Item}, 
    SelectedItemToken:null|string, 
    UpdateTimestamp:number
}

export type DataModelAction = {
    Label:string, 
    Args:string
    State:DataModelState
}

export enum PaintType {
    BASE, ACTIVE, ERASER
}