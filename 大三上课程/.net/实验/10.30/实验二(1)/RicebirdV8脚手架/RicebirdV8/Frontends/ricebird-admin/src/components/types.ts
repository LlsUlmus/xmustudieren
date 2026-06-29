import { VNode } from "vue";
interface DictEntry {
    DataKey: string,
    DataValue: string,
    Enable?: boolean, // 启用性，不写为true
    Visible?: boolean, // 可见性，不写为true
    label?: VNode, // 可以不写
    Group?: string, // 可以不写，这个字段只要在数组里有任何一个就会使选择器分组。
}

interface Dict {
    name: string,
    entries: Array<DictEntry>
}

export type { Dict }