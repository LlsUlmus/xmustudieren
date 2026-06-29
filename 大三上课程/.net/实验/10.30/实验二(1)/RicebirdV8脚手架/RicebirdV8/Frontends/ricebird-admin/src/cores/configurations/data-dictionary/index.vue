<template>
    <div id="dict-manager" class="page-with-tree">
        <dictTree />
        <div v-if="selectedKey === app.GUID_EMPTY">
            <a-empty description="点击左边的字典树开始操作" />
        </div>
        <dictDetail v-else></dictDetail>
    </div>
</template>

<script setup>
import dictTree from './dict-tree.vue'
import dictDetail from './dict-detail.vue'
import { ref, provide, watch } from 'vue'
import app from '@/app';
let selectedKey = ref(app.GUID_EMPTY);
provide("selectedKey", selectedKey);
</script>

<style lang="less">

</style>