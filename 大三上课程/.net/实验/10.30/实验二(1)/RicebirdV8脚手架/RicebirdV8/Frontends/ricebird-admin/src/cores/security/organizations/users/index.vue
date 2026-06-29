<template>
    <div id="organization-manager" class="page-with-tree">
        <departTree  ref="treeRef" title="用户管理" />
        <userDetail />
    </div>
</template>

<script setup>
import departTree from '../departs/depart-tree.vue'
import { ref, provide, watch } from 'vue'
import app from '@/app';
import userDetail from './user-detail.vue';
let selectedKey = ref(app.GUID_EMPTY);
const treeRef = ref();
provide("selectedKey", selectedKey);
provide("treeRef", treeRef);
</script>

<style lang="less">
</style>