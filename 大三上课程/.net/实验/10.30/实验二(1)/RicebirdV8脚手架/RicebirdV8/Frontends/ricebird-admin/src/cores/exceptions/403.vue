<template>
    <a-result status="403" title="403" sub-title="你无权访问本页.">
        <template #extra>
            <a-button type="primary" @click="toHome">
                返回登录页
            </a-button>
        </template>
    </a-result>
</template>

<script setup>
import app from '@/app';
app.setTitle("无权访问此页");
function toHome() {
    app.router.push({ path: '/authen/login' })
}
</script>
