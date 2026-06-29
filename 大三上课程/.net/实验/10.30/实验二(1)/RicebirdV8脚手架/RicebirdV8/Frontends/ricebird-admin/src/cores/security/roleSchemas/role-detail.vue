<template>
    <no-permission class="form-list-container" title="你无权修改角色，请切换正确的身份后再试" v-if="!app.succeed('获取角色')" />
    <a-result class="form-list-container" title="该角色不是登录身份的一种，无需配置权限" v-else-if="!model.UseAsPrincipal" />
    <div id="role-detail" class="form-list-container" v-else>
        <h1 class="header">
            为「{{ model.Name }}」配置，未选择视为
            <span v-if="model.NotSetEquals === 0" class="color-success">
                <a-icon icon="CheckCircleFilled" /> 允许
            </span>
            <span v-else class="color-error">
                <a-icon icon="CloseCircleFilled" /> 拒绝
            </span>
            。
        </h1>
        <a-tabs v-model:activeKey="activeKey" class="tab-container">
            <a-tab-pane key="permissions" tab="权限配置">
                <RolePermissions :data="apis.data" :noSetAs="model.NotSetEquals" v-model:value="model.Permissions" :enable="app.succeed('保存角色')" />
            </a-tab-pane>
            <a-tab-pane key="menus" tab="菜单配置">
                <RolePermissions :data="menus.data" :noSetAs="model.NotSetEquals" v-model:value="model.Menus" :enable="app.succeed('保存角色')" />
            </a-tab-pane>
        </a-tabs>
        <a-space class="btn-area">
            <a-button type="primary" @click="submit" v-if="app.succeed('保存角色')">提交</a-button>
            <a-button @click="copy" class="mr" v-if="app.succeed('复制角色')">复制角色</a-button>
        </a-space>
    </div>    
</template>

<script setup>
import { ref, watch, inject, reactive, provide } from 'vue'
import app from '@/app'
import axios from '@/axios';
import { message } from 'ant-design-vue';
import RolePermissions from './role-detail-permissions.vue'
import { loadTree } from './useRole'
import { apis, menus } from './usePermissions'

let key = inject("selectedKey");
let keyword = ref("");

watch(key, async () => await load(), {immediate: true})

const emptyModel = {
    "Name": "",
    "For": 1,
    "DisplayAs": "",
    "NotSetEquals": 1,
    "Menus": [],
    "Permissions": [],
    "DisplayOrder": 0,
    "UseAsPrincipal": true,
    "ID": app.GUID_EMPTY
};
const model = reactive(emptyModel);
const activeKey = ref("permissions");
provide("model", model);

async function copy () {
    let confirm = await app.modals.confirm({
        title: "操作确认",
        content: "是否要复制此角色？"
    });
    if (!confirm) return; 
    let msg = await axios.post("/api/roles/CopyRoleSchema", { id: key.value });
    await loadTree(true);
    message.success(msg.msg);
}

async function load () {
    let msg = await axios.post("/api/roles/GetRoleSchema", { id: key.value });
    Object.assign(model, msg.data);
}

async function submit () {
    const param = { id: model.ID, permits: model.Permissions, menus: model.Menus };
    let msg = await axios.post("/api/roles/SavePermissions", param);
    if (msg.success) {
        await load();
        message.success("保存成功");        
    } else {
        message.error("保存失败")
    }
}

// async function submitMenus () {
//     const param = { id: model.ID, descriptors: model.Menus };
//     let msg = await axios.post("/api/roles/SaveMenus", param);
//     if (msg.success) {
//         message.success("保存成功");
//         Object.assign(model, msg.data);
//     } else {
//         message.error("保存失败")
//     }
// }
</script>