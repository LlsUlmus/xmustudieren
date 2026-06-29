<template>
    <a-modal :attrs="$attrs" v-model:open="open" :footer="null" destroyOnClose :maskClosable="false">
        <h3>{{ user.RealName }}的可用身份：</h3>
        <div>
            <div class="role" v-for="(v, k) in user.Roles" :key="k" @click="processor(k)">
                <div class="title">{{ v.DisplayName }}</div>
                <div class="icon">
                    <CheckOutlined v-if="k === user.currentRoleIndex" class="color-success" />
                </div>
            </div>
        </div>
    </a-modal>
</template>

<script setup>
import app from '@/app';
import useModal from '@/components/modals/useModal'
import RoleSwitcher from '@/cores/security/role-switcher.vue'

const {
    open, loading, 
    onOk, onCancel,
    close, showModal
} = useModal(onOpen, processor);

defineExpose({
    showModal
});

// -- 业务逻辑，点击确定后应该如何 -- //
const user = app.currentUser;
function onOpen (...para) {
    // 窗口打开时，处理这里的逻辑，onOpen的参数就是showModal输入的参数
    // STEP3：把输入的参数合并到模型里
}

async function processor (k) {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {
        user.switchRole(k);
    } catch (err) {

    } finally {
        // location.reload();
        close();
    }
}
</script>

<style lang="less" scoped>
@border: rgba(0, 0, 0, 0.07) 1px solid;
.role {
    padding: 16px 24px;
    display: flex;
    // border-top: @border;
    border-bottom: @border;
    font-size: 16px;
    transition: .5s all;
    cursor: pointer;
    &:hover {
        box-shadow: 0 1px 2px -2px rgba(0, 0, 0, 0.16), 0 3px 6px 0 rgba(0, 0, 0, 0.12), 0 5px 12px 4px rgba(0, 0, 0, 0.09);
    }
    &:first-of-type {
        border-top: none;
    }
    .icon {
        margin-left: auto;
    }
}
</style>