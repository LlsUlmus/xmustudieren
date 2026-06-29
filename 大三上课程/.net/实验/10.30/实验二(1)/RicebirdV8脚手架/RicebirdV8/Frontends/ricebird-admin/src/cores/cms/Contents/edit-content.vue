<template>
    <div id="edit-content" class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    新闻编辑
                </div>
            </template>
            <a-tab-pane key="1" tab="编辑页面">
            </a-tab-pane>
        </a-tabs>
        <a-row :gutter="16" class="page-pane no-padding-bottom">
            <a-col span="19">
                <a-card>
                    <a-input size="large" placeholder="文章标题" v-model:value="article.Topic" />
                    <a-input class="optional" placeholder="文章的副标题" v-model:value="article.SubTopic" />
                    <a-space class="optional">
                        是否外链
                        <a-switch v-model:checked="article.IsOutLink" checked-children="是" un-checked-children="否" :checkedValue="true" :unCheckedValue="false" />
                        <a-tooltip title="点击后跳转到一个新页面。">
                            <a-icon icon="question-circle-outlined" />
                        </a-tooltip>
                        <a-divider type="vertical" />
                        是否显示
                        <a-switch v-model:checked="article.IsDisplay" checked-children="是" un-checked-children="否" />
                        <a-tooltip title="是否显示在前台页面上。">
                            <a-icon icon="question-circle-outlined" />
                        </a-tooltip>
                        <a-divider type="vertical" />
                        是否置顶
                        <a-switch v-model:checked="article.TopMost" checked-children="是" un-checked-children="否" :checkedValue="1" :unCheckedValue="0" />
                        <a-tooltip title="新闻将永久在最顶上。">
                            <a-icon icon="question-circle-outlined" />
                        </a-tooltip>
                        <div class="attachment">
                            <Uploader ref="articlePdf" :attachments="article.Attachments" :usage="'附件'"/>
                        </div>
                        <!-- <a-checkbox v-model:value="article.IsLocked" :true-value="1" :false-value="0">
                            是否锁定
                            <a-tooltip title="锁定则无法评论。">
                                <a-icon icon="question-circle-outlined" />
                            </a-tooltip>
                        </a-checkbox> -->
                    </a-space>
                    <a-input placeholder="请输入外链" v-show="article.IsOutLink" v-model:value="article.OutLink" />
                    <div class="optional" v-if="!article.IsOutLink">
                        <ueditor v-model="article.Content" style="width: 100%" height="800" :topOffset="0" />
                    </div>
                </a-card>
            </a-col>
            <a-col span="5">
                <a-card>
                    <a-form layout="vertical">
                        <h1>基本信息</h1>
                        <a-form-item label="作者">
                            <a-input v-model:value="article.Author" />
                        </a-form-item>
                        <a-form-item label="发布时间">
                            <date-picker v-model:value="article.ReleaseTime" />
                        </a-form-item>
                        <a-form-item label="排序号">
                            <a-input v-model:value="article.DisplayOrder" />
                            <div class="text-secondary">
                                栏目类型是时间轴时，本字段<b>升序</b>排列。<br/>栏目类型非时间轴时，本字段<b>降序</b>排列。
                            </div>
                        </a-form-item>
                        <a-form-item label="摘要">
                            <a-textarea :rows="4" v-model:value="article.Abstract" />
                        </a-form-item>
                        <a-form-item>
                            <a-space v-if="!app.succeed('新闻审核')">
                                <a-button type="primary" @click="submit()" :loading="loading">提交</a-button>
                                <a-button @click="createNew()">新建文章</a-button>
                            </a-space>
                            <a-space v-else>
                                <a-button type="primary" @click="reviewArticle(1)" :loading="loading">审核通过</a-button>
                                <a-button @click="reviewArticle(0)">审核不通过</a-button>
                            </a-space>
                        </a-form-item>
                    </a-form>
                </a-card>
                <a-card class="mt20 clip-card">
                    <h1 class="optional">
                        <a-space>
                            <span>封面大图</span>
                            <a href="javascript:void(0);" @click="removeFeaturedImage">[删除]</a>
                        </a-space>
                    </h1>
                    <avatar-uploader :width="1920" :height="1080" :displayWidth="200"
                    :displayHeight="170" usage="head-image" :needCrop="false" text="点击上传封面图片，尺寸视放置位置而定。"
                    v-model:value="article.FeaturedImage" />
                </a-card>
            </a-col>
        </a-row>
    </div>
</template>

<script setup>
import router from '@/routers';
import axios from '@/axios';
import { reactive, ref, onMounted } from 'vue';
import { useRoute } from 'vue-router'
import dayjs from 'dayjs'
import { message } from 'ant-design-vue';
import Uploader from "@/cores/cms/Contents/content-uploader.vue";
import app from '@/app'

const route  = useRoute();
let article = reactive({
    "CategoryId": route.query.categoryId,
    "Topic":"",
    "SubTopic":"",
    "FeaturedImageAttachmentId":"00000000-0000-0000-0000-000000000000",
    "FeaturedImage":"",
    "CreatedBy":"",
    "Author":"",
    "Abstract":"",
    Attachments: [],
    "AttachList":"",
    "Content":"",
    "EnableComment":0,
    "IsOutLink":false,
    "OutLink":"",
    "VerifyStatus":0,
    "ReleaseTime": dayjs().format("YYYY-M-D"),
    "DisplayOrder":0,
    "TopMost":0,
    "Keyword":"",
    "Source":"",
    "Hits":0,
    "IsDisplay":true,
    "Language":"zh-cn",
    "Involved":"",
    "ArtType":"",
    "CategoryName":"",
    "UniqueCode":"",
    "ID": route.query.id,
});
const articlePdf = ref();
console.log("articlepdfvalue",articlePdf.value);

async function getArticleById(){
    let id = route.query.id;
    let categoryId = route.query.categoryId
    let msg = await axios.post("/api/cms/articles/GetArticleById", { id, categoryId });
    if (id === "0000000000000000000000") {
        document.title = "新建内容";
    } else {
        document.title = `${msg.data.Topic} - 内容管理`
    }
    // msg.data.ReleaseTime = dayjs(msg.data.ReleaseTime, "YYYY-MM-DD");
    Object.assign(article, msg.data);
}

onMounted(getArticleById)

const loading = ref(false);
async function submit () {
    loading.value = true;
    const temp = Object.assign({}, article);
    temp['附件'] = articlePdf.value.uploadIDs;
    temp.AttachList=articlePdf.value.uploadIDs;
    delete temp.Attachments;
    let msg = await axios.post("/api/cms/articles/SaveArticle", temp);
    loading.value = false;
    if (!msg.success) {
        message.error(msg.errorStrings.join(","));
    } else {
        message.success("保存成功");
        article.VerifyStatus = status;
        article.ID = msg.data.ID;
    }
}

async function reviewArticle (status) {
    loading.value = true;
    const temp = Object.assign({}, article);
    temp['附件'] = articlePdf.value.uploadIDs;
    temp.AttachList=articlePdf.value.uploadIDs;
    temp.VerifyStatus = status;
    delete temp.Attachments;
    let msg = await axios.post("/api/cms/articles/SaveArticle", temp);
    loading.value = false;
    if (!msg.success) {
        message.error(msg.errorStrings.join(","));
    } else {
        message.success("保存成功")
        article.VerifyStatus = status;
        article.ID = msg.data.ID;
    }

}

function removeFeaturedImage () {
    article.FeaturedImage = "";
}

function createNew () {
    let url = router.resolve({
        name: "cms-detail",
        query: {
            categoryId: route.query.categoryId
        }
    });
    location.href = url.fullPath;
}
</script>

<style lang="less">
@import '@/assets/less/form-list.less';
#edit-content {
    .optional {
        margin: 8px 0px;
    }

    .clip-card .optional {
        margin: 8px 0 0 0;
        a {
            color: @primary-color;
        }
    }

    .mt20 {
        margin-top: 20px;
    }
}
.tab-page-container{
    width: 100%;
    overflow-x: hidden;
}
</style>