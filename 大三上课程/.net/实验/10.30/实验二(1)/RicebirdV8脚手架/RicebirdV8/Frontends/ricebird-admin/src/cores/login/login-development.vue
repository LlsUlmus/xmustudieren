<template>
    <div class="main" id="login-form">
        <a-form id="formLogin" class="user-layout-login" :model="model" ref="formRef">
            <a-tabs v-model:activeKey="activeKey" centered>
                <a-tab-pane key="tab1" tab="账号密码登录">
                    <a-alert v-if="error.length" type="error" showIcon style="margin-bottom: 24px;"
                        :message="error" />
                    <a-form-item name="token" :rules="[{ required: true, message: '请输入工号' }]">
                        <a-input size="large" type="text" placeholder="请输入工号" v-model:value="model.token">
                            <template #prefix>
                                <a-icon icon="UserOutlined" :style="{ color: 'rgba(0,0,0,.25)' }" />
                            </template>
                        </a-input>
                    </a-form-item>
                    <a-form-item name="password" :rules="[{ required: true, message: '必须输入密码' }]">
                        <a-input-password size="large" placeholder="请输入密码" v-model:value="model.password"
                            @pressEnter="login">
                            <template #prefix>
                                <a-icon icon="lock-outlined" :style="{ color: 'rgba(0,0,0,.25)' }" />
                            </template>
                        </a-input-password>
                    </a-form-item>
                </a-tab-pane>
                <a-tab-pane key="tab2" tab="快捷登录">
                    <a-alert v-if="error.length" type="error" showIcon style="margin-bottom: 24px;"
                        :message="error" />
                    <FastLoginDev @on-select="onSelect" />
                </a-tab-pane>
            </a-tabs>
            <a-row :gutter="16">
                <a-col span="15">
                    <a-form-item style="margin-top:24px">
                        <a-button size="large" type="primary" class="login-button" @click="toECard()">厦大统一认证登录</a-button>
                    </a-form-item>
                </a-col>
                <a-col span="9">
                    <a-form-item style="margin-top:24px">
                        <a-button size="large" class="login-button success-button" @click="login" :loading="loading">登录</a-button>
                    </a-form-item>
                </a-col>
            </a-row>
        </a-form>
    </div>
</template>

<script setup>
import { dashboard } from '@/all-page'
import axios from "@/axios"
import app from '@/app'
import { isLogined, setCurrentUser } from '@/cores/security/useCurrentUser'
import { onMounted, reactive, ref } from 'vue'
import FastLoginDev from './fast-login-dev.vue'

if (isLogined) {
    app.router.push(dashboard);
}

const model = reactive({
    token: "",
    password: ""
});
const loading = ref(false);
const activeKey = ref("tab1");
const error = ref("");

async function login () {
    loading.value = true;
    let method = "POST";
    let url = "/api/authorize/login";
    let timestamp = (new Date()).valueOf();
    let nounce = Math.random();
    let token = model.token;
    let pwd = app.sha1(model.password);

    let signature = [method, url, timestamp, nounce, token, pwd].join('\n');
    signature = app.sha256(signature);

    let msg = await axios.post("/api/authorize/login", {
        token,
        nounce,
        timestamp,
        signature
    });

    if (msg.success) {
        await setCurrentUser(msg);
        app.router.push(dashboard);
    } else {
        error.value = msg.msg;
    }
    loading.value = false;
}

function toECard () {
    
}

async function onSelect(e, reset) {
    let un = e.value;
    error.value = "";
    let msg = await axios.post("/debug/security/GetToken", {
        token: un,
    });

    if (msg.success) {
        await setCurrentUser(msg);
        app.router.push(dashboard);
    } else {
        error.value = msg.msg;
    }

    reset();
    // if (!msg.success) {
    //     setLoginError(msg.msg);
    // } else {
    //     setToken(msg.token);
    //     router.landing();
    // }
}
</script>

<style lang="less">
#login-form {
    .success-button {
        background-color: @success-color;
        border-color: @success-color;
        color: #fff;
    }

    .ant-spin-blur {
        opacity: 0;
    }

    .user-layout-login {
        min-height: 318px;

        label {
            font-size: 14px;
        }

        .getCaptcha {
            display: block;
            width: 100%;
            height: 40px;
        }

        .forge-password {
            font-size: 14px;
        }

        button.login-button {
            padding: 0 15px;
            font-size: 16px;
            height: 40px;
            width: 100%;
        }

        .user-login-other {
            text-align: left;
            margin-top: 24px;
            line-height: 22px;

            .item-icon {
                font-size: 24px;
                color: rgba(0, 0, 0, 0.2);
                margin-left: 16px;
                vertical-align: middle;
                cursor: pointer;
                transition: color 0.3s;

                &:hover {
                    color: #1890ff;
                }
            }

            .register {
                float: right;
            }
        }
    }
}</style>