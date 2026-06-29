<template>
    <a-tabs v-model:activeKey="activeKey" class="tab-page-container">
        <template #leftExtra>
            <div class="page-title-area">
                页面名称
            </div>
        </template>
        <a-tab-pane key="4" tab="管理全校项目" class="page-pane" >
        </a-tab-pane>
    </a-tabs>
</template>

<script setup>
import app from '@/app'
import { ref } from 'vue';

const activeKey = ref("4");
</script>

<style lang="less">

</style>