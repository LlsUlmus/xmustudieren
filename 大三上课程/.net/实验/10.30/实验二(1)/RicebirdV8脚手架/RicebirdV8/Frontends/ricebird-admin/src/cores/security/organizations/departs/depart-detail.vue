<template>
    <div id="department-list" class="tab-page-container" ref="scroller">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    <span class="back-btn" @click="goParent()" v-if="current.ParentId !== app.GUID_EMPTY">
                        <ArrowLeftOutlined  /> 去往上级&nbsp;&nbsp;
                    </span>
                    {{ current.Name }}
                </div>
            </template>
        </a-tabs>
        <div class="page-pane no-padding-bottom">
            <div class="detail-view">
                <a-space class="searcher-area">
                    <a-button v-if="app.succeed('保存部门')" type="primary" @click="edit({ ...empty })"><a-icon icon="plus-outlined" /> 新建子部门</a-button>
                    <a-button v-if="app.succeed('保存部门')" :disabled="current.ID === app.GUID_EMPTY" @click="edit({ ...current })">修改本部门</a-button>
                    <a-divider type="vertical" />
                    <a-button v-if="app.succeed('批量处理部门')" :disabled="!selectedRow.length" @click="cutOrCopy('cut')">剪切</a-button>
                    <a-button v-if="app.succeed('批量处理部门')" :disabled="!selectedRow.length" @click="cutOrCopy('copy')">复制</a-button>
                    <a-button  v-if="app.succeed('批量处理部门')" :disabled="!clipboard.value.length" @click="paste()">粘贴</a-button>
                    <a-divider type="vertical" />
                    <a-button v-if="app.succeed('批量处理部门')" :disabled="!selectedRow.length" @click="del()">删除</a-button>
                </a-space>
                <a-table :data-source="data" :columns="columns" :rowSelection="rowSelection" class="table-area" :pagination="false" @change="onPagination">
                    <template #bodyCell="{ column, text, record }">
                        <template v-if="column.key === 'operation'">
                            <span class="a-btn" v-if="app.succeed('保存部门')" @click="edit(record)">[编辑]</span>
                        </template>
                    </template>
                </a-table>
                <editDepart ref="editor"/>
                <a-back-top :target="target" />
            </div>
    </div>
    </div>
</template>

<script setup>
import app from '@/app';
import { watch, ref, reactive, provide, inject } from 'vue'
import { loadTree, dataSource, flatTree } from './useDepartment'
import { Modal, message } from 'ant-design-vue'
import editDepart from './edit-depart.vue'
import axios from '@/axios'

let data = ref(flatTree.filter(e => e.pid === app.GUID_EMPTY));
let pagination = reactive({
    current: 1,
    pageSize: 10,
    total: data.value.length,
    showQuickJumper: true,
    showTotal (total) {
        return `共 ${total} 条`;
    },
    position: ["bottomCenter"]
});

const scroller = ref();
const activeKey = ref("dashboard");
const selectedKey = inject("selectedKey");
const treeRef = inject("treeRef");
const current = reactive({
    Name: "",
    ID: app.GUID_EMPTY
});
const editor = ref();
const empty = {
    ID: app.GUID_EMPTY,
    Name: "所有部门",
    Code: "",
    DisplayOrder: 0,
    ParentId: app.GUID_EMPTY,
    SchemaName: "普通部门",
    Source: "后台新建",
};

watch(dataSource, nv => {
    loadDepartment();
}, {deep: true});
watch(selectedKey, nv => {
    loadDepartment();
}, { immediate: true });

function edit (model) {
    if (model.ID === app.GUID_EMPTY) {
        model.ParentId = current.ID;
    }
    editor.value.showModal(model);
}

function loadDepartment() {
    if (selectedKey.value === app.GUID_EMPTY) {
        data.value = flatTree.filter(e => e.pid === app.GUID_EMPTY);
        Object.assign(current, empty);
    } else {
        data.value = flatTree.filter(e => e.pid === selectedKey.value);
        let cur = flatTree.find(e => e.ID === selectedKey.value);
        Object.assign(current, cur);
    }
}

function target () {
    return scroller.value;
}

function goParent () {
    treeRef.value.toDepart(current.ParentId);
}

function onPagination() {
    // TODO
}

// -- 复制粘贴 -- //
const selectedRow = ref([]);
const selectedRowKeys = ref([]);
const clipboard = reactive({
    opera: "",
    value: [],
    srcDepartId: ""
});
function cutOrCopy(opera) {
    Object.assign(clipboard, { opera: opera, value: ref(selectedRow.value), srcDepartId: selectedKey.value });
    selectedRow.value = [];
    selectedRowKeys.value = [];
}
function del() {
    Object.assign(clipboard, { opera: 'delete', value: ref(selectedRow.value) });
    selectedRow.value = [];
    selectedRowKeys.value = [];
    move();
}
function paste() {
    move()
}

function move() {
    let selected = [];
    clipboard.value.map(e => {
        if (selected.length < 5) {
            selected.push(e.Name);
        }
    })

    let messageList = {
        "cut": {
            opera: "剪切",
            message: selected.length === 1 ? `是否要将${selected.join('，')}剪切到此处？` : `是否要将${selected.join('，')}等${clipboard.value.length}项剪切到此处？`
        },
        "copy": {
            opera: "复制",
            message: selected.length === 1 ? `是否要将${selected.join('，')}复制到此处？` : `是否要将${selected.join('，')}等${clipboard.value.length}项复制到此处？`
        },
        "delete": {
            opera: "删除",
            message: selected.length === 1 ? `是否要删除${selected.join('，')}？这会同时删除他们所有子部门。` : `是否要删除${selected.join('，')}等${clipboard.value.length}项？这会同时删除他们所有子部门。`
        },
    }
    let opera = messageList[clipboard.opera];

    let confirmOpt = {
        title: opera.message,
        onOk: () => {
            if (clipboard.opera === "delete" && !confirm(`这个操作无法还原，需要二次确认。${opera.message}`)) {
                return;
            }
            let param = {
                to: current.ID,
                opera: clipboard.opera,
                value: clipboard.value.map(e => e.ID).join(','),
                srcDepartId: clipboard.srcDepartId
            };
            Object.assign(clipboard, { opera: "", value: [] });
            axios.post("/api/depart/MoveDepartment", param).then(msg => {
                if (msg.success) {
                    message.success(`${opera.opera}成功`);
                    treeRef.value.reSync().then(() => {
                        loadDepartment();
                    });
                } else {
                    message.error(msg.msg);
                }
            });
        }
    };

    Modal.confirm(confirmOpt)
}

const rowSelection = {
    onSelect: (record, selected,) => {
        let existIndex = selectedRow.value.findIndex(e => e.ID === record.ID);
        if (selected && existIndex === -1) {
            selectedRow.value.push(record);
        } else if (!selected && existIndex > -1) {
            selectedRow.value.splice(existIndex, 1);
        }
    },
    onSelectAll: (selected, selectedRows) => {
        if (!selected) {
            selectedRow.value = [];
            return;
        }
        for (let record of selectedRows) {
            rowSelection.onSelect(record, selected);
        }
    },
    onChange: (srk, selectedRows) => {
        selectedRowKeys.value = srk;
    },
    getCheckboxProps: record => ({
        // disabled: record.CategoryType === 4,
        name: record.ID
    }),
    selectedRowKeys
}

// -- columns -- //
const columns = [
    {
        title: "部门名称",
        dataIndex: "Name",
    },
    {
        title: "部门类型",
        dataIndex: "SchemaName",
    },
    {
        title: "部门来源",
        dataIndex: "Source",
    },
    {
        title: "排序号(升)",
        dataIndex: "DisplayOrder",
    },
    {
        title: "操作",
        key: "operation",
    },
];
</script>

<style lang="less">
#department-list {
    .mt {
        margin-top: 16px;
    }
}
.back-btn {
    cursor: pointer;
}
</style>