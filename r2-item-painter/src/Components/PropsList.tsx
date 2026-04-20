import React from "react";
import { useDataModelState } from "../Data/DataModelContext"
import { Input } from "./Components"
import { ItemService } from "../Data/ItemService";

type InputProps = React.PropsWithChildren<React.ComponentPropsWithRef<'input'>>;

export function PropsList() {
    const {modelState, dataModel} = useDataModelState(); 
    const labelRef = React.useRef<HTMLInputElement>(null!); 
    const tokenRef = React.useRef<HTMLInputElement>(null!); 
    const maxCountRef = React.useRef<HTMLInputElement>(null!); 

    React.useEffect(() => {
        if(modelState.SelectedItemToken && modelState.ItemDict) {
            labelRef.current.value = modelState.ItemDict[modelState.SelectedItemToken].Label; 
            tokenRef.current.value = modelState.ItemDict[modelState.SelectedItemToken].Token; 
            maxCountRef.current.value = modelState.ItemDict[modelState.SelectedItemToken].MaxStackCount.toString(); 
        }

    }, [modelState]); 

    const IsAlreadyItem = React.useMemo(() => {
        if(modelState.SelectedItemToken)
            return ItemService.IsAlreadyItem(modelState.SelectedItemToken); 
        return true; 
    }, [modelState.SelectedItemToken])


    const HandleLabelInputBlur = () => {
        if(modelState.SelectedItemToken && labelRef.current && labelRef.current.value.trim() != "")
            dataModel.SetTitle(modelState.SelectedItemToken, labelRef.current.value.trim())
    }

    const HandleTokenInputBlur = () => {
        if(modelState.SelectedItemToken && tokenRef.current && tokenRef.current.value.trim() != "")
            dataModel.SetToken(modelState.SelectedItemToken, tokenRef.current.value.trim())
    }

    const HandleCountInputBlur = () => {
        if(modelState.SelectedItemToken && maxCountRef.current && maxCountRef.current.valueAsNumber != 0) 
            dataModel.SetMaxStackCount(modelState.SelectedItemToken, maxCountRef.current.valueAsNumber)
    }

    return <div className={styles._}>

        <div className={styles.header._}>
            <h2>Properties</h2>
        </div>
        {
            modelState.SelectedItemToken ? 
            <div className={styles.propList}>
                <PropItem disabled={IsAlreadyItem} title="Item Label" type="text" ref={labelRef} onBlur={HandleLabelInputBlur}/>
                <PropItem disabled title="Item Token" type="text" ref={tokenRef} onBlur={HandleTokenInputBlur}/>
                <PropItem title="Max Stack Count" type="number" ref={maxCountRef} onBlur={HandleCountInputBlur}/>                
            </div> :  <>No Item Selected</>
        }
    </div>
}

export function PropItem({title, className, ...rest}:{title:string} & InputProps) {
    return <div  className={`${styles.propItem._} ${className}`}>
        <p>{title}</p>
        <Input {...rest} />
    </div>
}

const styles = {
    _: `border flex flex-col`, 
    header: {
        _: `border p-2`, 
    },
    propList: `p-2 grid`, 
    propItem:{
        _:`grid grid-cols-1 grid-rows-2`
    }
}