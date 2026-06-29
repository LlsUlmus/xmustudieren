<template>
    <div id="api-manager" class="tab-page-container">
        <a-tabs>
            <template #leftExtra>
                <div class="page-title-area">
                    功能和接口管理
                </div>
            </template>
        </a-tabs>
        
        <div class="page-pane no-padding-bottom">
            <div class="detail-view">
                <a-space class="searcher-area">
                    <dict-select :dict="enums" v-model:value="status" @change="filter" width="188px" />
                    <dict-select :dict="apiGroups" v-model:value="group" @change="filter" width="230px" />
                    <dict-select dict="接口类型" v-model:value="type" @change="filter"/>
                    <dict-select dict="授权等级" v-model:value="level" @change="filter"/>
                </a-space>
                <a-table class="table-area" :columns="columns" :pagination="false" rowKey="Name" :data-source="dataSource.data">
                    <template #bodyCell="{ column, text, record }">
                        <template v-if="column.key === 'ApiType'">
                            <a-tooltip :title="`接口状态：${statusIcons[record.Status].text}`">
                                <a-icon class="icon-mr" :class="{ [statusIcons[record.Status].color]: true }" :icon="statusIcons[record.Status].icon" />
                            </a-tooltip>
                            {{ app.toText("接口类型", text) }}
                        </template>
                        <template v-if="column.key === 'AuthorizeLevel'">
                            {{ record.ApiType === 0 ? '-' : app.toText("授权等级", text) }}
                        </template>
                        <template v-if="column.key === 'Success'">
                            {{ record.Success.Count }} 次 / {{ toTotal(record.Success) }} / {{ toSql(record.Success) }}
                        </template>
                        <template v-if="column.key === 'Failure'">
                            {{ record.Failure.Count }} 次 / {{ toTotal(record.Failure) }} / {{ toSql(record.Failure) }}
                        </template>
                        <template v-if="column.key === 'Exceptions'">
                            {{ record.Exceptions.Count }} 次 / {{ toTotal(record.Exceptions) }} / {{ toSql(record.Exceptions) }}
                        </template>
                    </template>
                </a-table>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import app from '@/app';
import { createIcon } from '@/components/Icons';
import {
    loadTree,
    dataSource,
    apiGroups
} from './useApi'
// -- useApi -- //
let type = ref(-1);
let level = ref(-1);
let status = ref(-1);
let group = ref("");

async function loadApis() {
    await loadTree();
    filter();
}

onMounted(() => loadApis());

async function filter () {
    // let func = (v, f, e) => v.value === -1 || e[f] === v.value;
    dataSource.query()
        .whereIf("ApiType", type, -1)
        .whereIf("AuthorizeLevel", level, -1)
        .whereIf("Status", status, -1)
        .whereIf("ApiGroup", group, "")
        .end()
}
// -- useApi -- //
function toTotal(counter) {
    if (counter.Count === 0) {
        return '-'
    } else {
        return `${(parseFloat(counter.TotalEllapsed) / parseFloat(counter.Count)).toFixed(2)}ms`
    }
}

function toSql(counter) {
    if (counter.UseSqlCount === 0) {
        return '-'
    } else {
        return `${(parseFloat(counter.SqlUse) / parseFloat(counter.UseSqlCount)).toFixed(2)}次`
    }
}

const statusIcons = {
    0: {
        icon: "CheckCircleFilled",
        color: "color-success",
        text: "在线"
    },
    1: {
        icon: "CheckCircleFilled",
        color: "color-success",
        text: "在线"
    },
    2: {
        icon: "CloseCircleFilled",
        color: "color-error",
        text: "离线"
    },
    3: {
        icon: "WarningFilled",
        color: "color-error",
        text: "废弃"
    }
}

const enums = {
    name: "接口状态",
    entries: [
        {
            DataKey: -1,
            DataValue: '任意状态',
        },
        {
            DataKey: 1,
            DataValue: "在线",
            label: [createIcon("CheckCircleFilled", { class: { ["color-success"]: true}, style: { marginRight: "10px" } }), '在线'],
        },
        {
            DataKey: 2,
            DataValue: "离线",
            label: [createIcon("CloseCircleFilled", { class: { ["color-error"]: true}, style: { marginRight: "10px" } }), '离线'],
        },
        {
            DataKey: 3,
            DataValue: "废弃",
            label: [createIcon("WarningFilled", { class: { ["color-warning"]: true}, style: { marginRight: "10px" } }), '废弃'],
        },
    ]
}

const columns = ref([
    {
        title: "接口名称",
        dataIndex: "Name",
        width: 285,
    },
    {
        title: "接口类型",
        dataIndex: "ApiType",
        key: "ApiType",
        width: 150,
    },
    {
        title: "授权等级",
        dataIndex: "AuthorizeLevel",
        key: "AuthorizeLevel",
        width: 150,
    },
    {
        title: "成功计数器（访问数/平均耗时/查询次数）",
        key: "Success",
    },
    {
        title: "失败计数器（访问数/平均耗时/查询次数）",
        key: "Failure",
    },
    {
        title: "异常计数器（访问数/平均耗时/查询次数）",
        key: "Exceptions",
    },
]);
</script>
<style lang="less">
#api-manager {
    .icon-mr {
        margin-right: 5px;
    }
}
</style>