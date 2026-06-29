<template>
    <div id="dict-tree" class="ricebird-tree-section">
        <div class="tree-search-area">
            <h1 class="header">字典管理</h1>
            <div class="action-btn-area">
                <a-tooltip title="添加字典">
                    <div class="action-btn" title="新建" @click="onEdit(app.GUID_EMPTY, '')" v-if="app.succeed('保存数据字典')">
                        <plus-outlined />
                    </div>
                </a-tooltip>
                <a-tooltip title="重新加载字典">
                    <div class="action-btn" title="刷新" :class="{ 'on-loading': loading }" @click="reSync">
                        <sync-outlined />
                    </div>
                </a-tooltip>
            </div>
        </div>
        <div class="tree-search-area no-pt">
            <a-input v-model:value="keyword" @change="onSearchChange" placeholder="搜索：字典名称" />
        </div>
        <div class="ricebird-tree-area">
            <a-tree show-icon :tree-data="dataSource.data" v-model:selectedKeys="selectedKeys" :autoExpandParent="false">
                <template #switcherIcon="{ switcherCls }">
                    <a-icon icon="down-outlined" :class="switcherCls" />
                </template>
                <template #title="{ title, data }">
                    <span class="text-cut title">{{ title }}</span>
                    <div class="btn-area" v-if="data.CanDelete">
                        <div class="btn" @click.stop="onEdit(data.ID, data.Name)" v-if="app.succeed('保存数据字典')">
                            <edit-outlined />
                        </div>
                        <div class="btn" @click.stop="onDelete(data.ID)" v-if="app.succeed('删除数据字典')">
                            <delete-outlined />
                        </div>
                    </div>
                    <div class="btn-area show-text" v-else>
                        [{{ toText("字典来源", data.From) }}]
                    </div>
                </template>
                <template #icon="{ data }">
                    <a-icon icon="BookOutlined" />
                </template>
            </a-tree>
        </div>
        <AddDict ref="add" v-if="app.succeed('保存数据字典')"></AddDict>
    </div>
</template>

<script setup>
import app from '@/app';
import { ref, inject, watch, onMounted } from 'vue'
import toText, { loadDictionary, RemoveDictionary, dataSource, getDictionaryById } from './useDictionary'
import AddDict from './add-dict.vue'
import { useRoute } from 'vue-router';

const route = useRoute();
let keyword = ref('');
let loading = ref(false);
let selectedKeys = ref([]);
let key = inject("selectedKey");

watch(key, nv => {
    selectedKeys.value = [nv];
});

watch(selectedKeys, nv => {
    if (nv.length) {
        key.value = nv[0];
    } else {
        selectedKeys.value = [ key.value ]
    }
});

onMounted(_ => {
    let dict = route.query.dict;
    if (dict) {
        let selectId = getDictionaryById(dict);
        if (selectId !== app.GUID_EMPTY) {
            selectedKeys.value = [selectId];
        }
    }
})

function onSearchChange() {
    dataSource.query()
        .whereIf("Name", keyword)
        .end()
}

async function reSync() {
    await loadDictionary(true);
}

// -- useDict Start -- //
let add = ref();
async function onEdit(id, name) {
    let msg = await add.value.showModal(id, name);
    if (msg && msg.length) {
        await loadDictionary(true);
    }
}
async function onDelete(id) {
    let msg = await app.modals.removeConfirm("是否要删除这一项？该操作会同时删除数据字典中所有子项");
    if (!msg) return;
    await RemoveDictionary(id);
    await loadDictionary(true);
    key.value = key.value === id ? app.GUID_EMPTY : key.value;
}
// -- useDict End -- //
</script>

<style lang="less"></style>