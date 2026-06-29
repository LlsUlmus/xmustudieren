<template>
    <div id="content-list" class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    内容管理
                </div>
            </template>
        </a-tabs>
        <div class="page-pane no-padding-bottom">
            <a-alert tpe="info" message="注意事项" v-if="category.SpecialMessageInContent" class="mb" showIcon>
                <template #description>
                    <ul>
                        <li v-for="(v, k) in category.SpecialMessageInContent.split('\n')" :key="k" v-html="v"></li>
                    </ul>
                </template>
            </a-alert>
            <div class="detail-view">
                <a-space>
                    <a-button type="primary" @click="goTo('0000000000000000000000')" :disabled="categoryId === guidEmpty"><a-icon icon="plus-outlined"></a-icon>添加文章</a-button>
                    <a-button :disabled="!selectedRow.length" @click="doer('publish', true)">发布</a-button>
                    <a-button :disabled="!selectedRow.length" @click="doer('withdrawal', true)">撤稿</a-button>
                    <a-divider type="vertical" />
                    <a-button :disabled="!selectedRow.length" @click="doer('cut', false)">剪切</a-button>
                    <a-button :disabled="!selectedRow.length" @click="doer('copy', false)">复制</a-button>
                    <a-button :disabled="!clipboard.value.length" @click="move()">粘贴</a-button>
                    <a-divider type="vertical" />
                    <a-button :disabled="!selectedRow.length" @click="doer('delete', true)">删除</a-button>
                    <a-divider type="vertical" />
                    <content-searcher :category="categoryId" @onSearch="onDataChanged" ref="searcher" :page="pagination.current" :page-size="pagination.pageSize" />
                </a-space>
    
                <a-table class="main-table" :columns="columns" :rowSelection="rowSelection"
                :data-source="data" :pagination="pagination" @change="onPagination">
                    <template #headerCell="{ column }">
                        <template v-if="column.key === 'operation'">
                            <span>
                                <div class="reflush" @click="onPagination()">
                                    <sync-outlined />刷新
                                </div>
                            </span>
                        </template>
                    </template>
                    <template #bodyCell="{ column, text, record }">
                        <template v-if="column.dataIndex === 'Topic'">
                            <a-space>
                                <picture-outlined v-if="record.FeaturedImage" />
                                {{ text }}
                            </a-space>
                        </template>
                        <template v-if="column.dataIndex === 'VerifyStatus'">
                            {{ verifyStatusEnums[text] }}{{ record.TopMost ? `[${topMostEnums[record.TopMost]}]` : "" }}
                        </template>
                        <template v-if="column.key === 'operation'">
                            <a-space>
                                <span class="a-btn" @click="goTo(record.ID)">[编辑]</span>
                            </a-space>
                        </template>
                    </template>
                </a-table>
            </div>
            
        </div>
    </div>
</template>

<script setup>
import router from '@/routers';
import { ref, reactive, watchEffect, watch, onMounted, nextTick } from 'vue';
import ContentSearcher from './content-searcher.vue'
import { message, Modal } from 'ant-design-vue';
import axios from "@/axios"
import { loadCategory } from '../useCategories';
const guidEmpty = "00000000-0000-0000-0000-000000000000";

const props = defineProps({
    category: String
})
const verifyStatusEnums = {
    0: "未审核",
    1: "已发布",
    2: "已撤稿",    
}

const topMostEnums = {
    0: "普通",
    1: "置顶",
    2: "头条",
    3: "置顶头条"
}
const categoryId = ref("");
let lastCategoryId = "";
let category = reactive({
    ID: "",
    Name: "",
    SpecialMessageInCategory: "",
    SpecialMessageInContent: "",
    UniqueCode: ""
});
watchEffect(async () => {
    categoryId.value = props.category;
    if (props.category != lastCategoryId && searcher.value) {
        await nextTick();
        searcher.value.onSearch();
        lastCategoryId = props.category;
    }

    if (!!categoryId.value && categoryId.value !== guidEmpty) {
        let msg = await loadCategory(categoryId.value);
        category = Object.assign(category, msg.data);
        document.title = `${category.Name} - 内容管理`;
    } else {
        document.title = `内容管理`;
    }
})


const searcher = ref();
let data = ref([]);
let pagination = reactive({});
function onDataChanged (msg) {
    data.value = msg.data;
    Object.assign(pagination, {
        current: msg.page,
        pageSize: msg.pageSize,
        total: msg.totalRow
    });
}

async function onPagination (pg) {
    Object.assign(pagination, pg);
    await nextTick();
    searcher.value.onSearch();
}

function goTo (artId) {
    let url = router.resolve({
        name: "cms-detail",
        query: {
            id: artId,
            categoryId: categoryId.value
        }
    });
    window.open(url.fullPath);
}

const columns = ref([
    {
        title: "名称",
        dataIndex: "Topic",
    },
    {
        key: "UniqueCode",
        title: "唯一编号",
        dataIndex: "UniqueCode",
        width: 200
    },
    {
        title: "状态",
        dataIndex: "VerifyStatus",
        width: 200
    },
    {
        title: "发布时间",
        dataIndex: "ReleaseTime",
        width: 200
    },
    {
        key: "operation",
        title: "操作",
        dataIndex: "ID",
        width: 200
    },
]);

// 复制和粘贴功能
let selectedRow = ref([]);
const clipboard = reactive({
    opera: "",
    value: [],
    srcCategoryId: "",
});
let selectedRowKeys = ref([]);
function doer (opera, immediately) {
    Object.assign(clipboard, { opera: opera, value: ref(selectedRow.value), srcCategoryId: categoryId.value });
    selectedRow.value = [];
    selectedRowKeys.value = [];

    if (immediately) {
        move();
    }
}

function paste () {
    move()
}

function move () {
    let selected = [];
    clipboard.value.map(e => {
        if (selected.length < 10) {
            selected.push(`《${e.Topic}》`);
        }
    })

    let messageList = {
        "publish": {
            opera: "发布",
            message: selected.length === 1 ? `是否要发布${selected.join('，')}` : `是否要发布${selected.join('，')}等${clipboard.value.length}项？`
        },
        "withdrawal": {
            opera: "撤稿",
            message: selected.length === 1 ? `是否要将${selected.join('，')}撤销？` : `是否要将${selected.join('，')}等${clipboard.value.length}项撤销？`
        },
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
            message: selected.length === 1 ? `是否要删除${selected.join('，')}？` : `是否要删除${selected.join('，')}等${clipboard.value.length}项？`
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
                to: categoryId.value,  
                opera: clipboard.opera,
                value: clipboard.value.map(e => e.ID).join(','),
                srcCategoryId: clipboard.srcCategoryId
            };
            Object.assign(clipboard, {opera: "",value: [] });
            axios.post("/api/cms/articles/MoveArticle", param).then(msg => {
                if (msg.success) {
                    message.success(`${opera.opera}成功`);
                    searcher.value.onSearch();
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
    getCheckboxProps: record => {
        return {
            name: record.UniqueCode
        }
    },
    selectedRowKeys
}
</script>

<style lang="less">
@import '@/assets/less/form-list.less';
#content-list {
    overflow: auto;
    .mb {
        margin: 20px;
    }
}

.detail-view .main-table {
    margin-top: 16px; 
}

</style>
