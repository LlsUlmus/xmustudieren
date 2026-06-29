<template>
    <div id="organization-manager" class="page-with-tree">
        <departTree title="部门管理" ref="treeRef" />
        <!-- <div v-if="selectedKey === app.GUID_EMPTY">
            <a-empty description="点击左边的部门树开始操作，或者添加一个部门" >
                <a-button type="primary">添加部门</a-button>
            </a-empty>
        </div> -->
        <departDetail />
    </div>
</template>

<script setup>
import departTree from './depart-tree.vue'
import departDetail from './depart-detail.vue';
import { ref, provide, watch } from 'vue'
import app from '@/app';
let selectedKey = ref(app.GUID_EMPTY);
const treeRef = ref();
provide("selectedKey", selectedKey);
provide("treeRef", treeRef);
</script>

<style lang="less">
</style>