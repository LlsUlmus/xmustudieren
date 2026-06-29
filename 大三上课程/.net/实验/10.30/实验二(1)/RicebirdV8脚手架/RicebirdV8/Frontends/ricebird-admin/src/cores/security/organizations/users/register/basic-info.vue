<template>
    <form-view v-bind="$attrs" @submit="onBasicSubmit" :model="model" :labelCol="{ span: 2 }"
        :rules="rules">
        <div class="sub-view-title">基本信息</div>
        <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存部门时发生错误：`">
            <template #description>
                <ul>
                    <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                </ul>
            </template>
        </a-alert>
        <a-alert v-if="!errors.length" type="info" showIcon style="margin-bottom: 24px;"
            message="此处填写的姓名必须与证件上的姓名保持一致！" />
        <div id="各系统间都通用的内容" class="common-infor">
            <a-form-item name="Code" label="证件号">
                <a-input-group compact style="width: 100%">
                    <a-form-item-rest>
                        <dict-select v-model:value="model.CodeType" dict="证件类型" prefix="" remove="教工号" style="width: 200px;text-align: center;" />
                    </a-form-item-rest>
                    <a-input placeholder="在此输入证件号，此字段必须全系统唯一" style="width: calc(100% - 200px);" v-model:value="model.Code"
                        show-count :maxlength="30" />
                </a-input-group>
            </a-form-item>
            <a-form-item name="RealName" label="姓名">
                <a-input v-model:value="model.RealName" placeholder="请输入姓名" show-count :maxlength="50" />
            </a-form-item>
            <a-row>
                <a-col span="12">
                    <a-form-item name="Mobile" label="手机号" :labelCol="{ span: 4 }">
                        <a-input v-model:value="model.Mobile" placeholder="请输入手机号，此字段必须全系统唯一" show-count :maxlength="11" />
                    </a-form-item>
                </a-col>
                <a-col span="12">
                    <a-form-item name="Email" label="邮箱" :labelCol="{ span: 4 }">
                        <a-input v-model:value="model.Email" placeholder="请输入邮箱，此字段必须全系统唯一" show-count :maxlength="30" />
                    </a-form-item>
                </a-col>
            </a-row>
            <a-form-item name="Password" label="登录密码">
                <a-input-password v-model:value="model.Password" placeholder="在此输入密码" show-count :maxlength="20" />
            </a-form-item>
            <a-form-item name="ConfirmPassword" label="确认密码">
                <a-input-password v-model:value="model.ConfirmPassword" placeholder="再次输入密码确认" show-count :maxlength="20" />
            </a-form-item>
        </div>
        <!-- 每个系统都独有的内容 -->
        <!-- <a-form-item name="BankName" label="开户行">
            <a-input v-model:value="model.BankName" placeholder="当证件类型不是教工号时，在此输入开户行。否则此字段无效" show-count :maxlength="40" />
        </a-form-item>
        <a-form-item name="CardNumber" label="银行卡号">
            <a-input v-model:value="model.CardNumber" placeholder="当证件类型不是教工号时，在此输入银行卡号。否则此字段无效" show-count :maxlength="19" />
        </a-form-item> -->
        <a-form-item name="Gender" label="性别">
            <a-radio-group v-model:value="model.Gender">
                <a-radio-button value="男">男</a-radio-button>
                <a-radio-button value="女">女</a-radio-button>
            </a-radio-group>
        </a-form-item>
        <a-form-item name="departId" label="所在部门">
            <depart-select v-model:value="model.departId" prefix="" empty="请选择部门" schema="学院" />
        </a-form-item>
    </form-view>
</template>

<script setup>
import { ref, reactive, inject, watch, watchEffect } from 'vue'
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
    "UserSource": "自行注册",
    "Gender": "男",
    "CodeType": "居民身份证",
    "BankName": "",
    "CardNumber": "",
    "Password": "",
    "UserPassword": "",
    "ConfirmPassword": "",
    "departId": app.GUID_EMPTY,
    "ID": app.GUID_EMPTY
};
const userModel = inject("userModel");
const model = reactive({ ...empty });

watch(userId, nv => {
    if (nv === model.ID) return;
    if (form.value && form.value.clearValidate) {
        form.value.clearValidate();
    }
    if (nv === app.GUID_EMPTY) {
        clearUser();
    } else {
        loadUser(nv);
    }
}, {immediate: true});

// watch(() => model.Code, async nv => {
//     let msg = await axios.post("/api/applications/GetUserByCode", { code: nv });
//     if (msg.success) {
//         let data = msg.data;
//         model.Code = code;
//         model.CodeType = data.CodeType;
//         model.Gender = data.Gender;
//         model.RealName = data.RealName;
//         model.DepartId = data.DepartId;
//         model.Mobile = data.Phone;
//     }
// });

watchEffect(() => {
    Object.assign(userModel, model);
});

function clearUser() {
    title.value = "新建用户"
    isNew.value = true;
    Object.assign(model, empty);
}
async function loadUser(id) {
    title.value = "编辑用户"
    isNew.value = false;
    let msg = await axios.post("/api/users/GetUser", { id });
    if (!msg.success) {
        message.error(msg.msg);
        return;
    }
    Object.assign(model, msg.data);
}

// -- 登录信息 -- //
const rules = reactive({
    "RealName": [
        { "type": "string", "min": 0, "max": 50, "message": "姓名限50字" },
        { required: true, message: "必须填写姓名" }
    ],
    "Code": [
        { "type": "string", "min": 0, "max": 30, "message": "证件号限30字" },
        { "required": true, "message": "必须填写证件号" },
        {
            validator: function (rule, value, cb, source, options) {
                let userId = model.ID;            
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
        { "type": "string", "min": 0, "max": 11, "message": "手机号限11位", required: true },
        { "type": "string", "pattern": "^1[3-9]\\d{9}$", "message": "这不是一个合法的手机号" },
        {
            validator: function (rule, value, cb, source, options) {
                let userId = model.ID;
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
        { "type": "string", "min": 0, "max": 50, "message": "电子邮箱限50字", required: true },
        { "type": "string", "pattern": "^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$", "message": "这不是一个合法的电子邮箱名称" },
        {
            validator: function (rule, value, cb, source, options) {
                let userId = model.ID;
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
    "Password": [
        { required: true, "min": 6, "max": 20, "message": "密码限6-20位", },
    ],
    "ConfirmPassword": [
        {
            required: true,
            validator: function (rule, value, cb, source, options) {
                return new Promise((resolve, reject) => {
                    if (model.Password === model.ConfirmPassword) {
                        resolve();
                    } else {
                        reject("两次输入的密码必须相同");
                    }
                });
            },
            trigger: 'blur'
        }
    ],
    "CodeType": [
        { "type": "enum", enum: ["教工号", "身份证", "护照", "港澳台通行证"], "message": "证件类型必须是教工号，身份证，护照，港澳台通行证其中之一" }
    ],
    "departId": [
        { 
            required: true,
            validator: function (rule, value, cb, source, options) {
                return new Promise((resolve, reject) => {
                    if (model.departId !== app.GUID_EMPTY) {
                        resolve();
                    } else {
                        reject("必须选择一个部门");
                    }
                });
            },
            trigger: 'change'
        }
    ]
});
const errors = ref([]);
const onClose = inject("onClose");
async function onBasicSubmit({ formRef, loading }) {
    try {
        form.value = formRef.value;
        await formRef.value.validate();
        const submitModel = { ...model };
        submitModel.UserPassword = app.sha1(submitModel.Password);
        delete submitModel.Password;
        delete submitModel.ConfirmPassword;
        let msg = await axios.post("/api/users/Register", submitModel);
        if (msg.success) {
            title.value = "编辑用户"
            isNew.value = false;
            errors.value = [];
            message.success("保存成功");
            onClose();
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
    margin-bottom: 24px;
}
</style>