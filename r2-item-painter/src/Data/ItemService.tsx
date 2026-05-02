import Fuse from 'fuse.js'; 
import { ContentGroupType, ItemTier } from '.';

type ItemDetail  = {Token:string, IconFileName: string, BaseLabel: string, Label: string, Tier:ItemTier, IsEquip:boolean, ContentGroup: ContentGroupType, 
    IsFood:boolean, IsConsumed:boolean
}

export class ItemService {
    static ICON_PATH = "./ItemIcons"
    static ItemDetails:ItemDetail[]; 
    static FuzzySearch:Fuse<ItemDetail>; 


    static async Init() {
        const resp = await (await fetch('/data/data.json')).json();
        this.ItemDetails = resp['ItemDetails']; 
        this.FuzzySearch = new Fuse(this.ItemDetails, {
            keys: ['Token', 'BaseLabel', 'Label'], 
            includeScore: true, 
        }); 


    }

    static Dispose() {

    }

    static IsAlreadyItem(token:string):boolean {
        return this.GetItemDetails(token) != undefined; 
    }

    static SearchItem(searchStr:string, limit:number=5):string[] {
        const resp = this.FuzzySearch.search(searchStr, {limit: limit}); 
        return resp.map(e => e.item.Token); 
    }

    static GetItemDetails(token:string):ItemDetail|undefined {
        if(this.ItemDetails)
            return this.ItemDetails.find(det => det.Token == token); 
        return undefined; 
    }

    static GetItemIconPath = (token: string): string => {
        // console.log(token)
        const target = this.GetItemDetails(token); 
        // console.log(target)
        if(target) {
            // console.log(target[0].IconFileName)
            return [ItemService.ICON_PATH, target.IconFileName].join("/")
        }

        return [ItemService.ICON_PATH, "Question_Mark.webp"].join("/")
    }

    static GetItemLabel = (token: string): string | null => {
        const target = this.GetItemDetails(token); 
        // const target = this.ItemMap[token]; 
        if(target)
            return target.Label
        return null; 
    }
}
