import React from "react"
import { ItemService } from "../Data/ItemService";

type ButtonProps = React.PropsWithChildren<React.ComponentPropsWithRef<'button'>>;
type InputProps = React.PropsWithChildren<React.ComponentPropsWithRef<'input'>>;

export function Button({className, ...rest}:ButtonProps) {
    return <button className={`${style.button._} ${className}`} {...rest} />
}

export function Input({className, ...rest}:InputProps) {
    return <input className={`${style.input._} ${className}`} {...rest} />
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function Search({className, ref, type, OnItemSelected, ...rest}:InputProps & {OnItemSelected:(token:string)=>void, ref: React.RefObject<HTMLInputElement>}) {
    const SEARCH_ITEM_LIMIT = 5; 
    const [showResults, setShowResults] = React.useState(false); 
    const [searchText, setSearchText] = React.useState(""); 

    const HandleSearchResultSelected = (token:string) => {
        OnItemSelected(token); 
        setShowResults(false)
    }

    React.useEffect(() => {
        const HandleBlur = (ev:PointerEvent) => {
            if(ev.currentTarget && ref.current && !ref.current.contains(ev.target as HTMLDivElement))  {
                setShowResults(false); 
            }
        }; 

        window.addEventListener('click', HandleBlur); 
        return () => {
            window.removeEventListener('click', HandleBlur); 
        }
    }, [ref.current]); 

    const searchResults = React.useMemo(() => {
        if(searchText.trim() != "") {
            const items = ItemService.SearchItem(searchText.trim(), SEARCH_ITEM_LIMIT); 
            return items; 
        }
        return []
    }, [searchText]); 

    const resElements = React.useMemo(() => {
        const res:React.JSX.Element[] = []; 

        for(const token of searchResults) {
            const icon = ItemService.GetItemIconPath(token); 
            const label = ItemService.GetItemLabel(token)!; 
            res.push(<SearchResult key={`search-result-${token}`} imagePath={icon} itemLabel={label} itemToken={token} OnItemSelected={HandleSearchResultSelected} />)
        }

        return res; 
    }, [searchResults]); 

    return <div className={style.search._}>
        <Input ref={ref} type="search" onChange={e => setSearchText(e.currentTarget.value)} className={`${style.search.input} ${className}`} {...rest} onFocus={() => setShowResults(true)} />
        {showResults && resElements.length > 0 && 
            <div className={style.search.resultList}>
                {resElements}
            </div>
        }
    </div>
}

function SearchResult(props:{itemLabel:string, itemToken:string, imagePath:string, OnItemSelected:(token:string)=>void}) {
    return <button className={style.search.result._} onClick={() => props.OnItemSelected(props.itemToken)}>
        <img src={props.imagePath} className={style.search.result.img}/>
        <p className={style.search.result.title}>{props.itemLabel}</p>
    </button>
}

const style = {
    search: {
        _: `w-full grid relative`, 
        input:`w-full`, 
        resultList: `w-full grid grid-cols-1 grid-rows-auto overflow-y-scroll absolute top-full bg-white gap-2 p-2`, 
        result: {
            _: `hover:bg-gray-200 active:bg-gray-100 grid grid-cols-3 grid-rows-1 items-center p-2 cursor-pointer`, 
            title: ``,
            img: `w-8 h-8`, 
        }
    },

    button: {
        _: `
        border 
        px-4 py-2
        hover:bg-gray-200
        hover:cursor-pointer
        active:bg-gray-100
        `
    }, 
    
    input: {
        _: `
            border 
            disabled:cursor-not-allowed
            disabled:bg-gray-100
            `, 
    }
}