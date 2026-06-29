<template>
    <a-button @click="submit" v-bind="$attrs">
        <a-icon :icon="props.icon"></a-icon>
        <slot />
    </a-button>
</template>

<script setup>
import app from '@/app';
import axios from '@/axios';
import { onScheduleComplete } from './schedule-service'
const props = defineProps({
    action: {
        type: String,
        required: true,
    },
    params: {
        type: Object,
        default: {}
    },
    icon: {
        type: String,
        default: "CloudDownloadOutlined"
    },
    // 如果有值，则需要先确认再发送消息
    confirmMessage: {
        type: String,
        default: ""
    }
})
const emits = defineEmits(["click", "complete"]);
async function submit(ev) {
    if (props.confirmMessage) {
        let confirm = await app.modals.removeConfirm(props.confirmMessage, "操作确认");
        if (!confirm) return;
    }

    emits("click", ev);
    let msg = await axios.post(props.action, props.params);
    if (!msg.success) {
        app.modals.error("导出错误", msg.msg);
    } else {
        let id = msg.id;
        onScheduleComplete(id, schedule => {
            emits("complete", schedule);
        })
    }
}
</script>