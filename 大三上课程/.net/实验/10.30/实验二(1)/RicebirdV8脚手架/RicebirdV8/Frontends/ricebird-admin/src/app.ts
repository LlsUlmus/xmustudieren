import type { App as Vue, ComputedRef } from 'vue';
import type { RouteLocationRaw, Router } from 'vue-router';
import type { WebConfig } from './configs/web-config';
import type { resolveResult} from '@/utils/PromiseHelper';
import { accessToken, currentUser, CurrentUser, Role } from './cores/security/useCurrentUser';
import { sha256, sha1 } from '@/utils/sha256';
import webConfig from './configs/web-config';
import modals, { Modal } from './components/modals';
import withResolvers from '@/utils/PromiseHelper';
import { buildDictionary, Entry } from '@/utils/DataDictionary'
import viewport from './viewport';

// 所有模块
interface App {
    web: WebConfig,
    modals: Modal,
    vue?: Vue<any>,
    router?: Router,
    createVue() : void,
    sha1(message: string): string,
    sha256(message: string): string,
    toPage(to: RouteLocationRaw | string): void,
    newPage(to: RouteLocationRaw | string): void,
    delay(ms: number): Promise<any>,
    GUID_EMPTY: string,
    DATE_EMPTY: string,
    baseDepart: string,
    toText(dict: string, key: string): string,
    accessToken: { requestKey: string, token: string },
    currentUser: CurrentUser,
    succeed(permissionOrMenu: string): Boolean,
    getRole (role : string) : Role | undefined,
    getRoles (role: string) : Role[],
    /**
     * 检查当前用户是否包含指定角色集合
     * @param roles 需要检查的角色数组
     * @returns 当前用户角色包含所有输入角色时返回true
     */
    isInRoles (roles : string[]) : boolean,
    setTitle (title : string): void,
    withResolvers<T> () : resolveResult<T>,
    isDevelopment () : Boolean,
    queryString (key : string) : string,
    buildDictionary (dictName : string, empty : Entry, entries : string[]) : ComputedRef,
    viewport: typeof viewport,
    getPermissionLevel() : number,
    getExpertManagePermission(departId : string, isCol : Boolean) : Boolean,
}

const app: App = {
    web: webConfig,
    modals,
    GUID_EMPTY: "00000000-0000-0000-0000-000000000000", // "0000000000000000000000",
    DATE_EMPTY: "1970-1-1",
    baseDepart: "70ca9a3a-a116-41fd-93f7-a5d421aec030", // 厦门大学的ID
    createVue () {
        
    },
    toPage(to: RouteLocationRaw | string) {
        if (this.router) {
            this.router.push(to)        
        }
    },
    newPage(to: RouteLocationRaw | string) {
        if (this.router) {
            const resolved = this.router.resolve(to);
            window.open(resolved.href, '_blank');
        }
    },
    delay(ms) {
        return new Promise((resolve) => {
            setTimeout(resolve, ms);
        });
    },
    toText: () => "",
    setTitle (title) {
        document.title = title ? `${title} - ${webConfig.name}` : webConfig.name;
    },
    sha1,
    sha256,
    accessToken,
    currentUser,
    succeed: currentUser.succeed,
    getRole: currentUser.getRole,
    getRoles: currentUser.getRoles,
    isInRoles: currentUser.isInRoles,
    withResolvers,
    buildDictionary: buildDictionary,
    viewport,
    queryString (key : string) {
        if (this.router) {
            let value = this.router.currentRoute.value.query[key];
            if (Array.isArray(value)) value = value[0];
            return value || "";
        }
        return "";
    },
    isDevelopment () {
        return import.meta.env.MODE !== "production";
    },
    getPermissionLevel() {
        if (app.getRole('教务处管理员')) return 4;
        if (app.getRole('院级管理员')) return 3;
        if (app.getRole('教职工')) return 2;
        return 1;
    },
    getExpertManagePermission(departId, isCol) {
        if (app.getRole('教务处管理员')) return true;
        let role = app.getRole('院级管理员');
        if (isCol && role && role.ForDepart === departId) return true;
        return false;
    },
};

export default app;