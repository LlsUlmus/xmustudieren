<template>
    <a-modal title="新建/修改菜单项" :attrs="$attrs" v-model:open="open" @ok="onOk" @cancel="onCancel" okText="确定" cancelText="取消"
        :confirm-loading="loading" destroyOnClose :maskClosable="false">
        <a-form :rules="rules" :model="entity" :label-col="{ span: 5 }" class="form" ref="formRef">
            <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`修改菜单时发生错误：`">
                <template #description>
                    <ul>
                        <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                    </ul>
                </template>
            </a-alert>
            <a-form-item label="菜单名称" name="Name">
                <a-input v-model:value="entity.Name" placeholder="请填写菜单名称" />
            </a-form-item>
            <a-form-item label="排序号" name="DisplayOrder">
                <a-input-number v-model:value="entity.DisplayOrder" placeholder="请填写菜单排序号" style="width: 100%"/>
                <div class="text-secondary">
                    升序排列的整数排序号
                </div>
            </a-form-item>
            <a-form-item label="菜单图标" name="Icon">
                <icon-selector v-model:value="entity.Icon" />
            </a-form-item>
            <a-form-item label="上级菜单" name="ParentId">
                <a-select v-model:value="entity.ParentId" @change="parentIdChanged">
                    <a-select-option :value="v.value" v-for="(v, k) in parents" :key="k" :disabled="v.value !== app.GUID_EMPTY && v.value === entity.ID">
                        <a-icon :icon="v.icon"></a-icon> {{ v.label }}
                    </a-select-option>
                </a-select>
            </a-form-item>
            <a-form-item label="菜单类型" name="LinkType">
                <a-radio-group v-model:value="entity.LinkType" @change="linkTypeChanged">
                    <a-radio-button :value="1">普通的URL</a-radio-button>
                    <a-radio-button :value="2">内置页面</a-radio-button>
                    <a-radio-button :value="3" v-if="entity.ParentId === app.GUID_EMPTY">母菜单项</a-radio-button>
                </a-radio-group>
            </a-form-item>
            <a-form-item :label="linkToLabelName" name="LinkTo" v-if="entity.LinkType !== 3">
                <a-input v-model:value="entity.LinkTo" placeholder="请填写链接地址" v-if="entity.LinkType === 1"/>
                <a-select :options="vuePages" v-model:value="entity.LinkTo" v-if="entity.LinkType === 2" />
            </a-form-item>
        </a-form>
    </a-modal>
</template>

<script setup>
import app from '@/app';
import { axios } from '@/axios';
import { ref, reactive, watchEffect, inject, computed } from 'vue'
import { getMenuOptions } from '@/routers/buildMenu';

const props = defineProps({
    open: Boolean,
    id: {
        type: String,
        default: app.GUID_EMPTY
    }
});
const emits = defineEmits(["update:open"]);
const open = ref(false);
const id = ref("");
const loading = ref(false);
const parents = inject("parents");
const loadTable = inject("load");
const vuePages = getMenuOptions();
const formRef = ref();

watchEffect(() => {
    open.value = props.open;
    let oldId = id.value;
    id.value = props.id;
    if (open.value || oldId !== id.value) {
        loadMenu(id.value);
    }
});

function onOk() {
    loading.value = true;
    errors.value = [];
    formRef.value
        .validate()
        .then(async () => {
            let msg = await axios.post("/api/menu/SaveMenu", entity);
            loading.value = false;
            if (!msg.success) {
                errors.value = msg.errorStrings;
                return;
            } else {
                close();
                Object.assign(entity, emptyEntity);
                loadTable(true);
            }
        })
        .catch(err => {
            loading.value = false;
        });
}

function onCancel() {
    close();
}

function close() {
    loading.value = false;
    emits("update:open", false);
}

const errors = ref([]);
const emptyEntity = {
    "LinkType": 1,
    "Visibility": 1,
    "Icon": "",
    "LinkTo": "",
    "QueryString": "",
    "Parameters": "",
    "Name": "",
    "DisplayOrder": 0,
    "ParentId": "00000000-0000-0000-0000-000000000000",
    "InternalTreeCode": "",
    "ID": app.GUID_EMPTY
};
const entity = reactive({ ...emptyEntity });
const linkToLabelName = computed(() => {
    const enums = {
        1: "链接地址",
        2: "选择页面",
    }
    return enums[entity.LinkType];
});
const rules = reactive({
    Name: { required: true, message: "必须填写此项" },
    LinkTo: { required: true, message: "必须填写或者选择一个菜单" }
});
async function loadMenu(id) {
    let msg = await axios.post("/api/menu/GetMenu", { id });
    Object.assign(entity, msg.data);
    // Object.assign(rules, msg.rules);
}

function parentIdChanged() {
    if (entity.ParentId !== app.GUID_EMPTY && entity.LinkType === 3) {
        entity.LinkType = 1;
    }
}
function linkTypeChanged() {
    if (entity.LinkType === 3 && entity.LinkTo === "") {
        entity.LinkTo = '-';
    }

    if (entity.LinkType !== 3 && entity.LinkTo === "-") {
        entity.LinkTo = "";
    }
}
// const expose = defineExpose({});
</script>

<style scoped lang="less">
.form {
    margin-top: @margin;
    .error {
        margin-bottom: @margin;
    }
}
</style>