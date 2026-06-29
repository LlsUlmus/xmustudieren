<template>
    <a-form>
        <a-row :gutter="24">
            <a-col span="6">
                <a-form-item label="项目名称" class="no-margin">
                    <a-input placeholder="输入项目名称" v-model:value="props.params.achieved" @change="onEnter" />
                </a-form-item>
            </a-col>
            <a-col span="6">
                <a-form-item label="学生姓名">
                    <a-input placeholder="输入学号或姓名" v-model:value="props.params.projOwner" @change="onEnter" :disabled="props.params.lockOwner"/>
                </a-form-item>
            </a-col>
            <a-col span="6">
                <a-form-item label="指导教师">
                    <a-input placeholder="输入导师工号或姓名" v-model:value="props.params.guideTeacher" @change="onEnter" :disabled="props.params.lockTeacher"/>
                </a-form-item>
            </a-col>
            <a-col span="6">
                <a-form-item label="刊物名称">
                    <a-input placeholder="输入学术刊物（会议）名称及刊号" v-model:value="props.params.journal" @change="onEnter"/>
                </a-form-item>
            </a-col>
        </a-row>
        <a-row :gutter="24">
            <a-col span="4">
                <dict-select :dict="statusDict" width="100%" v-model:value="props.params.projStatus" @change="onEnter" :disabled="projStatus[0].length <= 1" />
            </a-col>
        </a-row>
    </a-form>
</template>


<script setup>
import app from '@/app'

const props = defineProps({
    projStatus: {
        type: Array,
        default: []
    },
    params: Object
});

const projStatus = props.projStatus;
const statusDict = {
    name: "项目状态",
    entries: [
        { DataKey: projStatus[0], DataValue: "请选择状态" },
        { DataKey: projStatus[1], DataValue: "申请中" },
        { DataKey: projStatus[2], DataValue: "待指导老师审核" },
        { DataKey: projStatus[3], DataValue: "待学院审核" },
        { DataKey: projStatus[4], DataValue: "已审核" },
        { DataKey: projStatus[5], DataValue: "已取消" },
        // ...projStatus[0].split(',').map(status => ({ DataKey: status, DataValue: app.toText("申请表状态", status) })),
    ].filter(item => item.DataKey !== ""),
}
const emits = defineEmits(["onEnter"]);

function onEnter () {
    emits("onEnter");
}
</script>

<style lang="less">
.no-margin {
    margin-bottom: 0;
}
</style>