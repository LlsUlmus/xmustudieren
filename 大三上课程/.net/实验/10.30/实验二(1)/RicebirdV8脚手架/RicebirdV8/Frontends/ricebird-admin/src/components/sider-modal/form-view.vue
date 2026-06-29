<template>
    <div class="modal-sub-view form-sub-view" v-show="activeState">
        <a-form v-bind="$attrs" ref="formRef">
            <slot>
            </slot>
            <a-form-item class="footer-action">
                <a-button type="primary" @click.prevent="onSubmit" :loading="loading">提交</a-button>
                <a-button @click="onCancel">关闭</a-button>
            </a-form-item>
        </a-form>
    </div>
</template>

<script setup>
import { ref, inject } from 'vue'
import useSubView from './useSubView'

const props = defineProps({
    icon: String,
    title: {
        type: String,
        required: true,
    },
    disabled: {
        type: Boolean,
        default: false
    },
    extraIcon: {
        type: String,
        default: ""
    }
});

let { activeState, } = useSubView(props);
const emits = defineEmits(["submit", "cancel"]);
const loading = ref(false);
const close = inject("close");
const formRef = ref();
async function onSubmit () {
    loading.value = true;
    try {
        await emits("submit", { formRef, close, loading });
    } catch (err) {
        loading.value = false;
    }
}

async function onCancel () {
    emits("cancel", { formRef, close, loading });
    close({ formRef, close, loading });
}
</script>