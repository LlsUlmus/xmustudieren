<template>
    <a-modal title="绑定企业微信" :attrs="$attrs" v-model:open="open" destroyOnClose :maskClosable="false">
        <a-flex :gap="16" class="get-openId-dialog">
            <div class="qr-area">
                <a-qrcode :value="currentState.qr || ''"/>
                <div class="mask" v-if="currentState.mask">
                    <a-icon :icon="currentState.icon" class="icon" :class="{[currentState.color]: true}" />
                </div>
            </div>
            <div class="text-desc">
                {{ currentState.desc }}
            </div>
        </a-flex>
        <template #footer>
            <a-button key="back" @click="onCancel">取消</a-button>
            <a-button key="submit" type="primary" :loading="loading" @click="onOk" v-if="false">提交</a-button>
        </template>
    </a-modal>
</template>

<script setup>
import app from '@/app'
import { ref, reactive, computed, watch } from 'vue'
import useModal from '@/components/modals/useModal'
import axios from '@/axios'
import signalR from '@/signalR'
import { state, currentState, getQr } from './corp-wechat-service'

const {
    open, loading, 
    onOk, onCancel,
    close, showModal
} = useModal(onOpen, processor, onClosing);

defineExpose({
    showModal
});

let timerId = "";
function onClosing () {
    clearInterval(timerId);
}

function onOpen (url) {
    // 窗口打开时，处理这里的逻辑，onOpen的参数就是showModal输入的参数
    // STEP3：把输入的参数合并到模型里
    getQr(url);
    timerId = setInterval(_ => {
        getQr(url);
    }, 50000);
}

let response = "";
watch(state, nv => {
    if (nv === "complete") {
        response = currentState.value.response;
        setTimeout(_ =>  close(response), 500);
    } else { nv === "error"} {
        onClosing();
    }
})

async function processor () {
    // 处理完成后，根据实际情况确认是否调用 close 关闭窗口，close的参数就是返回调用方的值
    try {

    } catch (err) {

    }
}
</script>

<style scoped lang="less">
.get-openId-dialog {
    .qr-area {
        width: 160px;
        height: 160px;
        position: relative;
        .mask {
            position: absolute;
            top: 0px;
            left: 0px;
            right: 0px;
            bottom: 0px;
            background: #fff;
            // backdrop-filter: blur(5px);
            display: flex;
            justify-content: center;
            align-items: center;
            .icon {
                font-size: 50px;
            }
        }
    }

    .text-desc {
        text-align: center;
        line-height: 160px;
        font-size: 16px;
    }
}
</style>./corp-wechat-service
