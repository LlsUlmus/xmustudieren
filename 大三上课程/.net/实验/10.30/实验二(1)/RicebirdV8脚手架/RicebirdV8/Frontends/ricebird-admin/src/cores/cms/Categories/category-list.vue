<template>
    <div id="category-list" class="form-list-container">
        <a-tabs v-model:activeKey="activeKey" class="tab-container">
            <template #leftExtra>
                <div class="page-title-area">
                    栏目管理
                </div>
            </template>
            <a-tab-pane class="dashboard" key="dashboard" :tab="`${entity.Name === '所有栏目' ? '' : entity.Name} 概览`">
                <a-alert type="info" message="注意事项" v-if="entity.SpecialMessageInCategory" class="mb" showIcon>
                    <template #description>
                        <ul>
                            <li v-for="(v, k) in entity.SpecialMessageInCategory.split('\n')" :key="k">{{ v }}</li>
                        </ul>
                    </template>
                </a-alert>
                <a-space class="btn-area">
                    <template  v-if="allowCreate">
                        <a-button type="primary" @click="addCategory" >新建栏目</a-button>
                        <a-button disabled >批量新建栏目</a-button>
                        <a-divider type="vertical" />
                        <a-button :disabled="!selectedRow.length" @click="cutOrCopy('cut')" >剪切</a-button>
                        <a-button :disabled="!selectedRow.length" @click="cutOrCopy('copy')" >复制</a-button>
                        <a-button :disabled="!clipboard.value.length" @click="paste()" >粘贴</a-button>
                        <a-divider type="vertical" />
                        <a-button :disabled="!selectedRow.length" @click="del()" >删除</a-button>
                    </template>
                    <!--预览当前栏目：
                    <a-button>电脑屏</a-button> -->
                </a-space>

                <a-alert type="info" message="这种类型不能新建子栏目，也不能将栏目粘贴到这里。" showIcon v-if="!allowCreate" class="mt"></a-alert>

                <a-table :data-source="children" :columns="columns" :rowSelection="rowSelection" class="mt" :pagination="false">
                    <template #bodyCell="{ column, text }">
                        <template v-if="column.key === 'type'">
                            <a-icon :icon="types[text].icon" /> {{ types[text].label }}
                        </template>
                        <template v-if="column.key === 'status'">
                            {{ text === 1 ? "不可见" : "可见" }}
                        </template>
                    </template>
                </a-table>

                <add-category-component v-if="add" @cancel="cancel" @onComplete="onAdded"></add-category-component>
            </a-tab-pane>
            <a-tab-pane key="details" tab="属性" v-if="entity.Name !== '所有栏目'">
                <edit-category :model="entity"/>
            </a-tab-pane>
        </a-tabs>
    </div>
</template>

<script setup>
import { message, Modal } from 'ant-design-vue'
import { watchEffect, ref, reactive, provide, inject } from 'vue'
import addCategoryComponent from './add-category.vue';
import { types } from './categoryType'
import {
    loadCategory
} from '../useCategories';
import editCategory from './edit-category.vue';
import axios from '@/axios'

const guidEmpty = "00000000-0000-0000-0000-000000000000";
const props = defineProps({
    id: String
})

const activeKey = ref("dashboard");
const id = ref("");
const add = ref(true);
const allowCreate = ref(true);
const children = ref([]);
let entity = reactive({});
provide("current", entity);
let lastCategoryId = "";

watchEffect(() => {
    id.value = props.id;
    cancel();
    getCategory();
});

function addCategory () {
    add.value = true;
}

function cancel () {
    add.value = false;
}

async function onAdded () {
    add.value = false;
    await getCategory(true);
}

// 切换到一个新的分类
async function getCategory(force) {
    if (id.value == lastCategoryId && !force) {
        return;
    }

    let msg = (await loadCategory(id.value));
    let cate = msg.data;
    if (id.value === guidEmpty) {
        cate.ID = guidEmpty;
        cate.Name = "所有栏目";
    }

    if (cate.Name === "所有栏目") {
        document.title = "栏目管理";
    } else {
        document.title = `${cate.Name} - 栏目管理`;
    }

    // 对服务器下发的数据进行结构变幻，把Children加到data里
    msg.Children.map(e => {
        e.key = e.ID;
    })
    cate.Children = children;

    lastCategoryId = cate.ID;
    if (activeKey.value !== "dashboard" && cate.Name === '所有栏目') {
        activeKey.value = "dashboard";
    }

    var type = types[cate.CategoryType];
    allowCreate.value = type.allowSub;
    Object.assign(entity, cate);

    children.value = msg.Children;
    selectedRow.value = [];
    selectedRowKeys.value = [];
}

// 复制和粘贴功能
const treeRef = inject("treeRef");
let selectedRow = ref([]);
const clipboard = reactive({
    opera: "",
    value: [],
    srcCategoryId: ""
});
let selectedRowKeys = ref([]);
function cutOrCopy (opera) {
    Object.assign(clipboard, { opera: opera, value: ref(selectedRow.value), srcCategoryId: id.value });
    selectedRow.value = [];
    selectedRowKeys.value = [];
}

function del () {
    Object.assign(clipboard, { opera: 'delete', value: ref(selectedRow.value) });
    selectedRow.value = [];
    selectedRowKeys.value = [];
    move();
}

function paste () {
    move()
}

function move () {
    let selected = [];
    clipboard.value.map(e => {
        if (selected.length < 10) {
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
            message: selected.length === 1 ? `是否要删除${selected.join('，')}？这会同时删除他们所有子分类。` : `是否要删除${selected.join('，')}等${clipboard.value.length}项？这会同时删除他们所有子分类。`
        },
    }
    let opera = messageList[clipboard.opera];

    let confirmOpt = {
        title: opera.message,
        onOk: () => {
            if (clipboard.opera === "delete" && !confirm(`这个操作无法还原，需要二次确认。${opera.message}`)){
                return;
            }
            let param = { 
                to: entity.ID,  
                opera: clipboard.opera,
                value: clipboard.value.map(e => e.ID).join(','),
                srcCategoryId: clipboard.srcCategoryId
            };
            Object.assign(clipboard, {opera: "",value: [] });
            axios.post("/api/cms/category/MoveCategory", param).then(msg => {
                if (msg.success) {
                    message.success(`${opera.opera}成功`);
                    treeRef.value.reSync(entity.ID);
                    getCategory(entity.ID, true);
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
        let existIndex = selectedRow.value.findIndex(e => e.UniqueCode === record.UniqueCode);
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
        disabled: record.CategoryType === 4,
        name: record.UniqueCode
    }),
    selectedRowKeys
}

const columns = ref([
    {
        key: "type",
        title: "类型",
        dataIndex: "CategoryType",
        width: 100
    },
    {
        title: "栏目名称",
        dataIndex: "Name",
    },
    {
        title: "栏目编号",
        dataIndex: "UniqueCode",
    },
    {
        key: "status",
        title: "栏目状态",
        dataIndex: "CategoryStatus",
    },
]);
</script>

<style lang="less">
@import '@/assets/less/form-list.less';
#category-list {
    .tab-container {
        margin: 0;        
        .page-title-area {
            padding: 0 16px;
            font-size: 16px;
            line-height: 46px;
            min-width: 12em;
        }
        .ant-tabs-content-holder {
            padding: 0 16px;
        }
    }

    .dashboard {
        position: relative;
    }

    .mt{
        margin-top: 20px;
    }

    .mb{
        margin-bottom: 20px;
    }
}
</style>
