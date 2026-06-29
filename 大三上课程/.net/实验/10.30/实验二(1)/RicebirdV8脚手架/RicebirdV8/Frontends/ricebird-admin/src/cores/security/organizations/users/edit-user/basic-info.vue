<template>
    <form-view v-bind="$attrs" @submit="onBasicSubmit" :model="userModel" :labelCol="{ span: 2 }"
        :rules="rules">
        <div class="sub-view-title">基本信息</div>
        <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存用户时发生错误：`">
            <template #description>
                <ul>
                    <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                </ul>
            </template>
        </a-alert>
        <div id="各系统间都通用的内容" class="common-infor">
            <a-form-item name="UserName" label="用户名">
                <a-input v-model:value="userModel.UserName" placeholder="请输入用户名" show-count :maxlength="50" />
            </a-form-item>
            <a-form-item name="RealName" label="姓名">
                <a-input v-model:value="userModel.RealName" placeholder="请输入姓名" show-count :maxlength="60" />
            </a-form-item>
            <a-form-item name="Code" label="学/工号">
                <a-input v-model:value="userModel.Code" placeholder="统一认证那里获得的学/工号" show-count :maxlength="30" />
            </a-form-item>
            <a-row>
                <a-col span="12">
                    <a-form-item name="Mobile" label="手机号" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Mobile" placeholder="请输入手机号，此字段必须全系统唯一" show-count :maxlength="11" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="Email" label="邮箱" :labelCol="{ span: 4 }">
                        <a-input v-model:value="userModel.Email" placeholder="请输入邮箱，此字段必须全系统唯一" show-count :maxlength="50" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="Gender" label="性别" :labelCol="{ span: 4 }">
                        <a-radio-group v-model:value="userModel.Gender">
                            <a-radio-button value="男">男</a-radio-button>
                            <a-radio-button value="女">女</a-radio-button>
                        </a-radio-group>
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="Birthday" label="生日" :labelCol="{ span: 4 }">
                        <date-picker v-model:value="userModel.Birthday" placeholder="请选择生日" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="AuditStatus" label="账户状态" :labelCol="{ span: 4 }">
                        <dict-radio-button v-model:value="userModel.AuditStatus" dict="用户状态" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="UserSource" label="用户来源" :labelCol="{ span: 4 }">
                        <dict-select dict="用户来源" prefix="" v-model:value="userModel.UserSource" remove="任意来源" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-row>
                <a-col span="12">
                    <a-form-item name="Level" label="访问级别" :labelCol="{ span: 4 }">
                        {{ app.toText("访问权限", userModel.Level) }}
                        <template #help>
                            <span>如果有多个角色同时对应某个权限。则只要有一个角色允许访问，即允许用户访问。</span>
                        </template>
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="UserSource" label="统一身份认证ID" :labelCol="{ span: 4 }">
                        {{ userModel.OpenId ? userModel.OpenId : "未绑定" }}
                        <template #help>
                            该ID应当等同于工号，并且可以用以识别企业微信相关信息。
                        </template>
                    </a-form-item>
                </a-col>
            </a-row>
        </div>
    </form-view>
</template>

<script setup>
import { ref, reactive, inject, watch } from 'vue'
import app from '@/app';
import axios from '@/axios'
import { message } from 'ant-design-vue';
import _ from 'lodash'

const isNew = inject("isNew");
const title = inject("title");
const userId = inject("id");
const form = ref();
const empty = {
    "RealName": "",
    "CodeType": "教工号",
    "Code": "",
    "DisplayOrder": 0,
    "Mobile": "",
    "Email": "",
    "UserName": "",
    "OpenId": "",
    "Avatar": "",
    "Level": 3,
    "RootDepartId": app.GUID_EMPTY,
    "LockCount": 0,
    "LockTo": "",
    "AuditStatus": 1,
    "UserSource": "后台添加",
    "Gender": "男",
    "Birthday": "",
    "IsPayroll": false,
    "BankName": "",
    "CardNumber": "",
    "ID": app.GUID_EMPTY,

    "Grade": "",
    "Major": "",
    "Department": "",
    "GraduadeTime": "",
    "ReformType": "",
    "Nationality": "",
    "Ethnic": "",
    "BirthPlace": "",
    "Politics": "",
    "StudentSource": "",
    
    "Title": "",
    "Position": "",
    "Education": "",
    "QQ": "",
    "OfficePhone": "",
};
const userModel = inject("userModel");
Object.assign(userModel, empty);

watch(userId, nv => {
    if (nv === userModel.ID) return;
    if (form.value && form.value.clearValidate) {
        form.value.clearValidate();
    }
    if (nv === app.GUID_EMPTY) {
        clearUser();
    } else {
        loadUser(nv);
    }
}, {immediate: true});

function clearUser() {
    title.value = "新建用户"
    isNew.value = true;
    Object.assign(userModel, empty);
}
async function loadUser(id) {
    title.value = "编辑用户"
    isNew.value = false;
    let msg = await axios.post("/api/users/GetUser", { id });
    if (!msg.success) {
        message.error(msg.msg);
        return;
    }
    Object.assign(userModel, msg.data);
}

// -- 登录信息 -- //
const rules = reactive({
    "RealName": [
        { "type": "string", "min": 0, "max": 60, "message": "姓名限60字" },
        { required: true, message: "必须填写姓名" }
    ],
    "Code": [
        { "type": "string", "min": 0, "max": 30, "message": "证件号限30字" },
        { "required": true, "message": "必须填写证件号" },
        {
            validator: function (rule, value, cb, source, options) {
                let userId = userModel.ID;            
                return new Promise((resolve, reject) => {
                    axios.post("/api/users/CodeValidate", { userId, code: value })
                        .then(msg => {
                            if (msg.success) {
                                resolve();
                            } else {
                                reject(msg.msg);
                            }
                        });
                });
            },
            trigger: 'blur'
        }
    ],
    "DisplayOrder": [
        { "type": "integer", "message": "排序号必须是数字" }
    ],
    "Mobile": [
        { "type": "string", "min": 0, "max": 11, "message": "手机号限11位" },
        { "required": true, "message": "必须填写手机号" },
        { "type": "string", "pattern": "^1[3-9]\\d{9}$", "message": "这不是一个合法的手机号" },
        {
            validator: function (rule, value, cb, source, options) {
                let userId = userModel.ID;
                return new Promise((resolve, reject) => {
                    axios.post("/api/users/MobileValidate", { userId, mobile: value })
                        .then(msg => {
                            if (msg.success) {
                                resolve();
                            } else {
                                reject(msg.msg);
                            }
                        });
                });
            },
            trigger: 'blur'
        }
    ],
    "Email": [
        { "type": "string", "min": 0, "max": 50, "message": "电子邮箱限50字" },
        { "required": true, "message": "必须填写电子邮箱" },
        { "type": "string", "pattern": "^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$", "message": "这不是一个合法的电子邮箱名称" },
        {
            validator: function (rule, value, cb, source, options) {
                let userId = userModel.ID;
                return new Promise((resolve, reject) => {
                    axios.post("/api/users/EmailValidate", { userId, email: value })
                        .then(msg => {
                            if (msg.success) {
                                resolve();
                            } else {
                                reject(msg.msg);
                            }
                        });
                });
            },
            trigger: 'blur'
        }
    ],
    "UserName": [
        { "type": "string", "min": 0, "max": 50, "message": "用户名必须小于50字" },
        {
            validator: function (rule, value, cb, source, options) {
                let userId = userModel.ID;
                return new Promise((resolve, reject) => {
                    axios.post("/api/users/UserNameValidate", { userId, userName: value })
                        .then(msg => {
                            if (msg.success) {
                                resolve();
                            } else {
                                reject(msg.msg);
                            }
                        });
                });
            },
            trigger: 'blur'
        }
    ],
    "BankName": [
        { "type": "string", "min": 0, "max": 40, "message": "银行名称限40字" }
    ],
    "CardNumber": [
        { "type": "string", "min": 0, "max": 19, "message": "银行卡号限19位，请不要输入空格" }
    ],
});
const errors = ref([]);
async function onBasicSubmit({ formRef, loading }) {
    try {
        form.value = formRef.value;
        await formRef.value.validate();
        let msg = await axios.post("/api/users/SaveUser", userModel);
        if (msg.success) {
            title.value = "编辑用户"
            isNew.value = false;
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