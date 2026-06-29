<template>
    <div id="schedule-detail" class="form-list-container" trigger="click">
        <h1 class="header">任务管理</h1>
        <a-space class="searcher-area">
            <!-- <a-button type="primary" @click="editItem()" ><a-icon icon="plus-outlined"></a-icon>添加任务</a-button>
            <import-button action="/api/schedules/Create" >上传任务</import-button> -->
        </a-space>
        <a-table class="table-area" :columns="columns" :pagination="pagination" rowKey="ID" :data-source="dataSource">
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
                <template v-if="column.key === 'Name'">
                    {{ text }}
                    <span v-if="record.Status === 1">
                        （{{ record.CurrentLog }}）
                    </span>
                </template>
                <template v-if="column.key === 'Status'">
                    {{ app.toText("任务状态", text) }}
                </template>
                <template v-if="column.key === 'operation'">
                    <span class="a-btn" @click="showSchedule(record.ID)" >[查看]</span>
                    <span class="a-btn" v-if="record.Status === 2 && record.DownloadPath" @click="download(record)" >[下载结果]</span>
                </template>
            </template>
        </a-table>
    </div> 
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue'
import app from '@/app'
import axios from '@/axios';
import { message } from 'ant-design-vue';
import { showSchedule, scheduleList, reloadList } from './schedule-service'


let pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
    showQuickJumper: true
});
let dataSource = scheduleList;

// 编辑项
async function editItem () {
    let msg = await axios.post("/api/schedules/Create");
}

async function uploadItem () {
    let msg = await axios.post("/api/schedules/Create");
}

function download (record) {
    window.open(record.DownloadPath);
}

// 删除项
async function removeItem (id) {
    // let msg = await app.modals.removeConfirm("是否要删除这一项？");
    // if (!msg) return;
    // let response = await axios.post("/api/rooms/RemoveRoom", { id });
    // response.success ? message.success("操作成功") : message.error(response.msg);
    // load();
}

// 载入数据
async function load () {
    await reloadList();
}

const columns = ref([
    {
        title: "任务名称",
        dataIndex: "Name",
        key: "Name",
    },
    {
        title: "任务进行时间",
        dataIndex: "Type",
        width: 200, 
    },
    {
        title: "任务进度",
        dataIndex: "Progress",
        key: "Progress",
        width: 200, 
    },
    {
        title: "任务状态",
        dataIndex: "Status",
        key: "Status",
        width: 200, 
    },
    {
        title: "操作",
        dataIndex: "operation",
        key: "operation",
        width: 200, 
    },
]);
</script>
