<template>
    <div id="dict-detail" class="tab-page-container">
        <!-- <a-result status="warning" title="你无权操作此数据字典，请选择其它项" /> -->
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    数据字典管理：{{ dictName }}
                </div>
            </template>
        </a-tabs>
        <div class="page-pane">
            <div class="detail-view">
                <a-space class="searcher-area">
                    <a-button v-if="app.succeed('保存字典项')" type="primary" @click="editItem(emptyModel)"
                        :disabled="!canEdit"><a-icon icon="plus-outlined"></a-icon>添加数据项</a-button>
                    <a-button v-if="app.succeed('重排字典项')" @click="reorder()" :disabled="!canEdit">重建顺序号</a-button>
                    <a-input-search v-model:value="keyword" placeholder="搜索键名或数据项名称" class="search-input"
                        @search="onSearch" @change="onSearch" />
                </a-space>
                <a-table class="table-area" :columns="columns" :pagination="false" rowKey="ID" :data-source="data">
                    <template #headerCell="{ column }">
                        <template v-if="column.key === 'operation'">
                            <span>
                                <div class="reflush" @click="load(true)">
                                    <sync-outlined />刷新
                                </div>
                            </span>
                        </template>
                    </template>
                    <template #bodyCell="{ column, text, record }">
                        <template v-if="column.key === 'Enable'">
                            {{ text ? "是" : "否" }}
                        </template>
                        <template v-if="column.key === 'Visible'">
                            {{ text ? "是" : "否" }}
                        </template>
                        <template v-if="column.key === 'operation'">
                            <span class="a-btn" v-if="record.CanEdit && app.succeed('保存字典项')"
                                @click="editItem(record)">[编辑]</span>
                            <span class="a-btn" v-if="record.CanDelete && app.succeed('删除字典项')"
                                @click="removeItem(record.ID)">[删除]</span>
                            <span class="lock" v-if="!record.CanEdit && !record.CanDelete">该项已锁定</span>
                        </template>
                    </template>
                </a-table>
                <AddEntry ref="addEntry" />
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, watch, inject } from 'vue'
import app from '@/app'
import { getDictionary, loadDictionary } from './useDictionary'
import AddEntry from './add-entry.vue'
import axios from '@/axios';
import { message } from 'ant-design-vue';
let key = inject("selectedKey");
let data = ref([]);
let keyword = ref("");

watch(key, () => {
    load();
    emptyModel.DataDictionaryId = key.value;
})

// 查找数据
function onSearch() {
    load();
}

let addEntry = ref();
const emptyModel = {
    ID: app.GUID_EMPTY,
    "DataDictionaryId": key.value,
    "DataKey": "",
    "DataValue": "",
    "Enable": true,
    "DisplayOrder": 100,
};
// 编辑项
async function editItem(model) {
    await addEntry.value.showModal(model);
    load(true);
}

// 删除项
async function removeItem(id) {
    let msg = await app.modals.removeConfirm("是否要删除这一项？");
    if (!msg) return;
    msg = await axios.post("/api/dict/RemoveEntry", { id });
    msg.success ? message.success("操作成功") : message.error(msg.msg);
    load(true);
}

// 重新排序
async function reorder() {
    let msg = await axios.post("/api/dict/ReorderEntry", { id: key.value });
    msg.success ? message.success("操作成功") : message.error(msg.msg);
    load(true);
}

let dictName = ref("");
let canEdit = ref(false), canDelete = ref(false);
// 载入数据
async function load(force = false) {
    await loadDictionary(force);
    let result = getDictionary(key.value, keyword.value);
    dictName.value = result.name;
    data.value = result.entries;
    canEdit.value = result.canEdit;
    canDelete.value = result.canDelete;
}
load();

const columns = ref([
    {
        title: "键名",
        dataIndex: "DataKey",
        width: 400,
    },
    {
        title: "显示值",
        dataIndex: "DataValue",
        width: 400,
    },
    {
        title: "是否启用",
        dataIndex: "Enable",
        key: "Enable",
        width: 130
    },
    {
        title: "是否显示",
        dataIndex: "Visible",
        key: "Visible",
        width: 130
    },
    {
        title: "排序号",
        dataIndex: "DisplayOrder",
        width: 100
    },
    {
        title: "操作",
        dataIndex: "operation",
        key: "operation",
        width: 160
    },
]);
</script>
<style lang="less" scoped>
#dict-detail {
    .page-pane {
        margin-top: 49px;
        padding-top: 16px;
        height: 100vh;
        overflow: auto;
        .detail-view {
            padding-bottom: 32px;
        }
    }
}
</style>