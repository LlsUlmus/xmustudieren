<template>
    <div id="dict-tree"  class="ricebird-tree-section">
        <div class="tree-search-area">
            <h1 class="header">角色管理</h1>
            <div class="action-btn-area">
                <a-tooltip title="添加角色">
                    <div class="action-btn" title="新建" @click="onEdit(app.GUID_EMPTY)" v-if="app.succeed('保存角色')">
                        <plus-outlined />
                    </div>
                </a-tooltip>
                <a-tooltip title="重新加载角色">
                    <div class="action-btn" title="刷新" :class="{ 'on-loading': loading }" @click="reSync" v-if="app.succeed('获取角色列表')">
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
                    <div class="btn-area" v-if="data.canEdit">
                        <div class="btn" @click.stop="onEdit(data.key)" v-if="app.succeed('保存角色')">
                            <edit-outlined />
                        </div>
                        <div class="btn" @click.stop="onDelete(data.key)" v-if="app.succeed('删除角色')">
                            <delete-outlined />
                        </div>
                    </div>
                    <div class="btn-area show-text" v-else>
                        [{{ data.isDefault ? "默认" : "内置" }}]
                    </div>
                </template>
                <template #icon="{ data }">
                    <a-icon icon="TeamOutlined" />
                </template>
            </a-tree>
        </div>
        <AddRow ref="add" />
    </div>
</template>

<script setup>
import app from '@/app';
import { ref, inject, watch } from 'vue'
import { loadTree, dataSource, tree, } from './useRole'
import AddRow from './add-Role.vue'
import axios from '@/axios'

let keyword = ref('');
let loading = ref(false);
let selectedKey = inject("selectedKey");
let selectedKeys = ref([]);

watch(selectedKeys, nv => {
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

// -- useRole Start -- //
let add = ref();
async function onEdit(id) {
    let msg = await add.value.showModal(id);
    if (msg && msg.length) {
        await loadTree(true);
    }
}
async function onDelete(id) {
    let msg = await app.modals.removeConfirm("是否要删除这一项？");
    if (!msg) return;
    await axios.post("/api/roles/RemoveRoleSchema", { id });
    await loadTree(true);
    selectedKeys.value = [ selectedKey.value === id ? app.GUID_EMPTY : selectedKey.value ];
}
// -- useRole End -- //
</script>

<style lang="less">
</style>