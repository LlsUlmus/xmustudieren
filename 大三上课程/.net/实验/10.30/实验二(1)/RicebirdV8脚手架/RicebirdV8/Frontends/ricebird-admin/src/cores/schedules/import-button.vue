<template>
    <a-upload :disabled="disabled" name="file" :showUploadList="false" accept=".xlsx,.xls" :action="props.action" :data="params" :headers="{ [accessToken.requestKey]: accessToken.token }" @change="change" >
        <a-button :type="props.type" :disabled="disabled">
            <a-icon icon="CloudUploadOutlined"></a-icon>
            <slot />
        </a-button>
    </a-upload>
</template>

<script setup>
import app from '@/app';
import { accessToken } from '../security/useCurrentUser';
import { onScheduleComplete } from './schedule-service'
const props = defineProps({
    type: {
        type: String,
        default: 'default'
    },
    action: {
        type: String,
        required: true
    },
    params: {
        type: Object,
        default: {}
    },
    disabled: {
        type: Boolean,
        default: false,
    }
})

const emits = defineEmits(["complete"]);
async function change (e) {    
    if (e.file.status === "done") {
        let id = e.file.response.id;
        let msg = e.file.response;
        if (!msg.success) {
            app.modals.error("导出错误", msg.msg);
        } else {
            onScheduleComplete(id, schedule => {
                emits("complete", schedule);
            })
        }
    }
}
</script>