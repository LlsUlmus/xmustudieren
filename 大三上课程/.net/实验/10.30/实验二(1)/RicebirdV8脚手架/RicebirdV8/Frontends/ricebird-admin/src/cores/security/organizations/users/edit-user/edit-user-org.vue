<template>
    <sub-view class="edit-user-org " v-bind="$attrs">
        <div class="sub-view-title">角色与组织机构</div>
        <a-alert message="添加或者修改完成后，必须要点击左侧的提交按钮。否则无法保存！" type="info" show-icon></a-alert>
        <a-space class="searcher-area">
            <a-button type="primary" @click="addOrg" ><a-icon icon="plus-outlined" /> 添加组织机构关系</a-button>
        </a-space>
        <div class="org-detail-area">
            <div v-for="(v, k) in data">
                <a-space class="org-detal-item">
                    <div title="提交此项">
                        <a-icon v-if="v.modify" :icon="v.icon" class="submit-btn" :class="{ 'loading': v.loading, 'modify': v.modify }" @click="v.submit()"/>
                        <div v-else class="icon-placeholder"></div>
                    </div>
                    <role-select :value="v.roleId" style="width: 12em" @change="v.roleChanged($event)" :remove="v.id !== app.GUID_EMPTY ? app.GUID_EMPTY : ''"
                            prefix="" empty="请选择角色" disableEmpty :disabled="v.loading" :status="v.roleStatus" />
                    <depart-select :disabled="v.loading || v.role.setFor === 0" :value="v.departId" prefix="部门" empty="任意部门" 
                            style="width: 440px" @change="v.departChanged($event)" disableEmpty :status="v.departStatus" />
                    <a-tooltip title="删除此项">
                        <CloseCircleOutlined class="btn remove-btn" @click="removeOrg(v, k)" />
                    </a-tooltip>
                    <template v-if="!v.modify && v.departId !== app.GUID_EMPTY">
                        <a-tooltip title="在系统各处使用部门时，都会将此部门视为用户的部门，其余部门仅在身份验证或者其它有明确指明的位置生效。" v-if="v.departId === rootDepartId">
                            <HomeOutlined class="root-depart"/>
                        </a-tooltip>
                        <span v-else class="set-root-depart-btn" @click="setRootDepart(v.departId)">
                            [设为主要部门]
                        </span>
                    </template>
                </a-space>
                <!-- {{ v.ID }} - {{ v.UserId }} -  -->
            </div>
        </div>
    </sub-view>
</template>

<script setup>
import app from '@/app';
import axios from '@/axios';
import { ref, reactive, inject, watch, computed } from 'vue'
import { getRole } from '@/cores/security/roleSchemas/useRole'
import { getDepart } from '../../departs/useDepartment'
import { message } from 'ant-design-vue';

const data = ref([]);
const userId = inject("id");
const userModel = inject("userModel");
const rootDepartId = computed(() => userModel.RootDepartId);

watch(userId, nv => loadOrgs(nv), {immediate: true});

function addOrg () {
    let relationship = toRelationship({
        ID: app.GUID_EMPTY,
        UserId: userId,
        RoleId: app.GUID_EMPTY,
        DepartId: app.GUID_EMPTY
    });
    data.value.push(relationship);
}

async function removeOrg (item, index) {
    if (item.id !== app.GUID_EMPTY) {
        let confirm = await app.modals.confirm({ title: "是否要删除此记录？", content: "此操作不会对用户产生任何影响。" });
        if (!confirm) {
            return;
        }
    } else {
        data.value.splice(index, 1);
        return;
    }

    let msg = await axios.post("/api/users/RemoveUserOrganization", {
        id: item.id,
    });

    if (msg.success) {
        data.value.splice(index, 1);
        message.success(msg.msg);
    } else {
        message.error(msg.msg);
    }
}

async function loadOrgs (id) {
    if (id === app.GUID_EMPTY) {
        data.value = [];
        return;
    }
    let msg = await axios.post("/api/users/GetUserOrganizations", { id });

    if (msg.success) {
        let arr = [];
        for (let item of msg.data) {            
            let obj = toRelationship(item);
            arr.push(obj);
        }
        data.value = arr;
    }
}

async function setRootDepart (departId) {
    console.log(departId, userId.value);
    let msg = await axios.post("/api/users/SetRootDepart", {
        userId: userId.value,
        departId: departId
    });
    if (msg.success) {
        userModel.RootDepartId = msg.rootDepartId;
        message.success(msg.msg);
    } else {
        message.error(msg.msg);
    }
}

function toRelationship (item) {
    let role = getRole(item.RoleId);
    let obj = {
        id: item.ID,
        userId: item.UserId,
        roleId: item.RoleId,
        roleStatus: "",
        departId: item.DepartId,
        departStatus: "",
        src: item.DepartId,
        role,
        icon: item.ID === app.GUID_EMPTY ? "PlusCircleOutlined" : "CheckCircleOutlined",
        modify: item.ID === app.GUID_EMPTY,
        loading: false,
        roleChanged: function (roleId) {
            if (this.roleId === roleId) return;

            this.roleId = roleId;
            let r = getRole(roleId);
            this.role = r;
            if (r.setFor === 0) {
                this.departId = app.GUID_EMPTY;
            } else {
                this.departId = this.src;
            }
            this.roleStatus = "";
            this.modify = true;
        },
        departChanged: function (departId) {     
            if (this.departId === departId) return;
            this.departId = departId;
            this.src = departId;
            this.departStatus = "";
            this.modify = true; 
        },
        setLoading: function (state) {
            if (state) {
                this.icon = "loading-outlined";
                this.loading = true;
            } else {
                this.icon = this.id === app.GUID_EMPTY ? "PlusCircleOutlined" : "CheckCircleOutlined";
                this.loading = false;
            }
        },
        submit: async function () {
            if (this.roleId === app.GUID_EMPTY) {
                app.modals.warning("填写错误", "必须选择角色才可以提交");
                this.roleStatus = "error";
                return;
            }
            if (this.role.setFor !== 0 && this.departId === app.GUID_EMPTY) {
                app.modals.warning("填写错误",`必须设置 ${this.role.title} 所在的部门方可生效`);
                this.departStatus = "error";
                return;
            }
            this.setLoading(true);
            let msg = await axios.post("/api/users/SetUserOrganization", {
                id: this.id,
                userId: this.userId,
                roleId: this.roleId,
                departId: this.departId
            });
            if (msg.success) {
                let data = msg.data;
                this.departChanged(data.DepartId);
                this.roleChanged(data.RoleId);
                this.id = data.ID;
                this.modify = false;
                userModel.RootDepartId = msg.rootDepartId;
                message.success(msg.msg);
            } else {
                message.error(msg.msg);
            }
            this.setLoading(false);
        }
    }
    return reactive(obj);
}
</script>

<style lang="less">
.edit-user-org {
    .org-detail-area {
        .org-detal-item {
            margin-top: @margin;
        }
        .icon-placeholder {
            width: 20px;
        }
        .submit-btn {
            cursor: pointer;
            font-size: 20px;
            line-height: 32px;
            height: 32px;
            color: @success-color;
            &.modify {
                .color-info;
            }
            &.loading {
                .color-info;
            }
        }
        .btn {
            cursor: pointer;
            font-size: 20px;
            line-height: 32px;
            height: 32px;
            &.remove-btn:hover {
                color: @error-color;
            }
        }
        .root-depart {
            font-size: 20px;
            line-height: 32px;
            height: 32px;
        }
        .set-root-depart-btn {
            cursor: pointer;
        }
    }
}
</style>