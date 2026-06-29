<template>
    <div id="category-tree" class="ricebird-tree-section">
        <div class="tree-search-area">
            <h1 class="header">{{ title }}</h1>
            
            <div class="action-btn-area">
                <a-tooltip title="同步栏目">
                    <div class="action-btn" title="刷新" @click="reSync"
                        :class="{ 'on-loading': loadingState === LOADING_STATE }">
                        <sync-outlined />
                    </div>
                </a-tooltip>
            </div>
        </div>
        <div class="tree-search-area no-pt">
            <a-input v-model:value="keyword" @change="onSearchChange" placeholder="搜索：栏目名" />
        </div>
        <div class="ricebird-tree-area">
            <a-tree show-icon :tree-data="allTree" v-model:selectedKeys="selectedKeys" :autoExpandParent="true">
                <template #switcherIcon="{ switcherCls }">
                    <a-icon icon="down-outlined" :class="switcherCls" />
                </template>
                <template #title="{ title, data }">
                    <span class="text-cut title">{{ title === "所有栏目" ? emptyName : title }}</span>

                    <div class="btn-area" v-if="title !== '所有栏目' && editable && data.type !== 4">
                    </div>
                </template>
                <template #icon="{ data }">
                    <a-icon :icon="getTypeIcon(data.type)"/>
                </template>
            </a-tree>
        </div>
    </div>
</template>

<script setup>
import { Modal, message } from 'ant-design-vue';
import { ref, reactive, watch, onMounted, toRef, watchEffect } from 'vue'
import { getTypeIcon } from './categoryType'
import {
    NORMAL_STATE,
    LOADING_STATE,
    categoryTree,
    loadingState,

    loadCategoryTree,
    searchCategory,
    deleteCategory
} from '../useCategories';
import { axios } from '@/axios';

let keyword = ref('');

const props = defineProps({
    editable: {
        type: Boolean,
        default: true
    },
    modelValue: {
        type: Array,
        default: () => [],
        required: true,
    },
    emptyName: {
        type: String,
        default: '所有栏目'
    },
    title: {
        type: String,
        default: '栏目管理'
    }
});

const emit = defineEmits();
let selectedKeys = ref(toRef(props, "modelValue").value);

function fireModelChanged (value) {
    if (typeof value === 'string') {
        selectedKeys.value = [ value ];
    } else if (value instanceof Array) {
        selectedKeys.value = value;
    }
}

watch(selectedKeys, v => {
    emit('update:modelValue', selectedKeys.value);
});

if (!selectedKeys.value.length) {
    fireModelChanged("00000000-0000-0000-0000-000000000000");
}

let allTree = ref([]);
onMounted(async function () {
    allTree.value = await loadCategoryTree(false);
})

async function reSync () {
    allTree.value = await loadCategoryTree(true);
}

async function onSearchChange () {
    allTree.value = await searchCategory(keyword.value);
}

function onEdit () {}
async function onDelete (data) {
    if (confirm('是否删除该栏目？')) {
        let msg = await axios.post("/api/cms/category/RemoveCategory", { id: data.id });
        if (!msg.success) {
            Modal.error({ title: msg.msg });
        } else {
            message.success("删除成功。");
            await reSync();
        }
    }
}

defineExpose({ reSync })
</script>

<style lang="less">
@import '@/assets/less/xmu-themes.less';

#category-tree {
    .action-btn-area {
        width: 32px;
    }
}
</style>
