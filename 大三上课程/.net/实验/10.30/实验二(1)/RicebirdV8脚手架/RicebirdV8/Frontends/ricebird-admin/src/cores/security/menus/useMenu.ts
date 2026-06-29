import axios from '@/axios'
import createTreeDataSource from '@/utils/DataSource'

let {
    tree,
    flatTree,
    dataSource,
    loadTree,
    createDataSource
} = createTreeDataSource(loader);

async function loader () {
    let msg = await axios.post("/api/menu/GetMenuItems");
    return msg.data;
}

// await loadTree();

export {
    loadTree,
    dataSource,
    tree,
    flatTree,
    createDataSource
}