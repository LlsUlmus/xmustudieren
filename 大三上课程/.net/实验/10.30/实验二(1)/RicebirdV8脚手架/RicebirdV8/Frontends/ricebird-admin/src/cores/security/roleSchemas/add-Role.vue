<template>
    <a-modal :attrs="$attrs" v-model:open="open" @ok="onOk" @cancel="onCancel" okText="确定" cancelText="取消" :confirm-loading="loading" destroyOnClose :maskClosable="false">
        <a-form :rules="rules" :model="model" layout="vertical" ref="formRef" class="form">
            <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存字典时发生错误：`">
                <template #description>
                    <ul>
                        <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                    </ul>
                </template>
            </a-alert>
            <a-form-item label="角色名称" name="Name">
                <a-input v-model:value="model.Name" placeholder="角色名称，限20字" show-count :maxlength="20" />
            </a-form-item>
            <a-row :gutter="16">
                <a-col :span="12">
                    <a-form-item label="适用范围：" name="For">
                        <dict-radio-button v-model:value="model.For" dict="角色适用范围" remove="-1"/>
                    </a-form-item>
                </a-col>
                <a-col :span="12">
                    <a-form-item label="默认权限：" name="NotSetEquals">
                        <dict-radio-button v-model:value="model.NotSetEquals" dict="可访问性" remove="-1"/>
                    </a-form-item>
                </a-col>
            </a-row>
            <a-form-item v-if="model.For === 1" label="显示名称（放空则同角色名）：" name="DisplayAs" help="显示时，使用“{ 部门名 }的{ 显示名称 }”。">
                <a-input v-model:value="model.DisplayAs" placeholder="角色名称，限20字" show-count :maxlength="20" />
            </a-form-item>
            <a-form-item label="排序号（升序）：" name="DisplayOrder">
                <a-input-number v-model:value="model.DisplayOrder" placeholder="排序号" style="width: 100%;" />
            </a-form-item>
        </a-form>
        <template #footer>
            <a-checkbox v-model:checked="model.UseAsPrincipal" style="float: left;">此角色视为用户登录身份的一种</a-checkbox>
            <a-button key="back" @click="onCancel">取消</a-button>
            <a-button key="submit" type="primary" :loading="loading" @click="onOk">提交</a-button>
        </template>
    </a-modal>
</template>

<script setup>
import app from '@/app'
import { ref, reactive } from 'vue'
import useModal from '@/components/modals/useModal'
import axios from '@/axios'
import { loadTree } from './useRole'

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
    "Name": "",
    "For": 1,
    "DisplayAs": "",
    "NotSetEquals": 1,
    "Menus": [],
    "Permissions": [],
    "FinalMenus": [],
    "FinalPermissions": [],
    "DisplayOrder": 0,
    "UseAsPrincipal": true,
    "ID": app.GUID_EMPTY
};
const model = reactive({ ...emptyModel });
// STEP2：写一下数据验证规则
const rules = {
    Name: { required: true, max: 20 },
    For: { required: true,  },
    NotSetEquals: { required: true,  },
    DisplayOrder: {  required: true },
    DisplayAs: {  max: 20 },
}
const formRef = ref("");
const errors = ref([]);

async function onOpen (id) {
    errors.value = [];
    // 窗口打开时，处理这里的逻辑，onOpen的参数就是showModal输入的参数
    // STEP3：把输入的参数合并到模型里
    if (id === app.GUID_EMPTY) {
        Object.assign(model, emptyModel);
    } else {
        let msg = await axios.post("/api/roles/GetRoleSchema", { id });
        Object.assign(model, msg.data);
    }
}

async function processor () {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {
        await formRef.value.validate();
        // STEP4：处理一下提交函数
        let msg = await axios.post("/api/roles/SaveRoleSchema", model);
        if (!msg.success) {
            errors.value = msg.errorStrings;
            return;
        } else {
            Object.assign(model, emptyModel);
            loadTree(true);
            close();
        }
    } catch (err) {

    }
}
</script>

<style scoped lang="less"></style>