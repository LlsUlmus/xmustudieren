<template>
    <a-popover overlayClassName="popover-user-menu">
        <template #content>
            <div class="nav-header-user-menu">
                <div class="user-info">
                    <div class="user-name">
                        {{ app.currentUser.RealName }}({{ app.currentUser.Code }})
                    </div>
                    <div class="user-role">
                        {{ app.currentUser.currentRole.Name || "无" }}
                    </div>
                </div>
                <div class="switch-role" v-if="app.currentUser.Roles.length > 1">
                    <a href="javascript:;" @click="switchRole">[切换身份]</a>
                </div>
            </div>
            <a-menu :selectable="false">
                <a-menu-divider />
                <a-menu-item @click="toProfile">
                    <template #icon>
                        <a-icon icon="user-outlined" />
                    </template>
                    个人中心
                </a-menu-item>
                <a-menu-divider />
                <a-menu-item @click="logoutClick">
                    <template #icon>
                        <logout-outlined />
                    </template>
                    注销
                </a-menu-item>
            </a-menu>
        </template>
        <a-avatar class="avatar-icon">
            <template #icon>
                <a-icon icon="user-outlined" />
            </template>
        </a-avatar>
    </a-popover>
    <switch-role-dialog ref="modal" />
</template>

<script setup>
import app from '@/app'
import SwitchRoleDialog from '@/cores/security/role-switcher.vue'
import { ref } from 'vue'
import { message } from 'ant-design-vue';
import { login } from '@/all-page';
import { logout } from '@/cores/security/useCurrentUser'

const modal = ref();
function switchRole () {
    modal.value.showModal();
}

async function logoutClick() {
    await logout();
    message.success("注销成功");
    await app.router.push(login);
}

async function toProfile () {
    await app.router.push("/manage/user-profile");
}
</script>

<style lang="less">
.popover-user-menu {
    .ant-menu {
        border-inline-end: 0px !important;
    }
    
    .ant-popover-arrow {
        left: 75%;
    }

    .nav-header-user-menu {
        width: 260px;
        display: flex;

        .user-info {
            padding: 16px 12px;
            flex: 1;

            .user-name {
                font-size: 16px;
                font-weight: 600;
                color: @text-color;
            }

            .user-role {
                margin-top: 12px;
                font-size: 12px;
                color: @text-color-secondary;
            }
        }
        .switch-role {
            width: 6em;
            line-height:87.98px;
            text-align: center;
        }
    }

    .ant-menu-item:hover {
        background: @nav-header-menu-item-hover;
        color: @text-color !important;
    }
}
</style>