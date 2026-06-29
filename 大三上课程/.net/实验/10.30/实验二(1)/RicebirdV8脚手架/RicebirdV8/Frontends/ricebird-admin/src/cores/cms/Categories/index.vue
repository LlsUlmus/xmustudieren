<template>
    <div id="categories-manager">
        <category-tree v-model="selectedKeys" ref="treeRef"/>
        <category-list :id="selected" />
    </div>
</template>

<script setup>
import { ref, onMounted, watch, provide, reactive } from 'vue';
import CategoryTree from './category-tree.vue'
import CategoryList from './category-list.vue'

let selectedKeys = ref([]);
let selected = ref("");
let treeRef = ref();
watch(selectedKeys, v => {
    selected.value = v.length ? v[0] : "00000000-0000-0000-0000-000000000000";
});

provide("treeRef", treeRef)

</script>

<style lang="less">
#categories-manager {
    width: 100%;
    height: @content-height;
    overflow: hidden;
    display: flex;
}
</style>
