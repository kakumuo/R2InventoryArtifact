import { createContext, useContext, useSyncExternalStore } from "react";
import { DataModel } from "./DataModel";
import React from "react";


const DataModelContext = createContext<DataModel | null>(null); 
const dataModel = new DataModel(); 
const LOCAL_STORAGE_KEY_MODEL_STATE = 'R2_ITEM_PAINTER_MODEL_STATE'; 
const LOCAL_STORAGE_KEY_HISTORY = 'R2_ITEM_PAINTER_HISTORY'; 

// eslint-disable-next-line react-refresh/only-export-components
export const useDataModelContext = () => {
    const dataModel = useContext(DataModelContext); 
    const [isInitialized, setIsInitialized] = React.useState(false); 

    React.useEffect(() => {
        if(!isInitialized) {
            setIsInitialized(true); 
            const target = localStorage.getItem(LOCAL_STORAGE_KEY_MODEL_STATE); 
            const history = localStorage.getItem(LOCAL_STORAGE_KEY_HISTORY); 
            if(target && dataModel) {
               dataModel.LoadState(target) 
            }

            if(history && dataModel) {
                dataModel.LoadHistory(history)
            }
        }
    }, [dataModel, isInitialized])

    if(!dataModel) throw new Error("Wrap with DataModelProvider"); 

    return {dataModel, isInitialized}; 
}

// eslint-disable-next-line react-refresh/only-export-components
export const useDataModelState = () => {
    const {dataModel, isInitialized} = useDataModelContext(); 
    const modelState = useSyncExternalStore(dataModel.Subscribe, dataModel.GetSnapshot); 

    // write to local storage
    React.useEffect(() => {
        if(isInitialized) localStorage.setItem(LOCAL_STORAGE_KEY_MODEL_STATE, JSON.stringify(modelState)); 
        if(isInitialized) localStorage.setItem(LOCAL_STORAGE_KEY_HISTORY, JSON.stringify(dataModel.GetHistory())); 
        
        // console.log(dataModel.GetHistory())
    }, [modelState, isInitialized])

    return {
        modelState, 
        dataModel
    }
}

export const DataModelProvider = ({children}:{children:React.ReactNode}) => {
    return <DataModelContext.Provider value={dataModel}>
        {children}
    </DataModelContext.Provider>
}