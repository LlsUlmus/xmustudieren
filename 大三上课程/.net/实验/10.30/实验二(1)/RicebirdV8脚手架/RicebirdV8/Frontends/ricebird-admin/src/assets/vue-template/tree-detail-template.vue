<template>
    <div id="room-detail" class="form-list-container">
        <!-- <a-result status="warning" title="你无权操作此数据字典，请选择其它项" /> -->
        <h1 class="header">考场管理</h1>
        <a-space class="searcher-area">
            <a-button type="primary" @click="editItem(app.GUID_EMPTY, {})" ><a-icon icon="plus-outlined"></a-icon>添加考场</a-button>
            <dict-select dict="科目级别" v-model:value="level" @change="onSearch" />
            <a-input-search v-model:value="filter" placeholder="搜索考场名" class="search-input" @search="onSearch" @change="onSearch" />
        </a-space>
        <a-table class="table-area" :columns="columns" :pagination="pagination" @change="onPagination" rowKey="ID" :data-source="dataSource">
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
                <template v-if="column.key === 'CampusCode'">
                    {{ app.toText("校区", text) }}
                </template>
                <template v-if="column.key === 'Level'">
                    {{ app.toText("科目级别", text) }}
                </template>
                <template v-if="column.key === 'Audition'">
                    {{ app.toText("听力播放方式", text) }}
                </template>
                <template v-if="column.key === 'operation'">
                    <span class="a-btn" @click="editItem(record.ID, record)">[编辑]</span>
                    <span class="a-btn" @click="removeItem(record.ID)">[删除]</span>
                </template>
            </template>
        </a-table>
    </div> 
</template>

<script setup>
import { ref, watch, inject, onMounted, reactive } from 'vue'
import app from '@/app'
import axios from '@/axios';
import { message } from 'ant-design-vue';

let filter = ref("");
let level = ref("A");
let key = inject("selectedKey");
let pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
    showQuickJumper: true
});
let dataSource = ref([]);

watch(key, load)

// 查找数据
function onSearch () {
    load();
}

// let editRoomRef = ref();
// 编辑项
async function editItem (id, model) {
    // editRoomRef.value.showModal(id, model, load);
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
    // filter.value = filter.value.trim();
    // let params = {
    //     level: level.value,
    //     filter: filter.value,
    //     buildingId: key.value,
    //     page: pagination.current,
    //     pageSize: pagination.pageSize
    // }

    // let msg = await axios.post("/api/rooms/GetRooms", params);

    // dataSource.value = msg.data;
    // pagination.current = msg.page;
    // pagination.pageSize = msg.pageSize;
    // pagination.total = msg.totalRow;
}

async function onPagination (pg) {
    Object.assign(pagination, pg);
    await load();
}

onMounted(load);

const columns = ref([
    // {
    //     title: "校区",
    //     dataIndex: "CampusCode",
    //     key: "CampusCode",
    //     width: 200, 
    // },
    // {
    //     title: "级别",
    //     dataIndex: "Level",
    //     key: "Level",
    //     width: 100, 
    // },
    // {
    //     title: "考场名",
    //     dataIndex: "Name",
    //     width: 400, 
    // },
    // {
    //     title: "容量",
    //     dataIndex: "Capacity",
    //     width: 100, 
    // },
    // {
    //     title: "听力播放方式",
    //     dataIndex: "Audition",
    //     key: "Audition",
    //     width: 200, 
    // },
    // {
    //     title: "操作",
    //     dataIndex: "operation",
    //     key: "operation",
    //     width: 200, 
    // },
]);
</script>