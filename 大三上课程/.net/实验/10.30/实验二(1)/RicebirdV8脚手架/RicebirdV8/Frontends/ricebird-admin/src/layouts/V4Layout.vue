<template>
    <a-layout id="basic-app-layout">
        <a-layout>
            <v4-sider :menu-info="menuInfo" v-if="!needFillInfo" />
            <div class="tools-bar" v-if="!needFillInfo">
                <scheduleIcon />
            </div>
            <main class="ant-layout-content basic-app-container" id="ricebird-app-container" ref="container">
                <router-view v-slot="{ Component }" v-if="viewPermission" :key="currentPath.path">
                    <component :is="Component" :key="`${currentPath.path}-${app.currentUser.ver}`" />
                </router-view>
                <no-permission v-else />
            </main>
        </a-layout>
    </a-layout>
</template>

<script setup>
import app from '@/app'
import router from '@/routers'
import V4Sider from './components/v4-sider.vue'
import { ref, watch, provide } from 'vue';
import scheduleIcon from '@/cores/schedules/schedule-icon.vue'
import signalR from '@/signalR'
// --- useMenu --- //
let menuInfo = router.useMenus();
let { menus, openKeys, selectedKeys, onMenuClick, currentPath, menuVersion } = menuInfo;
const viewPermission = ref(false);
const needFillInfo = ref(!app.currentUser.Mobile || !app.currentUser.Email)
watch(menuVersion, nv => {
    viewPermission.value = currentPath.allAccess || app.succeed(currentPath.path);
    if (needFillInfo.value && currentPath.path !== "/manage/user-profile") {        
        app.toPage("/manage/user-profile");
    }
}, { immediate: true })

// --- 容器 --- //
const container = ref();
provide("app-container", container);
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

    .tools-bar {
        position: fixed;
        top: 0;
        right: 24px;
        height: 46px;
        z-index: 1000;
        display: flex;
    }

    .basic-app-container {
        height: calc(100vh);
        flex: 1;
        overflow: auto;
    }
}
</style>
