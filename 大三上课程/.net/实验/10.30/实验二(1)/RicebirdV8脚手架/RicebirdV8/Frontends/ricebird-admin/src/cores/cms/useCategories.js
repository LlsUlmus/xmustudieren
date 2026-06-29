import { ref, reactive, watch, computed } from 'vue';
import axios from '@/axios'
const LOADING_STATE = "loading";
const NORMAL_STATE = "normal";
const guidEmpty = '00000000-0000-0000-0000-000000000000';

let categoryTree = {
    allCategories: []
};
let loadingState = ref(NORMAL_STATE);
const hasHome = ref(true);

function wait(ms) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            resolve(ms)
        }, ms)
    })
}

async function loadCategoryTree(force) {
    do {
        if (!force) {
            let tree = switchCategory();
            if (tree) {
                return tree;
            }
        }
        await wait(100);
    } while (loadingState.value === LOADING_STATE);

    loadingState.value = LOADING_STATE;
    let msg = await axios.post("/api/cms/category/GetCategoryTree", { force });
    if (msg.success) {
        categoryTree.allCategories = msg.data;
        hasHome.value = msg.hasHome;
    }

    loadingState.value = NORMAL_STATE;
    return switchCategory();
}

function switchCategory() {
    return categoryTree.allCategories.length ? categoryTree.allCategories : false;
}

function searchTree (tree, withChildren, callback) {
    for (let node of tree) {
        let isCatch = callback(node);
        // 匹配到文字之后，就不再向子级匹配，直接把当前级别返回即可 @ 黄玺 2021-7-2
        if (!isCatch && withChildren && node.children && node.children.length) {
            searchTree(node.children, withChildren, callback);
        }
    }
}

async function searchCategory (keyword, force) {
    let tree = await loadCategoryTree(force);
    let finalNodes = [];
    let searchChildren = true;
    searchTree(tree, searchChildren, node => {
        if (node.title.indexOf(keyword) > -1) {
            let returnNode = node;
            finalNodes.push(returnNode);
            return true;
        }
        return false;
    });

    return finalNodes;
}

async function deleteCategory (id, cate, keyword) {
    await axios.post("/api/cms/category/RemoveCategory", { id });
    return await searchDepart(cate, keyword, true);
}

async function loadCategoryByCache (id) {
    let tree = await loadCategoryTree(false);
    let finalNodes = [];
    let searchChildren = true;
    searchTree(tree, searchChildren, node => {
        if (node.key === id) {
            let returnNode = node;
            finalNodes.push(returnNode);
            return true;
        }
        return false;
    });

    return finalNodes.length ? finalNodes[0] : {};
}

async function loadCategory (id) {
    let data = await axios.post("/api/cms/category/GetCategory", { id });
    return data;
}

export {
    NORMAL_STATE,
    LOADING_STATE,
    categoryTree,
    loadingState,

    hasHome,

    loadCategoryTree,
    searchCategory,
    deleteCategory,
    loadCategory,
    loadCategoryByCache
}