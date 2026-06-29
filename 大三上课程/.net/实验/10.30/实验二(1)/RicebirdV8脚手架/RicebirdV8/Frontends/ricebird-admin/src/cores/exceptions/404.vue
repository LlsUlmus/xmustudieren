<template>
    <a-result status="404" title="404" sub-title="对不起，该页面离家出走了.">
        <template #extra>
            <a-button type="primary" @click="toHome">
                返回登录页
            </a-button>
        </template>
    </a-result>
</template>

<script setup>
import app from '@/app';
app.setTitle("页面找不到了");
function toHome() {
    app.router.push({ path: '/authen/login' })
}
</script>