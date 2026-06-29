import { createVNode, App, VNodeTypes, VNode, defineAsyncComponent } from "vue";
import * as Icons from '@ant-design/icons-vue';
import { VueNode, Data } from "ant-design-vue/es/_util/type";
import IconSelector from './IconSelector.vue'

export const iconComponents: {
    [prop: string]: VueNode
} = {};

export const selector: {
    [prop: string]: VueNode
} = {};

const icons: Array<any> = Icons as unknown as Array<any>;
for (let i in icons) {
    const ele = icons[i];
    let name = i.replace(/(?!^)([A-Z])/g, "-$1").toLowerCase();
    iconComponents[name] = ele;
    iconComponents[i] = ele;
    selector[name] = ele;
}

const Icon = (props: { icon: string, [prop: string]: unknown }, { attrs }: { attrs: Data, [prop: string]: unknown }) => {
    const { icon } = props;
    let com = iconComponents[icon] as unknown as VNodeTypes;

    if (!com) {
        com = iconComponents["question-circle-outlined"] as unknown as VNodeTypes;
    }

    return createVNode(com, attrs);
}

export function createIcon (icon? : string, props? : any, children? : any) : VNode {
    let com = iconComponents[icon || ""] as unknown as VNodeTypes;
    return createVNode(com, props, children);
}

function install(app: App) {
    app.component("a-icon", Icon);
    app.component("icon-selector", IconSelector);
    for (let key in iconComponents) {
        let com = iconComponents[key] as unknown as VNodeTypes;
        if (com) {
            const promise = new Promise<any>((resolve, reject) => {
                resolve((prop : any, { attrs } : any) => {
                    return createVNode(com, attrs);
                });
            });
            app.component(key, defineAsyncComponent(() => promise));
        }
    }
}

export default install;