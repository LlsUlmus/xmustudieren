<template>
    <a-modal :attrs="$attrs" v-model:open="open" destroyOnClose :maskClosable="false">
        <a-form :rules="rules" :model="model" layout="vertical" ref="formRef" class="form">
            <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存部门时发生错误：`">
                <template #description>
                    <ul>
                        <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                    </ul>
                </template>
            </a-alert>
            <a-form-item label="部门名称" name="Name">
                <a-input v-model:value="model.Name" placeholder="部门名称" />
            </a-form-item>
            <a-form-item label="部门简称" name="ShortName">
                <a-input v-model:value="model.ShortName" placeholder="部门简称" />
            </a-form-item>
            <a-row>
                <a-col span="12">
                    <a-form-item label="上级部门" name="ParentId" help="修改请用剪切功能">
                        {{ parent.Name }}
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item label="部门架构" name="SchemaName" help="选择学院时，才可以成为 负责学院 和 所属学院 的目标">
                        <dict-radio-button v-model:value="model.SchemaName" dict="部门类型" remove="-1"/>
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item label="同步编号" name="Code" help="此字段仅供系统同步模块使用！">
                        {{ model.Code === "" ? '未生成编号' : model.Code }}
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item label="部门来源" name="Source" help="">
                        {{ parent.Source }}
                    </a-form-item>
                </a-col>
            </a-row>
            <a-form-item label="严格学分模式：" name="StrictCreditStrategy">
                <a-checkbox v-model:checked="model.StrictCreditStrategy" style="width: 100%;"> 设置为严格学分模式</a-checkbox>
            </a-form-item>
            <a-form-item label="排序号（升序）：" name="DisplayOrder">
                <a-input-number v-model:value="model.DisplayOrder" placeholder="排序号" style="width: 100%;" />
            </a-form-item>
        </a-form>
        <template #footer>
            <a-checkbox v-model:checked="model.IsDefault" style="float: left;">设为默认部门</a-checkbox>
            <a-button key="back" @click="onCancel">取消</a-button>
            <a-button key="submit" type="primary" :loading="loading" @click="onOk">提交</a-button>
        </template>
    </a-modal>
</template>

<script setup>
import app from '@/app'
import { ref, reactive, inject } from 'vue'
import useModal from '@/components/modals/useModal'
import axios from '@/axios'
import { loadTree, dataSource, flatTree } from './useDepartment'

const {
    open, loading, 
    onOk, onCancel,
    close, showModal
} = useModal(onOpen, processor);

defineExpose({
    showModal
});

// -- 业务逻辑，点击确定后应该如何 -- //
// STEP1: 填一个空模型
const emptyModel = {
    ID: app.GUID_EMPTY,
    Name: "",
    Code: "",
    DisplayOrder: 0,
    ParentId: app.GUID_EMPTY,
    SchemaName: "普通部门",
    StrictCreditStrategy: true,
    ShortName: "",
    Source: "后台新建",
    IsDefault: false,
};
const model = reactive({ ...emptyModel });
// STEP2：写一下数据验证规则
const rules = {
    Name: { required: true }
}
const formRef = ref("");
const errors = ref([]);
const parent = reactive({ ...emptyModel });

function onOpen (input) {
    bindModel(input);
    errors.value = [];
    // 窗口打开时，处理这里的逻辑，onOpen的参数就是showModal输入的参数
    // STEP3：把输入的参数合并到模型里
}

function bindModel (input) {
    console.log(input);
    if (input.Name === '所有部门' || input.Name === '根部门') {
        input.Name = '';
    }

    Object.assign(model, emptyModel, input);
    
    if (model.ParentId === app.GUID_EMPTY) {
        parent.Name = "根部门";
        parent.ID = app.GUID_EMPTY;
    } else {
        var par = flatTree.find(e => e.ID === model.ParentId);
        Object.assign(parent, par);
    }
}

const treeRef = inject("treeRef");
async function processor () {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {
        await formRef.value.validate();
        // STEP4：处理一下提交函数
        let msg = await axios.post("/api/depart/SaveDepart", model);
        if (!msg.success) {
            errors.value = msg.errorStrings;
            return;
        } else {
            bindModel(emptyModel);
            treeRef.value.reSync();
            close();
        }
    } catch (err) {

    }
}
</script>

<style scoped lang="less"></style>