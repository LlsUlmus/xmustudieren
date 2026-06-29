<template>
    <sider-modal v-model:open="open" :sider-title="title" v-model:active-key="activeKey" @close="onClose">
        <basicInfo title="基本信息" icon="credit-card-outlined" />
        <!-- <editUserOrg title="角色和组织机构" icon="ApartmentOutlined" :disabled="isNew" />
        <authorizeInfo title="登录信息" icon="KeyOutlined" :disabled="isNew" /> -->
    </sider-modal>
</template>

<script setup>
import { ref, reactive, provide } from 'vue'
import app from '@/app';
import basicInfo from './basic-info.vue';
import _ from 'lodash'

const activeKey = ref("基本信息");
const open = ref(false);
const title = ref("注册用户");
const isNew = ref(false);
const id = ref(app.GUID_EMPTY);
const userModel = reactive({});

let resolve = () => {};
async function showModal() {
    open.value = true;
    let resolver = app.withResolvers();
    resolve = resolver.resolve;
    return resolver.promise;
}

function onClose () {
    open.value = false;
    resolve();
}

provide("isNew", isNew);
provide("title", title);
provide("id", id);
provide("userModel", userModel);
provide("onClose", onClose);

defineExpose({ showModal });
</script>
