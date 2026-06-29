<template>
    <div id="contents-manager">
        <category-tree v-model="selectedKeys" ref="treeRef" :editable="false" emptyName="未分类" title='内容管理'/>
        <content-list :category="selected" />
    </div>
</template>

<script setup>
import { ref, onMounted, watch, provide, reactive } from 'vue';
import CategoryTree from '../Categories/category-tree.vue'
import ContentList from './content-list.vue'

let selectedKeys = ref([]);
let selected = ref("");
let treeRef = ref();
watch(selectedKeys, v => {
    selected.value = v.length ? v[0] : "00000000-0000-0000-0000-000000000000";
});

provide("treeRef", treeRef)

</script>

<style lang="less">
#contents-manager {
    width: 100%;
    height: @content-height;
    overflow: hidden;
    display: flex;
}
</style>