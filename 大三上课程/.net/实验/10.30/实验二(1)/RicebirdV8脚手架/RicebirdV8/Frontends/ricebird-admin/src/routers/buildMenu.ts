import axios from '@/axios'
import { ref, Ref, VNode, watch } from 'vue'
import { buildRouteRecord } from './pageGenerator'

let { finder, descriptors } = buildRouteRecord();
// ---- buildMenus ----//
export async function buildMenus(vnode : VNode) {
    const setEl = (str : string) => {
        if (vnode.el) vnode.el.innerHTML = str;
    }

    for (let menu of descriptors) {
        if (menu.manual) {
            continue;
        }

        let param = {
            ID: "",
            display: menu.display,
            path: menu.path,
            icon: menu.icon,
            linkType: menu.children ? 3 : 2,
            parentId: "",
            displayOrder: menu.order
        };

        setEl(`正在上传${menu.to}...`);
        let msg = await axios.post<any, any>("/api/menu/BuildMenu", param);

        if (!msg.success) continue;
        param.ID = msg.data.ID;

        if (!menu.children) continue; // 没有菜单项就下一个
        let children = menu.children || [];
        for (let lv2 of children) {
            if (lv2.manual) continue;
            let lv2Param = {
                display: lv2.display,
                path: lv2.path,
                icon: lv2.icon || "",
                linkType: 2,
                parentId: param.ID,
                displayOrder: lv2.order
            };
            setEl(`正在上传${lv2.to}...`);
            await axios.post("/api/menu/BuildMenu", lv2Param);
            // message.success(`成功上传${param.display} > ${lv2Param.display}`);
        }
    }

    await axios.post("/api/menu/ReloadMenu");
    // message.success(`所有菜单上传成功`);
}

export function getMenuOptions() {
    let options: Ref<Array<any>> = ref([]);
    for (let lv1 of descriptors) {
        if (!lv1.children) {
            options.value.push({
                value: lv1.path,
                label: lv1.display,
            });
        } else {
            let obj: any = {
                label: lv1.display,
                options: []
            }
            for (let lv2 of lv1.children) {
                obj.options.push({
                    value: lv2.path,
                    label: lv2.display
                });
            }
            options.value.push(obj);
        }
    }
    return options;
}

export function getMenuByPath(path: string) : string {
    let result = finder[path];
    return result ? result.to : "页面已被删除";
}
// --- buildMenus --- //