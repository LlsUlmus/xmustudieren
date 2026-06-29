<template>
    <div id="menu-manager" class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    菜单管理
                </div>
            </template>
        </a-tabs>
        
        <div class="page-pane no-padding-bottom">
            <div class="detail-view">
                <a-space class="searcher-area">
                    <a-button v-if="app.succeed('保存菜单')" type="primary" @click="editItem(app.GUID_EMPTY)"><a-icon icon="plus-outlined"></a-icon>添加菜单项</a-button>
                    <a-button v-if="app.succeed('自动构建菜单')" @click="build"><a-icon icon="reload-outlined"></a-icon>自动添加菜单</a-button>
                    <a-button v-if="app.succeed('重新排列菜单')" @click="reorderItem()">重建菜单顺序号</a-button>
                </a-space>
                <a-table class="table-area" :columns="columns" :pagination="false" rowKey="ID" :data-source="dataSource.data">
                    <template #headerCell="{ column }">
                        <template v-if="column.key === 'operation'">
                            <span>
                                <div class="reflush" @click="load" v-if="app.succeed('重新加载菜单')">
                                    <sync-outlined />刷新
                                </div>
                            </span>
                        </template>
                    </template>
                    <template #bodyCell="{ column, text, record }">
                        <template v-if="column.key === 'Icon'">
                            <a-icon :icon="text"></a-icon> {{ text || '未设置图标' }}
                        </template>
                        <template v-if="column.key === 'LinkType'">
                            {{ app.toText("链接类型", text) }}
                        </template>
                        <template v-if="column.key === 'LinkTo'">
                            <span v-if="record.LinkType === 3">-</span>
                            <span v-else-if="record.LinkType === 2">{{ getMenuByPath(text) }}</span>
                            <a v-else-if="record.LinkType === 1" target="_blank" :href="text">{{ text }}</a>
                        </template>
                        <template v-if="column.key === 'Visibility'">
                            {{ enums.visibility[text] }} [{{ record.DisplayOrder }}]
                        </template>
                        <template v-if="column.key === 'operation'">
                            <span class="a-btn" v-if="app.succeed('保存菜单')" @click="editItem(record.ID)">[编辑]</span>
                            <span class="a-btn" v-if="app.succeed('删除菜单')" @click="removeItem(record.ID)">[删除]</span>
                        </template>
                    </template>
                </a-table>
                <EditMenu v-model:open="open" :id="itemId"  v-if="app.succeed('保存菜单')"/>
            </div>
        </div>
    </div>
</template>

<script setup lang="jsx">
import app from '@/app'
import { ref, provide, createVNode } from 'vue'
import { buildMenus, getMenuByPath } from '@/routers/buildMenu'
import enums from './enum'
import axios from '@/axios'
import { message } from 'ant-design-vue'
import EditMenu from './EditMenu.vue'
import { loadTree, dataSource, tree } from './useMenu'

const open = ref(false);
const itemId = ref(app.GUID_EMPTY);
const parents = ref([]);
provide("parents", parents)
provide("load", load)

async function build() {
    const vnode = createVNode('span', {}, "正在向后端导入菜单项...")
    let loading = message.loading({
        content: vnode
    });
    await buildMenus(vnode);
    await load(true);
    loading();
}

async function load(force = false) {
    let tree = await loadTree(force);
    parents.value = [];
    parents.value.push({
        value: app.GUID_EMPTY,
        icon: "fork-outlined",
        label: "根级"
    });

    for (let item of tree) {
        let menu = {
            value: item.ID,
            icon: item.Icon,
            label: item.Name
        }
        parents.value.push(menu);
    }
}

async function editItem (id) {
    open.value = true;
    itemId.value = id;
}

async function removeItem(id) {
    let confirm = await app.modals.removeConfirm("是否要删除此项，该操作无法还原。");
    if (!confirm) return;
    let msg = await axios.post("/api/menu/RemoveMenu", { id });
    if (!msg.success) {
        message.error(msg.msg);
    } else {
        message.success("成功删除");
    }
    load(true);
}

async function reorderItem() {
    await axios.post("/api/menu/ReorderMenu");
    load(true);
}

load();

const columns = ref([
    {
        title: "菜单名称",
        dataIndex: "Name",
        key: "Name",
        width: 285, 
    },
    {
        title: "菜单图标",
        dataIndex: "Icon",
        key: "Icon",
    },
    {
        title: "链接类型",
        dataIndex: "LinkType",
        key: "LinkType",
    },
    ,
    {
        title: "链接指向",
        dataIndex: "LinkTo",
        key: "LinkTo",
        width: 370, 
    },
    {
        title: "是否显示[升序号]",
        dataIndex: "Visibility",
        key: "Visibility",
    },
    {
        title: "操作",
        dataIndex: "operation",
        key: "operation",
    },
]);
</script>

<style lang="less">
#menu-manager {
    width: 100%;
    height: @content-height;
    overflow: auto;
}
</style>