<template>
    <a-modal :attrs="$attrs" v-model:open="open" @ok="onOk" @cancel="onCancel" okText="确定" cancelText="取消" :confirm-loading="loading" destroyOnClose :maskClosable="false">
        <a-form :rules="rules" :model="model" layout="vertical" ref="formRef" class="form">
            <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`保存字典时发生错误：`">
                <template #description>
                    <ul>
                        <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                    </ul>
                </template>
            </a-alert>
            <a-form-item label="字典名称" name="Name">
                <a-input v-model:value="model.Name" placeholder="字典名称，限20字" />
            </a-form-item>
        </a-form>
    </a-modal>
</template>

<script setup>
import app from '@/app'
import { ref, reactive, toRaw } from 'vue'
import useModal from '@/components/modals/useModal'
import axios from '@/axios'
const {
    open, loading, 
    onOk, onCancel,
    close, showModal
} = useModal(onOpen, processor);

defineExpose({
    showModal
});

// -- 业务逻辑，点击确定后应该如何 -- //
const emptyModel = {
    ID: app.GUID_EMPTY,
    Name: ""
};
const model = reactive({ ...emptyModel });
const rules = {
    Name: { required: true, max: 20 }
}
const formRef = ref("");
const errors = ref([]);

function onOpen (id, name) {
    errors.value = [];
    // 窗口打开时，处理这里的逻辑，onOpen的参数就是showModal输入的参数
    model.ID = id;
    model.Name = name;
}

async function processor () {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {
        await formRef.value.validate();
        let msg = await axios.post("/api/dict/SaveDictionary", model);
        if (!msg.success) {
            errors.value = msg.errorStrings;
            return;
        } else {
            close([ {...model} ]);
            Object.assign(model, emptyModel);
        }
    } catch (err) {

    }
}
</script>

<style scoped lang="less"></style>