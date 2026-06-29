<template>
    <div class="edit-password-control">
        <a-button type="primary" @click="modify" v-if="id != app.GUID_EMPTY">修改密码</a-button>
        <a-modal ref="modalRef" v-model:open="open" :centered="true" title="修改密码">
            <template #footer>
                <a-button key="back" @click="onCancel">取消</a-button>
                <a-button key="submit" type="primary" :loading="loading" @click="onOK">确认</a-button>
            </template>
            <a-form>
                <a-form-item label="输入密码">
                    <a-input-password v-model:value="password" />
                </a-form-item>
                <a-form-item label="确认密码">
                    <a-input-password v-model:value="confirm" />
                </a-form-item>
            </a-form>
        </a-modal>    
    </div>
</template>

<script setup>
import app from "@/app";
import { inject, ref, watch } from "vue";
import axios from "@/axios";
import { message } from 'ant-design-vue';

const props = defineProps({
    url: {
        type: String,
        required: true
    }
});

const modalRef = ref();
const id = inject("id");
const password = ref("");
const confirm = ref("");
const open = ref(false);
const loading = ref(false);

watch(open, nv => {
    password.value = "";
    confirm.value = "";
})

function modify () {
    open.value = true;
}
function onCancel () {
    open.value = false;
}
async function onOK () {
    if (password.value !== confirm.value) {
        alert("两次输入的密码必须一致");
        return;
    }

    if (password.value.length < 6) {
        alert("密码必须在6位以上");
        return;
    }

    // loading.value = true;
    let pass = app.sha1(password.value);
    let msg = await axios.post(props.url, {
        id: id.value,
        password: pass,
    });
    message.success(msg.msg);
    loading.value = false;
    open.value = false;
}
</script>

<style lang="less" scoped>
</style>