<template>
    <a-layout id="basic-app-layout">
        <a-layout-header class="basic-app-header">
            <a href="javascript:void(0);" class="logo">
                <!-- <span class="xmu-logo"></span> -->
                <span class="brand">{{ app.web.name }}</span>
            </a>
            <div class="flex-1"></div>
            <scheduleIcon />
            <div class="commander-icon">
                <user-info />
            </div>
        </a-layout-header>
        <a-layout>
            <a-layout-sider :width="260" class="navigation basic-app-sider">
                <a-menu @click="onMenuClick" :items="menus" mode="inline" :openKeys="openKeys"
                    :selectedKeys="selectedKeys" />
            </a-layout-sider>
            <main class="ant-layout-content basic-app-container" id="ricebird-app-container" ref="container">
                <router-view v-slot="{ Component }" v-if="viewPermission">
                    <component :is="Component" :key="`${currentPath.value}`" />
                </router-view>
                <no-permission v-else />
            </main>
        </a-layout>
    </a-layout>
</template>

<script setup>
import app from '@/app'
import router from '@/routers'
import UserInfo from './components/user-info.vue'
import scheduleIcon from '@/cores/schedules/schedule-icon.vue'
import { watch, ref, provide } from 'vue';

// --- useMenu --- //
let { menus, openKeys, selectedKeys, onMenuClick, currentPath, menuVersion } = router.useMenus();
const viewPermission = ref(false);
watch(menuVersion, nv => {
    viewPermission.value = currentPath.allAccess || app.succeed(currentPath.path);
}, { immediate: true })

// --- 容器 --- //
const container = ref();
provide("app-container", container)
</script>

<style lang="less">
#app {
    background: #fff;
}

#basic-app-layout {
    width: 100vw;
    height: 100vh;
    overflow: hidden;

    .ant-layout {
        background: #fff;
    }

    .basic-app-sider {
        background: @navigation-background;
        border-right: 1px solid @border-shadow;
        box-sizing: border-box;
        position: relative;
        height: @content-height;
        overflow-y: auto;


        .ant-menu {
            background: transparent;
            padding-right: 5px;
            border: none;
        }
    }

    .basic-app-header {
        height: @nav-header-height;
        overflow: hidden;
        z-index: 950;
        background: @nav-header-background;
        box-shadow: 0 0.46875rem 2.1875rem rgba(4, 9, 20, .03), 0 0.9375rem 1.40625rem rgba(4, 9, 20, .03), 0 0.25rem 0.53125rem rgba(4, 9, 20, .05), 0 0.125rem 0.1875rem rgba(4, 9, 20, .03);
        display: flex;
        padding: 0px 50px 0px 0px;
        gap: 20px;
        scroll-behavior: smooth;

        .logo {
            display: block;
            width: 260px;
            padding-left: 15px;
            align-items: center;
            align-self: stretch;
            line-height: 48px;

            span.xmu-logo {
                width: 26px;
                height: 26px;
                display: inline-block;
                // background: url('@/assets/xmu-logo.svg') 50% 50% / contain;
            }

            span.brand {
                font-size: 19px;
                font-weight: bold;
                color: @nav-header-dark-color;
            }

            &:hover {
                background: @nav-header-hover-background;
            }
        }

        .commander-icon {
            width: 48px;
            text-align: center;
            line-height: 48px;

            &:hover {
                background: @nav-header-hover-background;
            }

            .avatar-icon {
                background: @nav-header-dark-color;
                color: #fff;
            }
        }
    }

    .basic-app-container {
        height: calc(100vh - 48px);
        overflow: auto;
    }
}
</style>
