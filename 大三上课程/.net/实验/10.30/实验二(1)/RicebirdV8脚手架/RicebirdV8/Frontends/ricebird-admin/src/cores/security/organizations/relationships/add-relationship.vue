<template>
    <a-modal :attrs="$attrs" v-model:open="open" @ok="onOk" @cancel="onCancel" okText="确定" cancelText="取消" :confirm-loading="loading" destroyOnClose :maskClosable="false">
        <template #title>
            向 {{ model.departName }} 添加组织机构关系
        </template>
        <a-form :rules="rules" :model="model" layout="vertical" ref="formRef" class="form">
            <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存关系时发生错误`">
                <template #description>
                    <ul>
                        <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                    </ul>
                </template>
            </a-alert>
            <a-form-item label="选择角色" name="roleId">
                <role-select :value="model.roleId" width="100%" :disableSetFor="model.departId === app.GUID_EMPTY ? 1 : 0"  prefix="" 
                    empty="请选择角色" disableEmpty @change="model.roleChanged($event)" />
            </a-form-item>
            <a-form-item label="填写用户" name="code" help="填写用户证件号，用逗号分隔，半角全角的逗号都可以。">
                <a-input v-model:value="model.code" placeholder="在此填写证件号" />
            </a-form-item>
        </a-form>
    </a-modal>
</template>

<script setup>
import app from '@/app'
import { ref, reactive } from 'vue'
import useModal from '@/components/modals/useModal'
import axios from '@/axios'
import { getDepart } from '@/cores/security/organizations/departs/useDepartment'

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
    id: app.GUID_EMPTY,
    userId: app.GUID_EMPTY,
    roleId: app.GUID_EMPTY,
    departId: app.GUID_EMPTY,
    departName: "",
    code: "",
    roleChanged: function (roleId) {
        if (this.roleId === roleId) return;
        this.roleId = roleId;
    },
};
const model = reactive({ ...emptyModel });
// STEP2：写一下数据验证规则
const rules = {
    roleId: { 
        required: true, 
        validator: (rule, value, cb, source, options) => new Promise((resolve, reject) => {
            if (value === app.GUID_EMPTY) {
                reject("必须选择一个角色")
            } else {
                resolve();
            }
        }),
    },
    code: { required: true, message: "必须填写至少一个证件号" }
}
const formRef = ref("");
const errors = ref([]);
const title = ref("");
function onOpen (dptId, roleId) {
    errors.value = [];
    // 窗口打开时，处理这里的逻辑，onOpen的参数就是showModal输入的参数
    // STEP3：把输入的参数合并到模型里
    Object.assign(model, emptyModel);
    model.departId = dptId;
    model.roleId = roleId;
    model.departName = getDepart(dptId).title || "所有部门";
}

async function processor () {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {
        loading.value = true;
        await formRef.value.validate();
        // STEP4：处理一下提交函数
        let params = {
            roleId: model.roleId,
            departId: model.departId,
            code: model.code
        }
        let msg = await axios.post("/api/relationships/AddRelationships", params);
        if (!msg.success) {
            errors.value = msg.errorStrings;
            return;
        } else {
            Object.assign(model, emptyModel);
            close();
        }
    } catch (err) {

    } finally {
        loading.value = false;
    }
}
</script>

<style scoped lang="less"></style>