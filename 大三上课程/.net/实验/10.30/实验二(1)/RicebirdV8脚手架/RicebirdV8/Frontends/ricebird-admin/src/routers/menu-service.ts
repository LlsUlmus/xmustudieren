// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import MenuItem from './MenuItem'
import { reactive, ref, Ref, VNode, watch, createVNode } from 'vue'
import { createIcon } from '@/components/Icons'
import { Router, RouterLink } from 'vue-router';
import webConfig from '../configs/web-config';

// --- useMenus ---- //
const openKeys : Ref<Array<string>> = ref([]);
const selectedKeys : Ref<Array<string>> = ref([]);
const menus : Ref<Array<any>> = ref([]);
const pathNavMapping : { [url: string]: () => any } = {};
let router : Router = {} as Router;

export const currentPath = reactive({
    path: "",
    allAccess: false
});
export const menuVersion = ref(0);

watch(currentPath, nv => {
    nv && toMenuItem(nv.path);
    menuVersion.value++;
}, {deep: true});

function setTitle (title : string) : void {
    document.title = title ? `${title} - ${webConfig.name}` : webConfig.name;
}

function generateKeys (currentMenus : Array<MenuItem>) {
    let m = [];
    for (let lv1 of currentMenus) { 
        if (!lv1.children || !lv1.children.length) {
            // 只有一层的情况下
            let  lv1Item = {
                key: lv1.name,
                label: lv1.display,
                icon: createIcon(lv1.icon),
                title: lv1.display,
                path: lv1.path,
                linkType: lv1.linkType,
                level: 1,
            };
            m.push(lv1Item);
            if (lv1.path) {
                pathNavMapping[lv1.path] = () => {
                    setTitle(lv1.display);
                    selectedKeys.value = [ lv1Item.key ];
                    return lv1Item;
                }
            }
        }
        else {
            let group : { key: string, icon: VNode, label: string, children: Array<any> } = {
                key: lv1.name,
                label: lv1.display,
                icon: createIcon(lv1.icon),
                children: []
            }
            for (let lv2 of lv1.children) {
                if (!lv1.path) break;
                let lv2Item = {
                    key: lv2.name,
                    label: createVNode(RouterLink, { to: lv2.path }, () => lv2.display),
                    icon: undefined,
                    title: lv2.display,
                    path: lv2.path,
                    linkType: lv2.linkType,
                    level: 2,
                };
                group.children.push(lv2Item)
                if (lv2.path) {
                    pathNavMapping[lv2.path] = () => {
                        setTitle(lv2.display);
                        openKeys.value = [group.key];
                        selectedKeys.value = [ lv2Item.key ];
                        return lv2Item;
                    }
                }
                // 循环第二层
            } // for - lv2
            m.push(group);
        }
    } //  for - lv1

    menus.value = m;
    toMenuItem(currentPath.path);
    if (!open.length) {
        // 即：无权访问此项
        // toPage(landing);
    }
}

export function toMenuItem(path: string) {
    let mapping = pathNavMapping[path];
    if (mapping) {
        return mapping();
    }
    return null;
}

function onMenuClick ({ item } : { item : any }) {
    let result = toMenuItem(item.path);
    internalToPage(result)
}

function internalToPage (node: any) {
    if (node.level === 1) {
        if (node.linkType === 1) {
            window.open(node.path);
        } else {
            router.push({
                path: node.path || ""
            })
        }
    }
}

export function useMenus (currentMenus? : Array<MenuItem>) {
    if (currentMenus) generateKeys(currentMenus);
    menuVersion.value++;

    return {
        menus, openKeys, selectedKeys, onMenuClick, generateKeys,
        currentPath, menuVersion
    }
}

export function findMenuItem(lv1: string, lv2: string): { lv1: string, lv2: string} {
    for (let lv1Item of menus.value) {
        if (lv1Item.label === lv1) {
            if (!lv2 || !lv1Item.children) {
                return {
                    lv1: lv1Item.key,
                    lv2: ""
                }
            }
            for (let lv2Item of lv1Item.children) {
                if (lv2Item.title === lv2) {
                    return {
                        lv1: lv1Item.key,
                        lv2: lv2Item.key
                    }
                }
            }
        }
    }
    return {
        lv1: "",
        lv2: ""
    };
}

export function setMenuItem(lv1: string, lv2: string) {
    let key = findMenuItem(lv1, lv2);
    if (!key.lv2) {
        openKeys.value = [];
        selectedKeys.value = [key.lv1];
    } else {
        openKeys.value = [key.lv1];
        selectedKeys.value = [key.lv2];
    }
}

export function setRouter(r : Router) {
    router = r;
    (r as unknown as any).currentPath = currentPath;
    (r as unknown as any).useMenus = useMenus;
    return { currentPath, useMenus };
}

// --- useMenus ---- //