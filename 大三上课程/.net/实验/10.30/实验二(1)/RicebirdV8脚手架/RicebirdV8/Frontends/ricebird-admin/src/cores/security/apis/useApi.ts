import axios from '@/axios'
import lo from 'lodash'
import createTreeDataSource from '@/utils/DataSource'
import type { DataDictionary } from '@/utils/DataDictionary'
import { reactive, watch } from 'vue';
let {
    tree,
    flatTree,
    dataSource,
    loadTree,
    createDataSource
} = createTreeDataSource(loader);

let apiGroups : DataDictionary = reactive({
    name: '所在组别',
    entries: [
        {
            DataKey: "",
            DataValue: "所有分组"
        }
    ]
});

async function loader () {
    let msg = await axios.post("/api/viewer/GetApiList");
    return msg.data;
}

// await loadTree();

watch(dataSource, nv => {
    apiGroups.entries = [
        {
            DataKey: "",
            DataValue: "所有分组",
        }
    ];
    let obj : any = {};
    for (let item of flatTree) {
        obj[item.ApiGroup] = item.Module;
    }

    for (let key in obj) {
        if (!key) continue;        
        apiGroups.entries.push({
            DataKey: key,
            DataValue: key,
            Group: obj[key],
        });
    }
}, { deep: true });

export {
    loadTree,
    apiGroups,
    dataSource,
    createDataSource
}