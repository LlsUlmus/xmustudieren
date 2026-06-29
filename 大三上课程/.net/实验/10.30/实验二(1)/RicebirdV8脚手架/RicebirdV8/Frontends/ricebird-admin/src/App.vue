<template>
    <a-config-provider :locale="zhCN">
        <Suspense>
            <div id="app-container">
                <router-view />
            </div>
        </Suspense>
    </a-config-provider>
</template>

<script setup>
// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import { ConfigProvider } from "ant-design-vue";
import zhCN from "ant-design-vue/es/locale/zh_CN";
import dayjs from 'dayjs';
import 'dayjs/locale/zh-cn';
ConfigProvider.config({
    locale: zhCN
});
dayjs.locale("cn");
</script>

<style lang="less">
#app-container {
    height: 100%;
}
</style>
