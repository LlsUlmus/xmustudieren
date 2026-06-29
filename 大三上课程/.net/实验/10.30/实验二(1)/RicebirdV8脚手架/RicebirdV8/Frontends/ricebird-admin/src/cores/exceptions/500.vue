<template>
    <a-result status="500" title="500" sub-title="对不起，服务器出了一点小差子.">
        <template #extra>
            <a-button type="primary" @click="toHome">
                返回登录页
            </a-button>
        </template>
    </a-result>
</template>

<script setup>
import app from '@/app';
app.setTitle("服务器出现问题");
function toHome() {
    app.router.push({ path: '/authen/login' })
}
</script>