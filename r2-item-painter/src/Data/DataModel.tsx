import type { DataModelAction, DataModelState, GridPosition, Item } from "./types.d";

export class DataModel {
    
    private _state:DataModelState; 
    private _history:DataModelAction[]; 
    private _historyIndex:number; 

    constructor(){
        this._state = {} as DataModelState; 
        this._history = [];
        this._historyIndex = 0; 

        this.SetState({}); 
    }

    public LoadData(data:string):DataModelState|null{
        let jsonData = {};
        try {
            jsonData = JSON.parse(data);
            this.SetState(jsonData as DataModelState); 
        }catch(e) {
            console.log(e);
            return null;  
        }
        return this._state; 
    }

    /* ITEM LIST */
    public AddItem(token:string):DataModelState|null{
        if(token == "" || Object.hasOwn(this._state.ItemDict, token)) 
            return null;

        const item:Item = {
            Label: "", 
            Token: token, 
            MaxStackCount: 5, 
            NodeOrigin: [], 
            ActiveOrigin: [], 
        }

        this._state[token] = item; 
        return this.SetState(this._state); 
    }
    public RemoveItem(token:string){
        if(!token || token == "" || !Object.hasOwn(this._state, token)) 
            return null;
        
        delete(this._state[token]); 
        return this.SetState(this._state); 
    }

    /* ITEM PROPS */
    public PaintItem(token:string, nodes:GridPosition[], type:'base'|'active'){
        if(!token || token == "" || !Object.hasOwn(this._state, token)) 
            return null;

        switch(type) {
            case 'base': 
                this._state[token].NodeOrigin = [...new Set([...this._state[token].NodeOrigin, ...nodes])];  
            break; 
            case 'active':
                this._state[token].ActiveOrigin = [...new Set([...this._state[token].ActiveOrigin, ...nodes])];  
            break; 
        } 
        return this.SetState(this._state); 
    }

    public EraseNode(token:string, nodes:GridPosition[]){
        if(!token || token == "" || !Object.hasOwn(this._state, token)) 
            return null;

        const activeNodes = this._state[token].ActiveOrigin; 
        const baseNodes = this._state[token].NodeOrigin; 

        this._state[token].NodeOrigin = baseNodes.filter(pos => nodes.findIndex(nodePos => nodePos.Row == pos.Row && nodePos.Col == pos.Col) != -1); 
        this._state[token].ActiveOrigin = activeNodes.filter(pos => nodes.findIndex(nodePos => nodePos.Row == pos.Row && nodePos.Col == pos.Col) != -1); 

        return this.SetState(this._state); 
    }
    
    public SetMaxStackCount(token:string, count:number){
        if(!token || token == "" || !Object.hasOwn(this._state, token)) 
            return null;

        this._state[token].MaxStackCount = count; 
        return this.SetState(this._state); 
    }

    public SetTitle(token:string, title:string){
        if(!token || token == "" || !Object.hasOwn(this._state, token)) 
            return null;

        this._state[token].Label = title; 
        return this.SetState(this._state); 
    }

    public SetToken(oldToken:string, newToken:string){
        if(!oldToken || oldToken == "" || !Object.hasOwn(this._state, oldToken)) 
            return null;
        if(!newToken || newToken == "" || Object.hasOwn(this._state, newToken)) 
            return null;

        this._state[newToken] = this._state[oldToken]; 
        this._state[newToken].Token = newToken; 
        delete(this._state[oldToken]); 
        
        return this.SetState(this._state); 
    }

    /* HISTORY */
    private SetState(state:DataModelState){
        this._state = state; 
        this._history.push({
            Label: this.SetState.caller.name, 
            Args:this.SetState.caller.arguments, 
            State: JSON.parse(JSON.stringify(state))
        })

        this._historyIndex += 1; 

        return this._state; 
    }

    public UndoAction(step:number=1){
        this._historyIndex = Math.max(0, this._historyIndex-step); 
        return this._history[this._historyIndex]; 
    }

    public RedoAction(step:number=1){
        this._historyIndex = Math.min(this._history.length, this._historyIndex + step); 
        return this._history[this._historyIndex]; 
    }

    public GetCurrentState(){
        return this._state; 
    }
}