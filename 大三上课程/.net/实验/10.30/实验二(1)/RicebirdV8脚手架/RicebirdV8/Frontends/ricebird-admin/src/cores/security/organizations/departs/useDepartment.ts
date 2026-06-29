import axios from '@/axios'
import createTreeDataSource from '@/utils/DataSource'

const empty = "00000000-0000-0000-0000-000000000000";
let {
    tree,
    flatTree,
    dataSource,
    loadTree,
    createDataSource
} = createTreeDataSource(loader);

async function loader (force : boolean) {
    let msg = await axios.post<any, any>("/api/depart/GetDepartTree", { force });
    (msg.tree as any[]).unshift({
        key: empty,
        title: '所有部门',
        SchemaName: "",
    });
    return msg.tree;
}

function getDepart (id : string) {
    return id === empty ? { title: "所有部门" } : flatTree.find(e => e.key === id);
}

export { loadTree, dataSource, flatTree, getDepart, createDataSource }