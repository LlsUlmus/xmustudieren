<template>
    <sider-modal v-model:open="open" :sider-title="title" v-model:active-key="activeKey" v-if="app.succeed('保存用户')" @close="onClose">
        <basicInfo title="基本信息" icon="credit-card-outlined" />
        <studentInfo title="学生信息" icon="SmileOutlined" :disabled="isNew"/>
        <teacherInfo title="教师信息" icon="BookOutlined" :disabled="isNew"/>
        <editUserOrg title="角色和组织机构" icon="ApartmentOutlined" :disabled="isNew" />
        <authorizeInfo title="登录信息" icon="KeyOutlined" :disabled="isNew" />
    </sider-modal>
</template>

<script setup>
import { ref, reactive, provide } from 'vue'
import app from '@/app';
import editUserOrg from './edit-user-org.vue';
import basicInfo from './basic-info.vue';
import studentInfo from './student-info.vue';
import teacherInfo from './teacher-info.vue';
import authorizeInfo from './authorize-info.vue';
import _ from 'lodash'

const activeKey = ref("基本信息");
const open = ref(false);
const title = ref("新建用户");
const isNew = ref(true);
const id = ref(app.GUID_EMPTY);
const userModel = reactive({});

let resolve = () => {};
async function showModal(userId, defaultTab) {
    id.value = userId;
    open.value = true;
    activeKey.value = defaultTab || "基本信息";
    let resolver = app.withResolvers();
    resolve = resolver.resolve;
    return resolver.promise;
}

function onClose() {
    resolve();
}

provide("isNew", isNew);
provide("title", title);
provide("id", id);
provide("userModel", userModel);

defineExpose({ showModal });
</script>

<style lang="less" scoped>
.common-infor {
    margin-bottom: 24px;
}
</style>