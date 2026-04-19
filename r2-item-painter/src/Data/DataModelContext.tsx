import { createContext, useContext, useSyncExternalStore } from "react";
import { DataModel } from "./DataModel";


const DataModelContext = createContext<DataModel | null>(null); 

// eslint-disable-next-line react-refresh/only-export-components
export const useDataModelContext = () => {
    const model = useContext(DataModelContext); 

    if(!model) throw new Error("Wrap with DataModelProvider"); 

    return model; 
}

// eslint-disable-next-line react-refresh/only-export-components
export const useDataModelState = () => {
    const dataModel = useDataModelContext(); 
    const modelState = useSyncExternalStore(dataModel.Subscribe, dataModel.GetSnapshot); 

    return {
        modelState, 
        dataModel
    }
}

const dataModel = new DataModel(); 

export const DataModelProvider = ({children}:{children:React.ReactNode}) => {
    return <DataModelContext.Provider value={dataModel}>
        {children}
    </DataModelContext.Provider>
}