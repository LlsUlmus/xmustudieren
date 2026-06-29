<template>
    <div id="dict-tree"  class="ricebird-tree-section">
        <div class="tree-search-area">
            <h1 class="header">{{ props.title }}</h1>
            <div class="action-btn-area">
                <a-tooltip title="重新加载部门">
                    <div class="action-btn" title="刷新" :class="{ 'on-loading': loading }" @click="reSync(true)">
                        <sync-outlined />
                    </div>
                </a-tooltip>
            </div>
        </div>
        <div class="tree-search-area no-pt">
            <a-input-group compact>
                <a-select v-model:value="category" @change="onSearchChange">
                    <a-select-option value="">所有部门</a-select-option>
                    <!-- <a-select-option value="普通部门">普通部门</a-select-option> -->
                    <a-select-option value="学院">学院</a-select-option>
                </a-select>
                <a-input v-model:value="keyword" @change="onSearchChange" placeholder="搜索：部门名称" allow-clear style="width: 65%" />
            </a-input-group>
        </div>
        <div class="ricebird-tree-area">
            <a-tree show-icon :tree-data="dataSource.data" v-model:selectedKeys="selectedKeys" :autoExpandParent="false">
                <template #switcherIcon="{ switcherCls }">
                    <a-icon icon="down-outlined" :class="switcherCls" />
                </template>
                <template #title="{ title, data }">
                    <!-- data.key === app.GUID_EMPTY ? props.empty : title -->
                    <span class="text-cut title" :title="data.key === app.GUID_EMPTY ? props.empty : title">{{ data.key === app.GUID_EMPTY ? props.empty : title }}</span>
                    <div class="btn-area show-text" v-if="data.IsDefault">
                        [默认]
                    </div>
                </template>
                <template #icon="{ data }">
                    <a-icon icon="ApartmentOutlined" />
                </template>
            </a-tree>
        </div>
    </div>
</template>

<script setup>
import app from '@/app';
import { ref, inject, watch } from 'vue'
import { loadTree, dataSource, flatTree } from './useDepartment'

let keyword = ref('');
let category = ref('');
let loading = ref(false);
let selectedKey = inject("selectedKey");
let selectedKeys = ref([ app.GUID_EMPTY ]);
const props = defineProps({
    title: {
        type: String,
        default: "部门管理"
    },
    empty: {
        type: String,
        default: "所有部门"
    },
})

watch(selectedKeys, nv => {
    if (!nv.length) selectedKeys.value.push(app.GUID_EMPTY);
    let id = nv.length ? nv[0] : app.GUID_EMPTY;
    selectedKey.value = id;
});

function onSearchChange () {
    dataSource.query()
        .whereIf("title", keyword, "")
        .whereIf("SchemaName", category, "")
        .end();
}

function toDepart (id) {
    selectedKeys.value = [id];
}

async function reSync (force = true) {
    await loadTree(force);
}

defineExpose({ reSync, toDepart });
// -- useDepart Start -- //

// -- useDepart End -- //
</script>

<style lang="less">
</style>