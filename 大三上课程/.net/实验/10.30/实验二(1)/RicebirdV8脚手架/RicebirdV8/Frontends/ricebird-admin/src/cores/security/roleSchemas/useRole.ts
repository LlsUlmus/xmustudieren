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

async function loader () {
    let msg = await axios.post("/api/roles/GetRoleSchemas");
    return msg.data;
}

function getRole (id : string) {
    return id === empty ? { title: "所有部门", setFor: 0 } : flatTree.find(e => e.key === id);
}

// await loadTree();

export { loadTree, dataSource, tree, createDataSource, flatTree, getRole }