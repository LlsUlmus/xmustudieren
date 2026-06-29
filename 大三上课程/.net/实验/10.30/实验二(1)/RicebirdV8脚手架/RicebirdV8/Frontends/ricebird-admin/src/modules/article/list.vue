<template>
    <div class="tab-page-container">
        <a-tabs v-model:activeKey="activeKey">
            <template #leftExtra>
                <div class="page-title-area">
                    论文成果管理
                </div>
            </template>
            <a-tab-pane key="4" tab="管理全校项目" v-if="app.getRole('教务处管理员')"></a-tab-pane>
            <a-tab-pane key="3" tab="管理本院项目" v-if="app.getRole('院级管理员')"></a-tab-pane>
            <a-tab-pane key="2" tab="我指导的项目" v-if="app.getRole('教职工')"></a-tab-pane>
            <a-tab-pane key="1" tab="我申报的项目" v-if="app.getRole('本科生')"></a-tab-pane>
        </a-tabs>

        <div class="page-pane no-padding-bottom">
            <div class="detail-view">
                <searcher :params @on-enter="onSearch" :proj-status="searcherProjStatuses"/>
                <a-divider />
                <a-space class="operation-area">
                    <a-button type="primary" @click="onSearch">
                        <SearchOutlined /> 搜索
                    </a-button>
                    <a-button @click="goToCreate()"><PlusOutlined /> 申报</a-button>
                    <export-button action="/api/achievements/article/ExportArticleAchieves" :params
                        v-if="app.succeed('导出论文汇总表')">导出论文汇总表</export-button>
                </a-space>
                <a-table class="table-area" :columns="columns" :pagination="pagination" rowKey="ID" @change="onPagination"
                         :data-source="dataSource">
                    <template #headerCell="{ column }">
                        <template v-if="column.key === 'operation'">
                            <span>
                                <div class="reflush" @click="load">
                                    <sync-outlined />刷新
                                </div>
                            </span>
                        </template>
                    </template>
                    <template #bodyCell="{ column, text, record }">
                        <template v-if="column.key === 'Status'">
                            <span>{{ achievementStatusToText(record[column.key]) }}</span>
                        </template>
                        <template v-if="column.key === 'operation'">
                            <span class="a-btn" @click="toEditPage(record)">
                                {{ isStudent === true ? '[编辑]' : '[查看]' }}
                            </span>
                            <span v-if="canDelete(record.Status)==true" class="a-btn remove-btn" @click="removeItem(record.ID)">[删除]</span>
                        </template>
                    </template>
                </a-table>
            </div>
        </div>
        <!-- <TypeEditor ref="editorRef"/> -->
    </div>
</template>

<script setup>
import app from '@/app'
import {ref, reactive, watch, computed} from 'vue'
import axios from '@/axios'
import { useRoute } from 'vue-router';
import searcher from './searcher.vue'
import create from './create.vue'
import {achievementStatusToText ,getCompetitionPermissionLevel} from "./useArticles";

const searcherProjStatuses = ["-1","0","1","16","8","11"]

const permissionLevel = app.getPermissionLevel().toString();
const activeKey = ref(permissionLevel);
const PermissionLevel = computed(() => getCompetitionPermissionLevel());
const isStudent = computed(() => {return (PermissionLevel.value<=1);});
const pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
    showQuickJumper: true,
    showTotal(total) {
        return `共 ${total} 条`;
    },
    position: ["bottomCenter"]
});

const params = reactive({
    achieved: "",
    journal: "",
    projOwner:"",
    guideTeacher:"",
    projStatus:"-1",
    applyYear:"",
    deaprtName:"",
    departId: app.GUID_EMPTY,
    lockTeacher: false,
    lockOwner: false,
});
const dataSource = ref([]);

async function onPagination (pg) {
    Object.assign(pagination, pg);
    await load();
}
const router  = useRoute();

async function onSearch () {
    pagination.current = 1;
    await load();
}

function goToCreate () {
    app.toPage({
        name: "article-apply",
    })
}

function toEditPage(record){
    console.log('当前记录的id是：', record.ID); 
    app.newPage({
        path:"/manage/article/detail",
        query: {
            id: record.ID
        }
    })
}

async function load () {
    params.achieved = params.achieved.trim();
    params.guideTeacher = params.guideTeacher.trim();
    params.projOwner = params.projOwner.trim();
    Object.assign(params, {
        page: pagination.current,
        pageSize: pagination.pageSize,
    });

    let msg = await axios.post("/api/achievements/article/GetArticles", params);
    console.log(msg);
    let data = msg.data;
    dataSource.value = data;
    pagination.current = msg.page;
    pagination.pageSize = msg.pageSize;
    pagination.total = msg.totalRow;
}

// const editorRef = ref();
// async function editItem (model) {
//     await editorRef.value.showModal(model);
//     load();
// }
function canDelete(status){
    if(PermissionLevel.value>2){
        return true;
    }
    else if([1, 0, 11].includes(status)){
        return true;
    }
    return false;
}

async function removeItem (id) {
    const flag = await app.modals.confirm({ title: "删除类型", content: "确定要删除吗？" });
    if (!flag) return;
    let msg = await axios.post("/api/achievements/article/RemoveArticle", { id: id });
    console.log(msg);
    app.modals.showResponse(msg);
    await load();
}

watch(activeKey, nv => {
    function lockField(key, locker, field, lockValue, srcValue) {
        if (nv.toString() === key.toString()) {
            params[locker] = true;
            params[field] = lockValue;
        } else {
            params[field] = params[locker] ? srcValue : params[field];
            params[locker] = false;
        }
    }
    let role = app.getRole('院级管理员');
    let departId = role ? role.ForDepart : app.GUID_EMPTY;
    lockField(3, "lockDepartId", "departId", departId, app.GUID_EMPTY);
    lockField(2, "lockTeacher", "guideTeacher", app.currentUser.Code, "");
    lockField(1, "lockOwner", "projOwner", app.currentUser.Code, "");
    onSearch();
}, { immediate: true });

const columns = ref([
    {
        title: "论文名称",
        dataIndex: "Achieved",
        key:"Achieved"
    },
    {
        title:"刊物名称",
        dataIndex:"Journal",
        key: "Journal",
        width:137
    },
    {
        title:"学生姓名",
        dataIndex:"StuName",
        width:137
    },
    {
        title:"指导教师",
        dataIndex:"GuideTeacher",
        width:137
    },
    {
        title:"负责学院",
        dataIndex:"DepartName",
        width:137
    },
    {
        title:"申请年度",
        dataIndex:"ApplyYear",
        key: "ApplyYear",
        width:137
    },
    {
        title:"审核状态",
        dataIndex:"Status",
        key: "Status",
        width:137
    },
    {
        title: "操作",
        key: "operation",
        width: 137
    },
]);
</script>

<style lang="less">
</style>