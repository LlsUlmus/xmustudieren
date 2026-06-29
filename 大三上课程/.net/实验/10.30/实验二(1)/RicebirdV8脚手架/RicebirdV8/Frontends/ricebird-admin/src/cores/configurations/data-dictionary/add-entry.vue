<template>
    <a-modal :attrs="$attrs" v-model:open="open" @ok="onOk" @cancel="onCancel" okText="确定" cancelText="取消" :confirm-loading="loading" destroyOnClose :maskClosable="false">
        <a-form :rules="rules" :model="model" layout="vertical" ref="formRef" class="form">
            <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存字典项时发生错误：`">
                <template #description>
                    <ul>
                        <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                    </ul>
                </template>
            </a-alert>
            <a-row :gutter="16">
                <a-col span="12">
                    <a-form-item label="键" name="DataKey">
                        <a-input v-model:value="model.DataKey" placeholder="字典名称，限20字" show-count :maxlength="20"/>
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item label="值" name="DataValue">
                        <a-input v-model:value="model.DataValue" placeholder="字典名称，限20字" show-count :maxlength="20"/>
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row :gutter="16">
                <a-col span="8">
                    <a-form-item label="排序号（升序）" name="DisplayOrder">
                        <a-input-number v-model:value="model.DisplayOrder" placeholder="升序排序号" style="width: 100%;" />
                    </a-form-item>
                </a-col>
                <a-col span="8">
                    <a-form-item label="是否启用" name="Enable">
                        <a-switch v-model:checked="model.Enable" checkedChildren="是" unCheckedChildren="否"/>
                    </a-form-item>
                </a-col>
                <a-col span="8">
                    <a-form-item label="是否显示" name="Visible">
                        <a-switch v-model:checked="model.Visible" checkedChildren="是" unCheckedChildren="否"/>
                    </a-form-item>
                </a-col>
            </a-row>
        </a-form>
    </a-modal>
</template>

<script setup>
import app from '@/app'
import { ref, reactive } from 'vue'
import useModal from '@/components/modals/useModal'
import axios from '@/axios'

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
    "DataDictionaryId": "",
    "DataKey": "",
    "DataValue": "",
    "Enable": false,
    "Visible": true,
    "DisplayOrder": 100,
};
const model = reactive({ ...emptyModel });
// STEP2：写一下数据验证规则
const rules = {
    DataKey: { required: true, max: 20 },
    DataValue: { required: true, max: 20 },
    DisplayOrder: { required: true },
}
const formRef = ref("");
const errors = ref([]);

function onOpen (inputModel) {
    errors.value = [];
    // 窗口打开时，处理这里的逻辑，onOpen的参数就是showModal输入的参数
    // STEP3：把输入的参数合并到模型里
    Object.assign(model, inputModel);
}

async function processor () {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {
        await formRef.value.validate();
        // STEP4：处理一下提交函数
        let msg = await axios.post("/api/dict/SaveEntry", model);
        if (!msg.success) {
            errors.value = msg.errorStrings;
            return;
        } else {
            Object.assign(model, emptyModel);
            close();
        }
    } catch (err) {

    }
}
</script>

<style scoped lang="less"></style>