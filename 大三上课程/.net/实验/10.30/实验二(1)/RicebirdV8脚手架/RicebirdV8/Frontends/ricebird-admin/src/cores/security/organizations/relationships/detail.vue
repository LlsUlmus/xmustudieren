<template>
    <div id="user-list" class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    {{ displayTitle }}（共{{ pagination.total }}人）
                </div>
            </template>
        </a-tabs>
        
        <div class="page-pane no-padding-bottom">
            <div class="detail-view">
                <a-space class="searcher-area">
                    <a-segmented v-model:value="departType" :options="departTypes" v-if="departId === app.GUID_EMPTY" @change="search" />
                    <a-button type="primary" v-if="app.succeed('保存用户') && permit && !includeChildren" @click="addRelationship"><a-icon icon="plus-outlined" /> 添加关系</a-button>
                    <role-select v-if="permit" v-model:value="role" prefix="角色" @change="search" :disableSetFor="departId === app.GUID_EMPTY ? 1 : 0" />
                    <a-input-group compact style="width: 300px">
                        <a-select v-model:value="filterType" style="width: 100px" @change="onTypeChanged">
                            <a-select-option value="">综合</a-select-option>
                            <a-select-option value="code">证件号</a-select-option>
                            <a-select-option value="name">模糊姓名</a-select-option>
                            <a-select-option value="equals">精确姓名</a-select-option>
                        </a-select>
                        <a-input placeholder="按回车开始搜索" ref="filterRef" @click="clickFilter" style="width: calc(300px - 100px)"
                            @pressEnter="search" v-model:value="filter" />
                    </a-input-group>
                    <a-checkbox v-if="departId !== app.GUID_EMPTY" v-model:checked="includeChildren" @change="search">包含子部门</a-checkbox>
                    <!-- <a-button type="primary" @click="search">搜索</a-button> -->
                </a-space>
                <a-table class="table-area" :pagination="pagination" :data-source="dataSource" @change="onPagination"
                    :columns="columns">
                    <template #headerCell="{ column }">
                        <template v-if="column.key === 'operation'">
                            <span>
                                <div class="reflush" @click="load" v-if="app.succeed('获取组织关系')">
                                    <sync-outlined />刷新
                                </div>
                            </span>
                        </template>
                    </template>
                    <template #bodyCell="{ column, text, record }">
                        <template v-if="column.key === 'operation'">
                            <!-- <span class="a-btn" v-if="app.succeed('保存用户')" @click="edit(record.ID)" >[编辑]</span> -->
                            <span class="a-btn" v-if="app.succeed('保存用户')" @click="editUser(record.UserId)" >[设置关系]</span>
                            <span class="a-btn" v-if="app.succeed('删除组织关系') && permit" @click="remove(record.ID)" >[删除]</span> 
                        </template>
                    </template>
                </a-table>
                <editUserDialog ref="editorRef" />
                <addRelationshipDialog ref="addRef" />
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, inject, onMounted, watch, reactive, computed } from 'vue'
import app from '@/app'
import axios from '@/axios'
import { getDepart } from '../departs/useDepartment';
import editUserDialog from '../users/edit-user/edit-user.vue';
import addRelationshipDialog from './add-relationship.vue';

// -- edit-user -- //
const editorRef = ref();
const addRef = ref();
async function editUser (userId) {
    await editorRef.value.showModal(userId, "角色和组织机构");
    load();
}

async function addRelationship () {
    await addRef.value.showModal(departId.value, role.value);
    search();
}

// -- searcher -- //
let role = ref(app.GUID_EMPTY);
let filterType = ref("");
let departId = inject("selectedKey");
let filter = ref("");
let filterRef = ref();
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
let departType = ref("全部门通用");
let departTypes = ref(["全部门通用", "未设置关系"])
let dataSource = ref([]);
let includeChildren = ref(false);
let depart = reactive({});

async function load() {
    filter.value = filter.value.trim();
    let params = {
        roleId: role.value,
        departId: departId.value,
        filter: filter.value,
        filterType: filterType.value,
        departType: departType.value,
        includeChildren: includeChildren.value,
        page: pagination.current,
        pageSize: pagination.pageSize
    }

    let msg = await axios.post("/api/relationships/GetRelationships", params);
    dataSource.value = msg.data;
    pagination.current = msg.page;
    pagination.pageSize = msg.pageSize;
    pagination.total = msg.totalRow;
}

async function onPagination(pg) {
    Object.assign(pagination, pg);
    await load();
}

async function onTypeChanged() {
    if (filter.value) {
        await search();
    }
}

async function search() {
    pagination.current = 1;
    await load();
}

function clickFilter() {
    filterRef.value.select();
}
// -- searcher -- //

watch(departId, nv => {
    Object.assign(depart, getDepart(nv));
    if (nv === app.GUID_EMPTY) {
        depart.title = "无组织关系 / 任意部门";
    }
    load();
}, { immediate: true });

async function remove (id) {
    let msg = await app.modals.removeConfirm("是否要删除这一项，此项操作不会删除用户。");
    if (!msg) return;
    await axios.post("/api/relationships/RemoveRelationship", { id });
    await load();
}

const displayTitle = computed(() => {
    if (departId.value === app.GUID_EMPTY) {
        return departType.value;
    } else {
        return depart.title;
    }
});
const permit = computed(() => departId.value !== app.GUID_EMPTY || departType.value === '全部门通用');

const columns = ref([
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
        title: "所在部门",
        dataIndex: "DepartName",
        width: 350
    },
    {
        title: "用户身份",
        dataIndex: "RoleName",
        width: 200
    },
    {
        title: "操作",
        key: "operation",
        width: 200,
    },
]);
</script>