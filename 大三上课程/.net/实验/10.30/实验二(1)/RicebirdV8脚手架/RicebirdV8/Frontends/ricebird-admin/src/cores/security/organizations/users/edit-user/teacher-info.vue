<template>
    <form-view v-bind="$attrs" @submit="onBasicSubmit" :model="userModel" :labelCol="{ span: 2 }"
        :rules="rules">
        <div class="sub-view-title">老师信息</div>
        <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存用户时发生错误：`">
            <template #description>
                <ul>
                    <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                </ul>
            </template>
        </a-alert>
        <div class="common-infor">
            <a-row>
                <a-col span="12">
                    <a-form-item name="Title" label="职称" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Title" placeholder="请输入职称" show-count :maxlength="20" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="Position" label="职务" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Position" placeholder="请输入职务" show-count :maxlength="20" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="Education" label="学历" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Education" placeholder="请输入学历" show-count :maxlength="20" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="QQ" label="QQ号" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.QQ" placeholder="请输入QQ号" show-count :maxlength="30" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="OfficePhone" label="办公室电话" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.OfficePhone" placeholder="请输入办公室电话" show-count :maxlength="30" />
                    </a-form-item>
                </a-col>
            </a-row>
        </div>
    </form-view>
</template>

<script setup>
import { ref, reactive, inject, watch } from 'vue'
import axios from '@/axios'
import { message } from 'ant-design-vue';
import _ from 'lodash'

const userId = inject("id");
const form = ref();
const userModel = inject("userModel");

watch(userId, nv => {
    if (nv === userModel.ID) return;
    if (form.value && form.value.clearValidate) {
        form.value.clearValidate();
    }
}, {immediate: true});

// -- 登录信息 -- //
const rules = reactive({
    "Title": [
        { "type": "string", "min": 0, "max": 20, "message": "职称限20字" }
    ],
    "Position": [
        { "type": "string", "min": 0, "max": 20, "message": "职务限20字" }
    ],
    "Education": [
        { "type": "string", "min": 0, "max": 20, "message": "学历限20字" }
    ],
    "QQ": [
        { "type": "string", "min": 0, "max": 30, "message": "QQ号限30字" }
    ],
    "OfficePhone": [
        { "type": "string", "min": 0, "max": 30, "message": "办公室电话限30字" }
    ]
});
const errors = ref([]);
async function onBasicSubmit({ formRef, loading }) {
    try {
        form.value = formRef.value;
        await formRef.value.validate();
        let msg = await axios.post("/api/users/SaveUser", userModel);
        if (msg.success) {
            Object.assign(userModel, msg.data);
            errors.value = [];
            message.success("保存成功");
            userId.value = userModel.ID;
        } else {
            errors.value = msg.errorStrings;
        }
    } catch {} finally {
        loading.value = false;
    }
}
// -- 登录信息 -- //
</script>

<style lang="less" scoped>
.common-infor {
    margin: 24px 0;
}
</style>