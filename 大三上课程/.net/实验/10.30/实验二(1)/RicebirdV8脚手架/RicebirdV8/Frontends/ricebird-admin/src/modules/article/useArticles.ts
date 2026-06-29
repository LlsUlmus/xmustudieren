import app from "@/app.ts";
import creditOption from "@vm/credits/CreditOption.ts";
import {computed} from "vue";

// 有效值：-1，1，16，8，11
// 请自行生成字典。这个字典实际上是“申请表状态”的一部分
export const achievementStatusOption = [
    {value: -1, label: "请选择审核状态", disabled: false},
    {value: 0, label: "申请中", disabled: false},
    {value: 1, label: "待指导老师审核", disabled: false},
    {value: 16, label: "待学院审核", disabled: false},
    {value: 8, label: "已审核", disabled: false},
    {value: 11, label: "已取消", disabled: false}
]

// export const allowAchievementStatusOption = computed(()=>{
//     const permissionLevel = getCompetitionPermissionLevel();
//     console.log(permissionLevel);
//     switch (permissionLevel) {
//         case 4: return achievementStatusOption;
//         case 3: return achievementStatusOption;
//         case 2: return achievementStatusOption.
//         map(item => [-1, 0, 1, 16].includes(item.value)? item : {...item, disabled: true});
//     }
// })

export function getAllowedWinnerStatusOptions(permissionLevel: number) {
    switch (permissionLevel) {
        case 4:
            return achievementStatusOption;
        case 3:
            return achievementStatusOption;
        case 2:
            return achievementStatusOption.filter(item => [-1, 0, 1, 16].includes(item.value));
        case 1:
            return achievementStatusOption.filter(item => [-1, 0].includes(item.value));
        default:
            return [];
    }
}

/**
 * 获取当前用户权限级别，2：普通老师，3：院级，4：教务处
 */
export function isJwc () {
    return app.succeed('教务处权限');
}
export function isCollege () {
    if (app.getRole('院级管理员')) 
        return true;
}
export function getAllowedStatusOptions(permissionLevel: number){
    switch (permissionLevel) {
        case 4: return achievementStatusOption;
        case 3: return achievementStatusOption;
        case 2: return achievementStatusOption.filter(item => [-1, 0, 1].includes(item.value));
        case 1: return achievementStatusOption.filter(item => [-1, 0, 1].includes(item.value));
        default: return [];
    }
}

export function getCompetitionPermissionLevel(departId: string) {
    if (isJwc()) return 4;
    if (isCollege() && (app.currentUser.getRole('院级管理员')?.ForDepart === departId || departId === app.GUID_EMPTY) ) {
        return 3;
    }
    return app.getPermissionLevel() === 1 ? 1 : 2;
}

export function isAllowEdit (status: number, departId: string) {
    if (isJwc()) return true;
    return creditOption.AllowEdit &&
        getAllowedStatusOptions(getCompetitionPermissionLevel(departId)).some(item => item.value === status);
}

export function achievementStatusToText(status: number){
    return achievementStatusOption.find(item => item.value === status)?.label || "未知状态";
}