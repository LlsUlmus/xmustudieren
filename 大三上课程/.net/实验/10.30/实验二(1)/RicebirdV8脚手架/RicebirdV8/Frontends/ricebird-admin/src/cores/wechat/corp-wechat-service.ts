
import { ref, reactive, computed, Ref, ComputedRef } from 'vue'
import signalR from '@/signalR'
import { axios } from '@/axios';
import app from '@/app';
interface Status {
    mask: Boolean,
    icon?: string,
    color?: string,
    qr?: "",
    response?: "",
    desc: string
}
type AvaliableStatus = "preload" | "scan" | "response" | "success" | "error" | "complete";
interface StatusDict {
    [key : string]: Status,
}

const statusDict : StatusDict = {
    "preload": {
        mask: true,
        icon: "LoadingOutlined",
        color: "",
        desc: "正在向服务器拉取二维码"
    },
    "scan": {
        mask: false,
        desc: "使用企业微信扫描二维码",
        qr: ""
    },
    "response": {
        mask: true,
        icon: "LoadingOutlined",
        color: "",
        desc: "成功扫码，等待服务器响应"
    },
    "success": {
        mask: true,
        icon: "CheckCircleFilled",
        color: "color-success",
        desc: "等待手机端响应"
    },
    "error": {
        mask: true,
        icon: "CloseCircleFilled",
        color: "color-error",
        desc: "请与管理员确认您已正确加入企业微信"
    },
    "complete": {
        mask: true,
        icon: "CheckCircleFilled",
        color: "color-success",
        desc: "操作成功"
    },
}

const state : Ref<AvaliableStatus> = ref("preload");
const currentState : Ref<Status> = computed(_ => statusDict[state.value]);
let connId : string = "";
signalR.then(conn => {
    connId = conn.connectionId || "";  
    state.value = "preload";
    conn.on("corp-callback-log", function ([msg]) {
        // app.isDevelopment() && console.log(`openId有返回: ${msg}`);
    });

    conn.on("corp-callback-success", function ([success]) {
        try{
            if (success) {
                state.value = "success";
            } else {
                throw 1;
            }
        } catch {
            state.value = "error";
        }
    });

    conn.on("corp-callback-complete", function ([success, msg]) {
        try{
            if (success) {
                state.value = "complete";
                currentState.value.response = msg;
            } else {
                throw 1;
            }
        } catch {
            state.value = "error";
        }
    });
});

async function getQr (url : string) {
    state.value = "preload";
    await signalR;
    try {
        let msg = await axios.post<any, any>(url, { connId });
        if (msg.success) {
            state.value = "scan";
            currentState.value.qr = msg.url;
        } else {
            throw 1;
        }
    } catch {
        state.value = "error";
    }
}

export { state, currentState, getQr }