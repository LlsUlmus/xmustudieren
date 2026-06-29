<template>
    <div id="dict-tree"  class="ricebird-tree-section">
        <div class="tree-search-area">
            <h1 class="header">角色管理</h1>
            <div class="action-btn-area">
                <a-tooltip title="添加角色">
                    <div class="action-btn" title="新建" @click="onEdit(app.GUID_EMPTY, '')">
                        <plus-outlined />
                    </div>
                </a-tooltip>
                <a-tooltip title="重新加载角色">
                    <div class="action-btn" title="刷新" :class="{ 'on-loading': loading }" @click="reSync">
                        <sync-outlined />
                    </div>
                </a-tooltip>
            </div>
        </div>
        <div class="tree-search-area no-pt">
            <a-input v-model:value="keyword" @change="onSearchChange" placeholder="搜索：角色名称" />
        </div>
        <div class="ricebird-tree-area">
            <a-tree show-icon :tree-data="dataSource.data" v-model:selectedKeys="selectedKeys" :autoExpandParent="false">
                <template #switcherIcon="{ switcherCls }">
                    <a-icon icon="down-outlined" :class="switcherCls" />
                </template>
                <template #title="{ title, data }">
                    <span class="text-cut title">{{ title }}</span>
                    <div class="btn-area">
                    </div>
                </template>
                <template #icon="{ data }">
                    <a-icon icon="TeamOutlined" />
                </template>
            </a-tree>
        </div>
    </div>
</template>

<script setup>
import app from '@/app';
import { ref, inject, watch } from 'vue'
import { loadTree, dataSource, tree, } from './useRole'

let keyword = ref('');
let loading = ref(false);
let selectedKey = inject("selectedKey");
let selectedKeys = ref([app.GUID_EMPTY]);

// watch(selectedKeys, nv => {
//     let id = nv.length ? nv[0] : app.GUID_EMPTY;
//     selectedKey.value = id;
// });
watch(selectedKeys, nv => {
    if (!nv.length) selectedKeys.value.push(app.GUID_EMPTY);
    let id = nv.length ? nv[0] : app.GUID_EMPTY;
    selectedKey.value = id;
});

function onSearchChange () {
    dataSource.query()
        .whereIf("title", keyword)
        .end()
}

function reSync () {
    loadTree(true);
}

// -- useDict Start -- //

// -- useDict End -- //
</script>

<style lang="less">
</style>