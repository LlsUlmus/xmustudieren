// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import { RouteRecordRaw } from 'vue-router'
import exportItem from '@/all-page'

let finalRoutes : Array<RouteRecordRaw> = [];
let finder : PathFinder = {};
let descriptors : Array<MenuDescriptor> = [];
interface PathFinder { [path: string]: { icon: string, name: string, display: string, to: string, manual: boolean, as?: string } };
interface MenuDescriptor  { icon: string, name: string, display: string, to: string, manual: boolean, path: string, order: number, allAccess: boolean, children?: Array<MenuDescriptor> };
export let allAccessPages : string[] = [];
export function buildRouteRecord () : { routes: Array<RouteRecordRaw>, finder : PathFinder, descriptors: Array<MenuDescriptor> } {
    if (finalRoutes && finalRoutes.length > 0) return { routes: finalRoutes, finder, descriptors };

    finalRoutes = [];
    finder = {};
    descriptors = [];
    // 路由最多两层
    let lv1Order = 100;
    for (let item of exportItem) {
        // 循环第一层
        let obj : RouteRecordRaw = {
            name: item.name,
            path: item.path || "",
            component: item.component,
            children: [],
            meta: {
                as: item.as,
                allAccess: !!item.allAccesss,
            }
        }

        let desc : MenuDescriptor = {
            icon: item.icon || "",
            name: item.name,
            display: item.display,
            to: item.display,
            manual: !!item.manual,
            path: item.path || item.name,
            allAccess: !!item.allAccesss,
            order: lv1Order,
        }
        lv1Order += 100;
        if (!item.children) {
            // 只有一层的情况
            finder[item.path || ""] = { ...desc, as: item.as };
        } else {
            obj.path = "/individual/parent";
            desc.children = [];
            let lv2Order = 100;
            // 有两层的情况
            for (let lv2 of item.children) {
                obj.children.push({
                    name: lv2.name,
                    path: lv2.path || "",
                    component: lv2.component,
                    meta: {
                        as: lv2.as,
                        allAccess: !!lv2.allAccesss,
                    }
                });

                let lv2Desc : MenuDescriptor = {
                    icon: lv2.icon || "",
                    name: lv2.name,
                    display: lv2.display,
                    to: `${item.display} > ${lv2.display}`,
                    manual: !!lv2.manual,
                    path: lv2.path || "",
                    allAccess: !!item.allAccesss,
                    order: lv2Order,
                };
                lv2Order += 100;
                finder[lv2.path || ""] = { ...lv2Desc, as: lv2.as }
                desc.children.push(lv2Desc);
            }
        } // if
        finalRoutes.push(obj);
        descriptors.push(desc);
    } // for

    finalRoutes.push({
        name: '404',
        path: "/:pathMatch(.*)*",
        component: () => import(/* webpackChunkName: "error" */ "@vc/exceptions/404.vue")
    });
    return { routes: finalRoutes, finder, descriptors  };
}// buildRouteRecord

export default exportItem;