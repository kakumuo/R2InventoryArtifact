export class ItemService {
    static ICON_PATH = "/src/Assets/ItemIcons"

    static GetItemIconPath = (token:string):string => {
        switch(token) {
            case "ITEM_TEST_NAME": return  [ItemService.ICON_PATH, "Aegis.webp"].join("/") 
            default: return  [ItemService.ICON_PATH, "Question_Mark.webp"].join("/") 
        }
    }

    static GetItemLabel = (token:string):string|null => {
        switch(token) {
            case "ITEM_TEST_NAME": return "Test Item";
            default: return null; 
        }
    }
}
