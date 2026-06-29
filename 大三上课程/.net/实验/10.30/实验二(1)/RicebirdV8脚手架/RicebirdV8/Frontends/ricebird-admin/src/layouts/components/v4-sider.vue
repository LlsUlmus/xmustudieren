<template>
    <a-layout-sider :width="260" class="navigation" id="basic-app-sider">
        <div class="logo-area">
            <a href="javascript:void;" class="logo">
                <a-avatar class="cxw-logo" shape="square">
                    <template #icon>
                        <RadarChartOutlined />
                    </template>
                </a-avatar>
                <span class="site-title">待修改名称</span>
            </a>
            <div class="btn-area">
                <a href="javascript: void(0);" class="btn" @click="logoutClick">
                    <logout-outlined /> 注销
                </a>
            </div>
        </div>
        <div class="profile-area">
            <div class="user-icon">
                <img class="xmu-logo" src="/src/assets/images/profile.png" />
            </div>
            <div class="user-body">
                <div class="user-info">
                    <div class="user-name">
                        {{ app.currentUser.RealName }}
                    </div>
                    <div class="user-role">
                        {{ app.currentUser.currentRole.Name || "无" }}
                    </div>
                </div>
            </div>
        </div>
        <div class="menu-area">
            <a-menu @click="onMenuClick" :items="menus" mode="inline" :openKeys="openKeys" 
                :selectedKeys="selectedKeys" />
        </div>
    </a-layout-sider>
</template>

<script setup>
import app from '@/app'
import { login } from '@/all-page';
import { logout } from '@/cores/security/useCurrentUser'
import { message } from 'ant-design-vue';
import { watch } from 'vue'

const props = defineProps({
    menuInfo: Object
})

// --- useMenu --- //
let { menus, openKeys, selectedKeys, onMenuClick, currentPath, menuVersion } = props.menuInfo;
async function logoutClick() {
    await logout();
    message.success("注销成功");
    await app.router.push(login);
}
</script>

<style lang="less">
#basic-app-sider {
    background: @navigation-background;
    border-right: 1px solid @border-shadow;
    box-sizing: border-box;
    position: relative;
    height: 100vh;

    .logo-area { // 49px
        padding: 8px 10px 8px 16px;
        display: flex;
        border-bottom: 1px solid #eee;
        .logo {
            display: flex;
            .cxw-logo {
                width: 32px;
                height: 32px;
                display: block;
                background-color: @primary-color;
            }
            .site-title {
                line-height: 32px;
                font-size: 16px;
                color: @text-color;
                margin-left: 5px;
            }
        }
        .btn-area {
            margin-left: auto;
            line-height: 32px;
            font-size: 16px;
        }
    }

    .profile-area { // 83px
        display: flex;
        padding: 16px 0px 16px 16px;
        border-bottom: 1px solid #eee;
        box-shadow: 0 2px 3px rgba(0,0,0,.03);
        background: #fff;
        .user-icon {
            display: inline-block;
            .xmu-logo {
                width: 50px;
                border-radius: 50%;
                vertical-align: middle;
                border: 0;
            }
        }
        .user-body {
            padding: 0px 12px;
            flex: 1;

            .user-name {
                font-size: 16px;
                font-weight: 600;
                color: @text-color;
            }

            .user-role {
                margin-top: 12px;
                font-size: 12px;
                color: @text-color-secondary;
            }
        }
    }

    .menu-area {
        height: calc(100vh - 132px);
        overflow-y: auto;

        .ant-menu {
            background: transparent;
            padding-right: 5px;
            border: none;
        }

        ul.ant-menu-light.ant-menu-inline .ant-menu-sub.ant-menu-inline {
            background: transparent;
        }
    }
}
</style>