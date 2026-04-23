import {expect, test} from '@jest/globals';
import { DataModel } from "./DataModel";
// import type { DataModelState } from '.';


test('Load Success', () => {
    const model = new DataModel(); 
    const resp = model.LoadState('{"SelectedItemToken": null, "ItemDict": {}}'); 
    expect(resp).not.toBeNull(); 
});

test('Load Failed', () => {
    const model = new DataModel(); 
    const resp = model.LoadState('"123"'); 
    expect(resp).toBeNull(); 
}); 


// test('Item Add and Item Remove', () => {
//     const model = new DataModel(); 
//     const tokens = ["ITEM_SOMEITEM1_NAME", "ITEM_SOMEITEM2_NAME", "ITEM_SOMEITEM3_NAME"]; 
//     tokens.forEach(token => model.AddItem(token)); 

//     let state:DataModelState | null = model.GetCurrentState(); 
//     expect(state.ItemDict).not.toBeNull(); 
//     expect(Object.keys(state.ItemDict).length).toBe(tokens.length); 

//     state = model.RemoveItem(tokens[0]); 
//     expect(state?.ItemDict).not.toBeNull(); 
//     expect(Object.keys((state as DataModelState).ItemDict).length).toBe(tokens.length - 1); 
// }); 

