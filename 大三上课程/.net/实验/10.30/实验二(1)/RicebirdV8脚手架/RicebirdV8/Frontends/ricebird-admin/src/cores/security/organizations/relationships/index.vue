<template>
    <div id="relationship-manager" class="page-with-tree">
        <departTree ref="treeRef" title="组织关系管理" empty="无组织关系 / 任意部门" />
        <detail />
    </div>
</template>

<script setup>
import departTree from '../departs/depart-tree.vue'
import { ref, provide, watch } from 'vue'
import app from '@/app';
import detail from './detail.vue';
let selectedKey = ref(app.GUID_EMPTY);
const treeRef = ref();
provide("selectedKey", selectedKey);
provide("treeRef", treeRef);
</script>

<style lang="less">
</style>