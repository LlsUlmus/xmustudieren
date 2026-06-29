<template>
    <div id="role-schema-manager"  class="page-with-tree">
        <RoleTree />
        <div v-if="selectedKey === app.GUID_EMPTY">
            <a-empty description="点击左边的角色树开始操作" />
        </div>
        <RoleDetail v-else />
    </div>
</template>

<script setup>
import app from '@/app';
import { ref, provide } from 'vue'
import RoleTree from './role-tree.vue'
import RoleDetail from './role-detail.vue';

let selectedKey = ref(app.GUID_EMPTY);
provide("selectedKey", selectedKey);
</script>