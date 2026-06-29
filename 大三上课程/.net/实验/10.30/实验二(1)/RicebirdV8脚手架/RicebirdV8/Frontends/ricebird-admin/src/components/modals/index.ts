// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import { Modal, message } from "ant-design-vue"
import { createVNode, ref, defineComponent, createBlock, openBlock, Fragment, VNode } from "vue"
import { createIcon } from '../Icons'
import { VueNode } from "ant-design-vue/es/_util/type";

export interface CommonResponse
{ 
    success : boolean, 
    msg : string
}

export interface Modal {
    confirm(param: {
        title?: string,
        content?: string,
        icon?: string | VueNode,
    }): Promise<Boolean>,
    /**
     * 返回一个正在加载中的提示。
     * 返回值是一个函数，这个函数输入参数则会修改 加载中的文字。不输入参数则关闭加载中状态。
     * @param title 
     * @param duration 单位是秒
     */
    loading(title: string, duration?: number): Function,
    removeConfirm(message?: string, title?: string): Promise<Boolean>,
    info(title: string, content : string | VNode) :void
    warning(title: string, content : string | VNode) :void
    error(title: string, content : string | VNode) :void
    /**
     * 按照返回中的success字段，显示提示
     * @param msg 
     */
    showResponse(msg : CommonResponse) : void
}

const modals : Modal = {
    info (title, content) {
        Modal.info({
            title: title,
            content: content,
            onOk() {
                // console.log('ok');
            },
        });
    },
    warning (title, content) {
        Modal.warning({
            title: title,
            content: content,
            onOk() {
                // console.log('ok');
            },
        });
    },
    error (title, content) {
        Modal.error({
            title: title,
            content: content,
            onOk() {
                // console.log('ok');
            },
        });
    },
    loading (title : string, duration : number = 3600) : Function {
        let textRef = ref(title)
        let dynamic = defineComponent({
            render () {
                return (openBlock(), createBlock(Fragment, null, [ createVNode("span", null, textRef.value, 1) ]))
            }
        });
        
        let vnode = createVNode(dynamic);
        let exit = message.loading(vnode, duration);
        
        return (text : string) : void => {
            if (typeof text !== 'number' && !text) exit();
            textRef.value = text;
        }
    },
    confirm(param: { title?: string, content?: string, icon?: string | VueNode }): Promise<Boolean> {
        const promise = new Promise<Boolean>((resolve, reject) => {
            let icon : any = param.icon || "ExclamationCircleOutlined";
            if (typeof icon === 'string') {
                icon = createIcon(icon);
            }
            Modal.confirm({
                title: param.title || "",
                content: param.content || "",
                icon: createVNode(icon),
                onOk() {
                    resolve(true);
                },
                onCancel() {
                    resolve(false);
                },
            })
        });

        return promise;
    },
    removeConfirm(message?: string, title?: string): Promise<Boolean> {
        return this.confirm({
            title: title || "删除确认",
            content: message || "是否要删除此项?",
        });
    },
    showResponse(msg : { success : boolean, msg : string}) : void {
        if (msg.success) {
            message.success(msg.msg || "操作成功");
        } else {
            message.error(msg.msg, 5);
        }
    },
}

export default modals;