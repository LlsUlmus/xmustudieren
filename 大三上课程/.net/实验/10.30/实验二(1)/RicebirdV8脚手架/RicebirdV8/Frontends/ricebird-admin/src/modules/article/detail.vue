<template>
    <div class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    <div class="page-title">
                        编辑论文成果
                    </div>
                </div>
            </template>
            <a-tab-pane class="page-pane nopadding">
                <ArticleBasicInfo :isNew="false" :data="articlemodel" />
            </a-tab-pane>
        </a-tabs>
    </div>
</template>

<script setup>
import app from "@/app";
import axios from "@/axios";
import {useRoute} from "vue-router";
import {message} from "ant-design-vue";
import {onMounted, reactive, ref} from "vue";
import ArticleBasicInfo from "@/modules/article/article-basic-info.vue";
import Articles from "./Articles";

const route = useRoute();

const articlemodel = reactive(Articles.getEmptyArticle());

async function loadmodel(ID) {
    let msg = await axios.post("/api/achievements/article/GetArticle",
    {
        id:ID
    });
    if (msg.success) {
        console.log("msg",msg);
        // msg.data.ReleaseTime = temp.toLocaleString();
        console.log(msg.data.ReleaseTime);
        Object.assign(articlemodel, msg.data);
        console.log("articlemode of detail",articlemodel);
    } else {
        message.error(msg.msg);
    }
}

onMounted(() => {
    loadmodel(route.query.id);
});
</script>

<style scoped lang="less">
</style>