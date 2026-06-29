<template>
    <!-- <template #extra>
            <a-button key="console" type="primary" @click="switchRole" v-if="app.currentUser.Roles.length > 1">切换身份</a-button>
        </template>
        <switch-role-dialog ref="modal" /> -->
    <a-result :title="props.title">
        <slot></slot>
    </a-result>
</template>

<script setup>
import app from '@/app'
import SwitchRoleDialog from '@/cores/security/role-switcher.vue'
import { watch, ref } from 'vue'

const props = defineProps({
    title: {
        type: String,
        default: '当前身份无法访问此页面，请尝试切换身份或页面'
    }
})
console.log(props.title)
const modal = ref();
function switchRole () {
    modal.value.showModal();
}
</script>