<template>
    <div id="user-list" class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    {{ depart.title }}（共{{ pagination.total }}人）
                </div>
            </template>
        </a-tabs>
        <div class="page-pane no-padding-bottom">
            <div class="detail-view">
                <a-space class="searcher-area">
                <a-button type="primary" @click="edit(app.GUID_EMPTY)" v-if="app.succeed('保存用户')"><a-icon icon="plus-outlined" /> 添加用户</a-button>
                <role-select v-model:value="role" prefix="角色" @change="search" />
                <a-input-group compact style="width: 300px">
                    <a-select v-model:value="filterType" style="width: 100px" @change="onTypeChanged">
                        <a-select-option value="">综合</a-select-option>
                        <a-select-option value="code">证件号</a-select-option>
                        <a-select-option value="name">模糊姓名</a-select-option>
                        <a-select-option value="equals">精确姓名</a-select-option>
                    </a-select>
                    <a-input placeholder="按回车开始搜索" ref="filterRef" @click="clickFilter" style="width: calc(300px - 100px)" @pressEnter="search" v-model:value="filter"/>
                </a-input-group>
                <a-checkbox v-if="app.succeed('允许查看用户隐私')" v-model:checked="proctected" @change="search">保护用户信息</a-checkbox>
                <a-button type="primary" @click="search">搜索</a-button>
            </a-space>
            <a-table class="table-area" :pagination="pagination" :data-source="dataSource" @change="onPagination" :columns="columns">
                <template #headerCell="{ column }">
                    <template v-if="column.key === 'operation'">
                        <span>
                            <div class="reflush" @click="load" v-if="app.succeed('查询用户')">
                                <sync-outlined />刷新
                            </div>
                        </span>
                    </template>
                </template>
                <template #bodyCell="{ column, text, record }">
                    <template v-if="column.dataIndex === 'RealName'">
                        <!-- <span class="a-btn" v-if="app.succeed('保存用户')" @click="edit(record.ID)" >{{ text }}</span> -->
                        <span>{{ text }}</span>
                    </template>
                    <template v-if="column.key === 'operation'">
                        <span class="a-btn" v-if="app.succeed('保存用户')" @click="edit(record.ID)" >[编辑]</span>
                        <span class="a-btn" v-if="app.succeed('删除用户')" @click="remove(record.ID)" >[删除]</span>
                    </template>
                </template>
            </a-table>
            <editUser ref="editorRef" />
        </div>
    </div>
</div>
        
</template>
<script setup>
import { ref, inject, onMounted, watch, reactive } from 'vue'
import editUser from './edit-user/edit-user.vue';
import app from '@/app'
import axios from '@/axios'
import { getDepart } from '../departs/useDepartment';

// -- edit-user -- //
const editorRef = ref();
async function edit (userId) {
    await editorRef.value.showModal(userId);
    search(false);
}

// -- searcher -- //
let role = ref(app.GUID_EMPTY);
let filterType = ref("");
let departId = inject("selectedKey");
let filter = ref("");
let filterRef = ref();
let proctected = ref(!app.succeed("允许查看用户隐私"));
let pagination = reactive({
    current: 1,
    pageSize: 10,
    total: 0,
    showQuickJumper: true,
    showTotal (total) {
        return `共 ${total} 条`;
    },
    position: ["bottomCenter"]
});
let dataSource = ref([]);
let depart = reactive({});

async function load () {
    filter.value = filter.value.trim();
    let params = {
        roleId: role.value,
        departId: departId.value,
        filter: filter.value,
        filterType: filterType.value,
        proctected: proctected.value,
        page: pagination.current,
        pageSize: pagination.pageSize
    }
    let msg = await axios.post("/api/users/GetUsers", params);
    dataSource.value = msg.data;
    pagination.current = msg.page;
    pagination.pageSize = msg.pageSize;
    pagination.total = msg.totalRow;
}

async function onPagination (pg) {
    Object.assign(pagination, pg);
    await load();
}

async function onTypeChanged () {
    if (filter.value) {
        await search();
    }
}

async function search(resetPage = true) {
    if (resetPage) {
        pagination.current = 1;
    }
    await load();
}

function clickFilter () {
    filterRef.value.select();
}
// -- searcher -- //
watch(departId, nv => {
    Object.assign(depart, getDepart(nv));
    load();
}, {immediate: true});

async function remove (id) {
    let msg = await app.modals.removeConfirm("是否要删除这一项？");
    if (!msg) return;
    await axios.post("/api/users/RemoveUser", { id });
    await load();
}

const columns = [
    {
        title: "姓名",
        dataIndex: "RealName",
        width: 210,
    },
    {
        title: "证件号",
        dataIndex: "Code",
        width: 220
    },
    {
        title: "电话",
        dataIndex: "Mobile",
    },
    {
        title: "邮箱",
        dataIndex: "Email",
    },
    {
        title: "操作",
        key: "operation",
        width: 180,
    },
];
</script>

<style lang="less"></style>