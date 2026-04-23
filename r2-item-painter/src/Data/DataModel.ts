import { ItemService } from "./ItemService";
import { PaintType, type DataModelAction, type DataModelState, type GridPosition, type Item } from ".";

export class DataModel {

    private _state:DataModelState; 
    private _history:DataModelAction[]; 
    private _historyIndex:number; 
    private _listeners:Set<()=>void>; 

    constructor(){
        this._state = {
            ItemDict: {}, 
            SelectedItemToken: null
        } as DataModelState; 
        this._history = [];
        this._historyIndex = 0;
        this._listeners = new Set(); 

        // // DEBUG: remove later
        // this._state = {
        //     ItemDict: {"TEST_ITEM": {Label: "Test Item", ActiveOrigin:[{Row: 0, Col: 1}], NodeOrigin: [{Row: 0, Col: 0}], MaxStackCount:5, Token: "TEST_ITEM"}},
        //     SelectedItemToken: "TEST_ITEM", 
        //     UpdateTimestamp: Date.now()
        // }

        this.SetState("constructor", [], this._state); 
    }

    LoadState = (data:string) => {
        let jsonData = {};
        try {
            jsonData = JSON.parse(data);
        } catch {
            return null;  
        }

        if(!Object.prototype.hasOwnProperty.call(jsonData, "ItemDict")) return null; 
        if(!Object.prototype.hasOwnProperty.call(jsonData, "SelectedItemToken")) return null; 

        return this.SetState("LoadData", [data.length.toString()], jsonData as DataModelState); 
    }

    LoadItemDict = (data:string) => {
        let jsonData = {};
        try {
            console.log(typeof(data)); 
            jsonData = JSON.parse(data);
        } catch {
            return false;  
        }

        // rotate the items 180; FIXME: update UI to correct item orientation
        this._state.ItemDict = jsonData; 
        for(const key of Object.keys(this._state.ItemDict)) {
            this.RotateItem180(this._state.ItemDict[key]); 
        }
        this.SetState("LoadItemDict", [data.length.toString()], this._state); 
        return true; 
    }

    ExportItemDict = () => {
        const itemDict:DataModelState['ItemDict'] = JSON.parse(JSON.stringify(this._state.ItemDict)) as DataModelState['ItemDict']; 
        for(const key of Object.keys(itemDict)) {
            this.RotateItem180(itemDict[key]); 
        }
        return JSON.stringify(itemDict, null, 2)
    }
    
    private RotateItem180 = (item:Item) => {
        for(let i = 0; i < item.NodeOrigin.length; i++) {
            [item.NodeOrigin[i].Row, item.NodeOrigin[i].Col] = [-item.NodeOrigin[i].Row, -item.NodeOrigin[i].Col]
        }
    }
    
    LoadHistory(history: string) {
        const historyObj = JSON.parse(history); 
        this._history = historyObj.history; 
        this._historyIndex = historyObj.historyIndex;
    }

    /* ITEM LIST */
    AddItem = (token:string) => {
        if(token == "" || Object.prototype.hasOwnProperty.call(this._state.ItemDict, token)) 
            return null;

        const item:Item = {
            Label: "Untitled Item", 
            Token: token, 
            MaxStackCount: 5, 
            NodeOrigin: [{Row: 0, Col: 0}], 
            ActiveOrigin: [], 
        }; 
        
        const itemLabel = ItemService.GetItemLabel(token); 
        if(itemLabel)
            item.Label = itemLabel; 

        this._state.ItemDict[token] = item; 
        return this.SetState("AddItem", [token], this._state); 
    }
    RemoveItem = (token:string) => {
        if(!token || token == "" || !Object.prototype.hasOwnProperty.call(this._state.ItemDict, token)) 
            return null;
        
        delete(this._state.ItemDict[token]); 
        if(this._state.SelectedItemToken == token) {
            this._state.SelectedItemToken = null; 
        }

        return this.SetState("RemoveItem", [token], this._state); 
    }

    /* ITEM PROPS */
    SetSelectedItem = (token:string|null) => {
        if(!token || token == "" || !Object.prototype.hasOwnProperty.call(this._state.ItemDict, token)) 
            return null; 

        if(this._state.SelectedItemToken == token)
            return null; 

        this._state.SelectedItemToken = token; 
        return this.SetState("SetSelectedItem", [token], this._state); 
    }

    PaintItem = (token:string, nodes:GridPosition[], type:PaintType) => {
        if(!token || token == "" || !Object.prototype.hasOwnProperty.call(this._state.ItemDict, token)) 
            return null;

        // console.log(token, nodes, type); 

        switch(type) {
            case PaintType.BASE: 
                this._state.ItemDict[token].NodeOrigin = [...new Set([...this._state.ItemDict[token].NodeOrigin, ...nodes])];  
                this._state.ItemDict[token].NodeOrigin.sort((a, b) => (a.Col + a.Row) - (b.Col + b.Row)); // sort to make sure 0,0 is always first
                this._state.ItemDict[token].ActiveOrigin = 
                    this._state.ItemDict[token].ActiveOrigin.filter(pos => nodes.findIndex(curNode => curNode.Row === pos.Row && curNode.Col == pos.Col) == -1)
            break; 
            case PaintType.ACTIVE:
                this._state.ItemDict[token].ActiveOrigin = [...new Set([...this._state.ItemDict[token].ActiveOrigin, ...nodes])];  
                this._state.ItemDict[token].NodeOrigin = 
                    this._state.ItemDict[token].NodeOrigin.filter(pos => nodes.findIndex(curNode => curNode.Row === pos.Row && curNode.Col == pos.Col) == -1)
            break;
            case PaintType.ERASER: 
                this._state.ItemDict[token].ActiveOrigin = 
                    this._state.ItemDict[token].ActiveOrigin.filter(pos => nodes.findIndex(curNode => curNode.Row === pos.Row && curNode.Col == pos.Col) == -1)

                this._state.ItemDict[token].NodeOrigin = 
                    this._state.ItemDict[token].NodeOrigin.filter(pos => nodes.findIndex(curNode => curNode.Row === pos.Row && curNode.Col == pos.Col) == -1)
            break; 
        } 
        return this.SetState("PaintItem", [token, nodes.toString(), type.toString()], this._state); 
    }

    // EraseNode = (token:string, nodes:GridPosition[]) => {
    //     if(!token || token == "" || !Object.prototype.hasOwnProperty.call(this._state.ItemDict, token)) 
    //         return null;


    //     return this.SetState("EraseNode", [token, nodes.toString()], this._state); 
    // }
    
    SetMaxStackCount = (token:string, count:number) => {
        if(!token || token == "" || !Object.prototype.hasOwnProperty.call(this._state.ItemDict, token)) 
            return null;

        this._state.ItemDict[token].MaxStackCount = count; 
        return this.SetState("SetMaxStackCount", [token, count.toString()], this._state); 
    }

    SetTitle = (token:string, title:string) => {
        if(!token || token == "" || !Object.prototype.hasOwnProperty.call(this._state.ItemDict, token)) 
            return null;

        this._state.ItemDict[token].Label = title; 
        return this.SetState("SetTitle", [token, title], this._state); 
    }

    SetToken = (oldToken:string, newToken:string) => {
        if(!oldToken || oldToken == "" || !Object.prototype.hasOwnProperty.call(this._state.ItemDict, oldToken)) 
            return null;
        if(!newToken || newToken == "" || Object.prototype.hasOwnProperty.call(this._state.ItemDict, newToken)) 
            return null;

        this._state.ItemDict[newToken] = this._state.ItemDict[oldToken]; 
        this._state.ItemDict[newToken].Token = newToken; 
        delete(this._state.ItemDict[oldToken]); 

        if(this._state.SelectedItemToken == oldToken) this._state.SelectedItemToken = newToken; 
        
        return this.SetState("SetToken", [oldToken, newToken], this._state); 
    }

    /* HISTORY */
    private SetState = (label:string, args:string[], state:DataModelState) => {
        this._state = JSON.parse(JSON.stringify(state));
        this._state.UpdateTimestamp = Date.now();  

        this._history = this._history.slice(0, this._historyIndex + 1)
        this._history.push({
            Label: label, 
            Args: args.join(" "), 
            State: JSON.parse(JSON.stringify(state))
        })

        console.log(this._history, this._historyIndex)

        this._historyIndex = this._history.length - 1; 
        this.InvokeListeners(); 
        return this._state; 
    }

    Subscribe = (callback:()=>void) => {
        this._listeners.add(callback); 

        return () => {
            this._listeners.delete(callback); 
        }
    }

    private InvokeListeners = () => {
        this._listeners.forEach(l => l());
    }


    StepAction = (dir:'forward'|'backward', step:number=1) => {
        if(this._historyIndex - step < 0 && dir == 'backward' || this._historyIndex + step >= this._history.length && dir == 'forward')
            return null; 
        
        console.log("Stepping: ", dir, step)
        step = Math.abs(step); 
        const curAction = this._history[this._historyIndex]; 
        if(dir == "forward") 
            this._historyIndex = Math.min(this._history.length - 1, this._historyIndex + step); 
        else 
            this._historyIndex = Math.max(0, this._historyIndex - step); 
        this._state = this._history[this._historyIndex].State; 

        this.InvokeListeners(); 
        return curAction;
    }

    GetHistory = () => {
        return {
            history: this._history, 
            historyIndex: this._historyIndex
        }; 
    }    

    GetSnapshot = () => {
        return this._state; 
    }
}