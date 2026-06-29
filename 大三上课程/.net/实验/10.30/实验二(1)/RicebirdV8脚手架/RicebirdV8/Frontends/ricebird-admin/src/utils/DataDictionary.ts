import { VNode, computed, ComputedRef } from "vue"

export interface Entry {
    DataKey: string,
    DataValue: string,
    Group?: string,
    label?: string | VNode,    
}

export interface DataDictionary {
    name: string,
    entries: Entry[]
}

export function buildDictionary (dictName : string, empty : Entry, entries : string[]) : ComputedRef {
    return computed(() => {
        let dict : DataDictionary = {
            name: dictName,
            entries: [
                empty
            ]
        };
    
        for (let item of entries) {
            let entry : Entry = {
                DataKey: item,
                DataValue: item
            }
            dict.entries.push(entry)
        }
    
        return dict;
    });
}

// export function buildDictionary (dictName : string, empty : Entry, entries : Entry[]) : ComputedRef {
//     return computed(() => {
//         let dict : DataDictionary = {
//             name: dictName,
//             entries: [
//                 empty
//             ]
//         };
    
//         for (let item of entries) {
//             dict.entries.push(item)
//         }
    
//         return dict;
//     });
// }