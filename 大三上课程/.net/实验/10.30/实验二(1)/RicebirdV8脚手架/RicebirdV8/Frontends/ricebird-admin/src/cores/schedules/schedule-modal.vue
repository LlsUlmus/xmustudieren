<template>
    <a-modal :attrs="$attrs" v-model:open="open" :confirm-loading="loading" destroyOnClose :maskClosable="false" class="schedule-modal" @cancel="destroy">
        <template #title>
            {{ model.Name }}进度
        </template>
        <h5 class="lg-title text-cut">
            {{ topic }}
        </h5>
        <a-progress :percent="precent" :status="model.Status === 3 ? 'exception' : ''" />
        <div class="detail" :class="{ hide: !showDetail}">
            <div class='msg' v-for="(v, k) in model.Logs" :key="k">
                {{ v }}
            </div>
        </div>
        <template #footer>
            <a-checkbox v-model:checked="showDetail" class="left">显示详细</a-checkbox>
            <a-button key="submit" type="primary" v-if="model.Status === 2 && model.DownloadPath" @click="download">下载结果</a-button>
            <a-button key="back" @click="destroy">关闭窗口</a-button>
        </template>
    </a-modal>
</template>

<script setup>
import app from '@/app'
import { ref, reactive, watchEffect, computed } from 'vue'
import useModal from '@/components/modals/useModal'
import axios from '@/axios'

const {
    open, loading,
    onOk, onCancel,
    close, showModal
} = useModal(onOpen, processor);

defineExpose({
    showModal, onClear, destroy
});

// -- 业务逻辑，点击确定后应该如何 -- //
const emptyModel = {
    ID: "",
    Name: "",
    Duration: "",
    Progress: "",
    Status: "",
    DownloadPath: "",
    ExpiredOn: "",
    Current: 0,
    Total: 0,
    Type: "",
    Logs: [],
    CurrentLog: ""
};
const model = reactive({ ...emptyModel });
const topic = computed(() => {
    switch (model.Status) {
        case 0:
            return "等待服务器响应中";
        case 1:
            return `正在处理${model.Current}/${model.Total}：${model.CurrentLog}`;
        case 2:
        case 3:
            return model.CurrentLog;
    }
});
const precent = computed(() => {
    if (model.Total === 0) {
        return model.Current === 0 ? 100 : 0;
    } else {
        return parseFloat((model.Current * 100.0 / model.Total).toFixed(2));
    }
});
const showDetail = ref(true); //ref(import.meta.env.MODE !== "production");

function download () {
    window.open(model.DownloadPath);
}

function onClear (id) {
    if (model.ID === id) {
        close();
    }
}

let stopWatch = () => {};
function onOpen(schedule) {
    // if (schedule.Status === 2) {
    //     showDetail.value = false;
    // }
    stopWatch = watchEffect(() => {
        Object.assign(model, schedule);
        // showDetail.value = true; // (import.meta.env.MODE !== "production") || (model.Type === "导入任务");
    });
}

function destroy () {
    stopWatch();
    close();
}

async function processor() {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {
        await formRef.value.validate();
        // STEP4：处理一下提交函数
        // let msg = await axios.post("/api/dict/SaveDictionary", model);
        // if (!msg.success) {
        //     errors.value = msg.errorStrings;
        //     return;
        // } else {
        //     Object.assign(model, emptyModel);
        //     close();
        // }
    } catch (err) {

    }
}
</script>

<style lang="less">
.schedule-modal {
    .lg-title {
        font-weight: 500;
        color: #333;
        margin: 0;
        margin-bottom: 5px;
        // font-size: 13px
    }

    .detail {
        padding: 10px;
        margin-bottom: 0;
        border: #ddd solid 1px;
        border-radius: 2px;
        overflow-y: scroll;
        height: 150px;
        transition: all .5s;
        box-sizing: border-box;
        &.hide {
            height: 0px;
            padding: 0px;
            border: 0px;
        }
        .msg {
            margin: 5px 0
        }
        .msg:first-child {
            margin-top: 0
        }
    }

    .ant-modal-footer {
        display: flex;
        .left {
            align-self: center;
            margin-right: auto;
        }
    }
}
</style>