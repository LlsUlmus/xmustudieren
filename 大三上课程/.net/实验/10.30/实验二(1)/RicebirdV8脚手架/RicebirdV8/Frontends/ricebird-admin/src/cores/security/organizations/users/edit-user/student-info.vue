<template>
    <form-view v-bind="$attrs" @submit="onBasicSubmit" :model="userModel" :labelCol="{ span: 2 }"
        :rules="rules">
        <div class="sub-view-title">学生信息</div>
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
                    <a-form-item name="Grade" label="年级" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Grade" placeholder="请输入年级" show-count :maxlength="10" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="Major" label="专业" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Major" placeholder="请输入专业" show-count :maxlength="20" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="GraduadeTime" label="预计毕业时间" :labelCol="{ span: 4 }">
                        <date-picker v-model:value="userModel.GraduadeTime" placeholder="请选择预计毕业时间" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="Nationality" label="国籍" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Nationality" placeholder="请输入国籍" show-count :maxlength="20" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="Ethnic" label="民族" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Ethnic" placeholder="请输入民族" show-count :maxlength="10" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="BirthPlace" label="籍贯" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.BirthPlace" placeholder="请输入籍贯" show-count :maxlength="20" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="Politics" label="政治面貌" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Politics" placeholder="请输入政治面貌" show-count :maxlength="20" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="StudentSource" label="生源地" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.StudentSource" placeholder="请输入生源地" show-count :maxlength="20" />
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
    "BankName": [
        { "type": "string", "min": 0, "max": 40, "message": "银行名称限40字" }
    ],
    "CardNumber": [
        { "type": "string", "min": 0, "max": 19, "message": "银行卡号限19位，请不要输入空格" }
    ],
    "Grade": [
        { "type": "string", "min": 0, "max": 10, "message": "年级限10字" }
    ],
    "Major": [
        { "type": "string", "min": 0, "max": 20, "message": "专业限20字" }
    ],
    "Department": [
        { "type": "string", "min": 0, "max": 30, "message": "系所限30字" }
    ],
    "GraduadeTime": [
        { "type": "string", "min": 0, "max": 10, "message": "预计毕业时间限10字" }
    ],
    "Nationality": [
        { "type": "string", "min": 0, "max": 20, "message": "国籍限20字" }
    ],
    "Ethnic": [
        { "type": "string", "min": 0, "max": 10, "message": "民族限10字" }
    ],
    "BirthPlace": [
        { "type": "string", "min": 0, "max": 20, "message": "籍贯限20字" }
    ],
    "Politics": [
        { "type": "string", "min": 0, "max": 20, "message": "政治面貌限20字" }
    ],
    "StudentSource": [
        { "type": "string", "min": 0, "max": 20, "message": "生源地限20字" }
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