// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import axios from 'axios'; // 这里必须用 axios，而不能用 @/axios
import webConfig from '@/configs/web-config';
import { reactive } from 'vue';
import { useMenus } from '@/routers/menu-service';

interface Role {
    Name: string,
    DisplayName: string,
    Menus: any[],
    NotSetEquals: number,
    DepartName: string,
    ForDepart: string,
    Permissions: string[]
}

interface CurrentUser {
    RealName: string,
    Avatar: string,
    Code: string,
    Roles: Role[],
    ID: string,
    Mobile: string,
    Email: string,
    currentRole: Role,
    currentRoleIndex: number,
    switchRole: (nv : number) => void,
    succeed (permissionOrMenu : string) : Boolean,
    getRole (role : string) : Role | undefined,
    getRoles (role: string) : Role[],
    isInRoles (roles : string[]) : boolean,
    ver: number
}

const requestKey = webConfig.requestKey;
localStorage.setItem("requestKey", requestKey);
const alphabets = "abcdefghijklmnopqrstuvwxyz";
const defaultRole : Role = { "Name": "匿名用户", "DisplayName": "匿名用户", DepartName: "", ForDepart: "", NotSetEquals: -1, "Permissions": [], "Menus": [] };

let accessToken = {
    requestKey, 
    token: ""
}

let currentUser: CurrentUser = reactive({ 
    "Code": "", 
    "RealName": "测试", 
    "Avatar": "", 
    "Roles": [ defaultRole ],
    "ID": "",
    Mobile: "",
    Email: "",
    currentRole: defaultRole,
    currentRoleIndex: 0,
    switchRole,
    succeed,
    getRole,
    getRoles,
    /**
     * 检查当前用户是否包含指定角色集合
     * @param roles 需要检查的角色数组
     * @returns 当前用户角色包含所有输入角色时返回true
     */
    isInRoles,
    ver: 0,
});
let token = "";
let isLogined = false;

function succeed (permissionOrMenu : string) : Boolean {
    if (!permissionOrMenu) return true;

    let r = currentUser.currentRole;
    if (r.Permissions.indexOf(permissionOrMenu) > -1) return true;
    let roleIndex = currentUser.Roles.findIndex(e => e.Permissions.indexOf(permissionOrMenu) > -1);
    if (roleIndex > -1) {
        switchRole(roleIndex);
        return true;
    }

    return r.NotSetEquals === 0 || !!r.Menus.find(e => { return e.path === permissionOrMenu || (e.children as any[]).find(x => x.path === permissionOrMenu) });
}

function getRole (role : string) : Role | undefined {
    return currentUser.Roles.find(e => e.Name == role);
}

function getRoles (role: string) : Role[] {
    return currentUser.Roles.filter(e => e.Name == role);
}

function isInRoles(roles: string[]): boolean {
    return roles.some(roleName => 
		currentUser.Roles.some(role => role.Name === roleName)
	);
}


let mergedMenus : any[] = [];
function switchRole (nv : number) {
    currentUser.currentRoleIndex = nv;
    currentUser.currentRole = currentUser.Roles.length > nv ? currentUser.Roles[nv] : defaultRole;
    accessToken.token = token +  alphabets[nv];
    localStorage.setItem(requestKey, accessToken.token);

    // 创新网特别处理
    if (currentUser.currentRole.Name === "本科生") {
        for (let item of currentUser.currentRole.Menus) {
            if (item.display === "论文/专利") item.display = "论文/专利学分申请";
            if (item.display === "学业竞赛") item.display = "学业竞赛学分申请";
            if (item.display === "创新学分") {
                item.children[0].display = "科创项目/自主创业学分申请";
                item.children[1].display = "我的创新学分";
            }
        }
    }
    
    if (mergedMenus.length === 0) {
        mergeMenu();
        useMenus(mergedMenus);
    }
    
    // useMenus(currentUser.currentRole.Menus);
    // currentUser.ver++;
}

function mergeMenu () {
    let menu : Array<any> = [];
    for (let role of currentUser.Roles) {
        for (let lv1 of role.Menus) {
            let exists = menu.find(e => e.path === lv1.path);
            if (!exists) {
                exists = lv1;
                menu.push(exists);
            }
            if (lv1.children instanceof Array) {
                let finalChildren = [];
                for (let lv2 of lv1.children) {
                    let lv2Exists = exists.children.find((e : any) => e.path === lv2.path);
                    if (!lv2Exists) {
                        finalChildren.push(lv2);
                    }
                }
                lv1.children = [...lv1.children, ...finalChildren];
            }
        }
    }

    menu.sort((a, b) => a.order - b.order);
    mergedMenus = menu;
    return menu;
}

async function setCurrentUser(msg: any) {
    if (!msg.success) {
        isLogined = false;
        return;
    }

    Object.assign(currentUser, msg.data);
    token = (msg.token as string);
    let index: number = alphabets.indexOf(token.charAt(token.length - 1));
    token = token.slice(0, token.length - 1);
    mergedMenus = [];
    switchRole(index);
    isLogined = true;

    let setAppFunction = await import('@/cores/configurations/data-dictionary/setAppFunction');
    await setAppFunction.setAppFunction();
}

async function validateCurrentUser () {
    accessToken.token = localStorage.getItem(requestKey) || "";
    token = accessToken.token.slice(0, token.length - 1);
    let msg = await axios.post<any, any>("/api/authorize/ValidateToken", {}, {
        headers: {
            [accessToken.requestKey]: accessToken.token
        }
    });
    msg = msg.data;
    // let msg = await axios.post<any, any>("/debug/security/ValidateToken");
    
    if (msg.success) {
        await setCurrentUser(msg);
    } else {        
        isLogined = false;
    }

    return isLogined;
}

async function logout () {
    await axios.post("/api/authorize/logout", {}, {
        headers: {
            [accessToken.requestKey]: accessToken.token
        }
    });
    isLogined = false;
    window.location.reload();
}

export {
    currentUser,
    accessToken,
    switchRole,
    setCurrentUser,
    validateCurrentUser,
    isLogined,
    logout
}

export type {
    Role,
    CurrentUser,
}