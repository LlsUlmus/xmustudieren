import lo from 'lodash'
import { reactive, ref, watch } from 'vue'
import type { Data, TreeNode, DataSource, Queryable } from './utils'
import createQuery from './Queryable';

function flat (nodes : TreeNode[], result : TreeNode[]) : TreeNode[] {
    for (let item of nodes) {
        let clone = lo.clone(item);
        delete clone.children
        result.push(clone);
        if (item.children) {
            flat(item.children, result);
        }
    }
    return result;
}
/**
 * 生成一个用以缓存的树结构数据源
 * @param loader 一个异步函数，返回值是需要缓存的数据
 * @returns 数据源
 */
export default function createTreeDataSource ( loader: (force: boolean) => Promise<TreeNode[]> ) {
    let id = 0;
    let tree : Array<TreeNode> = [];
    let flatTree : Array<TreeNode> = [];
    let currentVersion = ref(0);
    
    function createDataSource (defaultFlat : Boolean, complete? : (data : Array<Data>, merge? : boolean) => void) : DataSource<TreeNode> {
        let query : Queryable<TreeNode> = createQuery(flatTree, function () {});
        let dataSource : DataSource<TreeNode> = reactive<DataSource<TreeNode>>({
            id: id,
            data: [],
            query: () => {
                query.restart();
                return query;
            },
            ver: 0,
        });
        id++;

        dataSource.data = defaultFlat ? flatTree : tree;

        query = createQuery(flatTree, arr => {
            let hasQuery = arr && arr.length !== flatTree.length;
            let merging = !defaultFlat && !hasQuery;
            dataSource.data = merging ? tree : arr;
            if (typeof complete === 'function') {
                complete(dataSource.data, merging);
            }
        });

        watch(currentVersion, () => {
            query.execute(flatTree);
            dataSource.ver++;
        });

        return dataSource;
    }

    let dataSource = createDataSource(false);

    async function loadTree (force : boolean = false) {
        if (force || !tree.length) {
            tree = await loader(force);
            flatTree.splice(0, flatTree.length);
            flat(tree, flatTree);            
            currentVersion.value++;
        }
        return tree;
    }

    loadTree();

    return {
        tree,
        flatTree,
        dataSource,
        createDataSource,
        loadTree
    }
}

/**
 * 生成一个用以缓存的普通数据源
 * @param loader 一个异步函数，返回值是需要缓存的数据
 * @returns 数据源
 */
export function createNormalDataSource ( loader: (force: boolean) => Promise<any[]> ) {
    let id = 0;
    let data : any[] = [];
    let currentVersion = ref(0);
    
    function createDataSource (complete? : (data : Array<Data>, merge? : boolean) => void) : DataSource<TreeNode> {
        let query : Queryable<TreeNode> = createQuery(data, function () {});
        let dataSource : DataSource<TreeNode> = reactive<DataSource<TreeNode>>({
            id: id,
            data: [],
            query: () => {
                query.restart();
                return query;
            },
            ver: 0,
        });
        id++;

        dataSource.data = data;

        query = createQuery(data, arr => {
            dataSource.data = arr;
            if (typeof complete === 'function') {
                complete(dataSource.data, false);
            }
        });

        watch(currentVersion, () => {
            query.execute(data);
            dataSource.ver++;
        });

        return dataSource;
    }

    let dataSource = createDataSource();

    async function loadData (force : boolean = false) {
        if (force || !data.length) {
            data = await loader(force);      
            currentVersion.value++;
        }
        return data;
    }

    function removeId (id : string) {
        var index = data.findIndex(e => e.ID === id);
        if (index > -1) {
            data.splice(index, 1);
        }

        currentVersion.value++;
    }

    loadData();

    return {
        data,
        dataSource,
        createDataSource,
        loadData,
        removeId
    }
}