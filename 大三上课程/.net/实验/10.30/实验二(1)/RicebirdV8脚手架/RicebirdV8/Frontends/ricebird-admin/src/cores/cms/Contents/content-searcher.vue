<template>
    <a-space class="content-searcher" >
        <!-- <div>
            从<a-date-picker class="mlr5" v-model:value="searchModel.From" mode="date" />至<a-date-picker class="mlr5" v-model:value="searchModel.To" />
        </div> -->
        <a-select v-model:value="searchModel.VerifyStatus" style="width: 140px">
            <a-select-option :value="-1">--- 发布状态 ---</a-select-option>
            <a-select-option :value="0">未审核</a-select-option>
            <a-select-option :value="1">已审核</a-select-option>
            <a-select-option :value="2">已撤稿</a-select-option>
        </a-select>
        <div>
            <a-input v-model:value="searchModel.Topic" placeholder="按关键字搜索" />
        </div>
        <div>
            <a-button type="primary" @click="onSearch">搜索</a-button>
        </div>
    </a-space>
</template>

<script setup>
import { ref, reactive, watchEffect } from 'vue';
import axios from '@/axios'
import app from "@/app";

const props = defineProps({
    category: String,
    page: Number,
    pageSize: Number,
    relateId: app.GUID_EMPTY
})
const emit = defineEmits();
const categoryId = ref("");
let searchModel = reactive({
    VerifyStatus: -1,
    Topic: "",
    Page: 1,
    PageSize: 10,
    CategoryId: ""
});

watchEffect(() => {
    categoryId.value = props.category;
    searchModel.Page = props.page || 1;
    searchModel.PageSize = props.pageSize || 10;
    searchModel.CategoryId = props.category;
    searchModel.RelateId = props.relateId;
})

async function onSearch () {
    let msg = await axios.post("/api/cms/articles/GetArticles", searchModel);
    // 这一步必须做，否则 复制，粘贴 等功能全部无法实现 @ 黄玺 2022-10-30
    msg.data.map(e => {
        e.key = e.UniqueCode;
    })
    if (msg.success) {
        emit("onSearch", msg);
    }
}

defineExpose({ onSearch });
</script>

<style lang="less" scoped>
.content-searcher {
    flex: 1;
    .mlr5 {
        margin: 0 5px;
    }
}
</style>
