<template>
    <div class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    <div class="page-title">
                        申报论文成果
                    </div>
                </div>
            </template>
            <a-tab-pane class="page-pane nopadding" :forceRender="true">
                <ArticleBasicInfo :data="articlemodel" :isNew="true"/>
            </a-tab-pane>
        </a-tabs>
    </div>
</template>

<script setup>
import app from "@/app";
import {useRoute} from "vue-router";
import { ref, reactive, watch, onMounted } from 'vue'
import ArticleBasicInfo from "@/modules/article/article-basic-info.vue";
import Articles from "./Articles";

const canApp = ref(true);
const errorStrings = ref([]);
const route = useRoute();
const id = route.query.id ;
const articlemodel = reactive(Articles.getEmptyArticle());
</script>

<style scoped lang="less">
</style>