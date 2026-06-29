import axios from '@/axios'
import { message } from 'ant-design-vue';
import createTreeDataSource from '@/utils/DataSource'

const GUID_EMPTY = "00000000-0000-0000-0000-000000000000";

let {
    tree,
    flatTree,
    dataSource,
    loadTree
} = createTreeDataSource(loader);

async function loader(force : boolean = false) {
    let msg = await axios.post("/api/dict/GetDictionaries", { force });
    let data = [];
    for (let ele of (msg.data || [])) {
        data.push({
            key: ele.ID,
            title: ele.Name,
            ...ele
        });
    }
    return data;
}

// interface TreeNode {
//     key: string,
//     title: string,
//     Entries?: Array<DictionaryEntry>
//     [prop: string]: any
// }

export interface DictionaryEntry {
    "DataKey": string,
    "DataValue": string,
    "CanEdit": boolean,
    "CanDelete": boolean,
    "Visible": boolean,
    "Enable": boolean,
    "DisplayOrder": number,
}

// -- 处理字典 -- //
export default function toText(dict: string, key: string | undefined): string {
    if (key === undefined) return "";
    let ele = flatTree.find(e => e.title === dict);
    if (ele && ele.Entries && ele.Entries.length) {
        let entry = ele.Entries.find((x: any) => key.toString && x.DataKey === key.toString());
        return entry ? entry.DataValue : "数据项不存在";
    }
    return "字典不存在";
}

async function ensureDictionary(dict: string, filter?: string): Promise<{ name: string, canEdit: boolean, canDelete: boolean, entries: Array<DictionaryEntry> }> {
    await loadTree();
    return getDictionary(dict, filter);
}

function getDictionaryById (dict: string) : string {
    if (!flatTree.length) return GUID_EMPTY;
    let entry = flatTree.find(e => e.Name === dict);
    if (entry) return entry.ID;
    return GUID_EMPTY;
}

function getDictionary(dict: string, filter?: string): { name: string, type: string, canEdit: boolean, canDelete: boolean, entries: Array<DictionaryEntry> } {
    interface internal { Name: string, From: number, CanEdit: boolean, CanDelete: boolean, Entries: Array<DictionaryEntry> };
    if (!flatTree.length) return { name: "", type: "string", canDelete: false, canEdit: false, entries: [] }
    let ele: internal = flatTree.find(e => e.title === dict || e.key === dict) as unknown as internal;
    let entries = ele && ele.Entries ? ele.Entries.filter(e => !filter || e.DataKey.indexOf(filter) >= 0 || e.DataValue.indexOf(filter) >= 0) : [];
    return {
        name: ele.Name,
        type: ele.From === 0 ? "number" : "string",
        canEdit: ele.CanEdit,
        canDelete: ele.CanDelete,
        entries
    }
}

async function RemoveDictionary(id: string) {
    let msg = await axios.post<any, { success: boolean, msg: string }>("/api/dict/RemoveDictionary", { id });
    if (msg.success) {
        message.success("删除成功");
    } else {
        message.error(msg.msg);
    }
    await loadTree();
}

export {
    loadTree as loadDictionary,
    getDictionary,
    dataSource,
    ensureDictionary,
    RemoveDictionary,
    getDictionaryById,
}