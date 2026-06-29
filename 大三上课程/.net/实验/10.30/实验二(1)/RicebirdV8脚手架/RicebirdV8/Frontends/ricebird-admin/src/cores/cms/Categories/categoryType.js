import { reactive } from "vue";
import { hasHome } from "../useCategories";

export const types = reactive({
    0: {
        label: "普通",
        desc: "一个普通的栏目，可以为该栏目设置子栏目。",
        icon: "folder-outlined",
        hide: false,
        allowSub: true,
        allowCreate: true,
        value: 0,
        rules: {
            Name: { required: true, message: "必须填写栏目名称" },
            DisplayOrder: { required: true, message: "必须填写排序号" },
        },
    },
    1: {
        label: "链接",
        desc: "该栏目直接指向一个链接，可以为该栏目设置子栏目，但直接在其中添加内容是没有效果的。",
        icon: "link-outlined",
        hide: false,
        allowSub: true,
        allowCreate: true,
        value: 1,
        rules: {
            Name: { required: true, message: "必须填写栏目名称" },
            DisplayOrder: { required: true, message: "必须填写排序号" },
            // LinkTo: { required: true, message: "必须填写链接指向" }
        },
        
    },
    2: {
        label: "图集",
        desc: "该栏目的内容均是图片，不可以为该栏目设置子栏目。在其中添加的所有内容都必须有标题图。",
        icon: "file-image-outlined",
        hide: false,
        allowSub: false,
        allowCreate: true,
        value: 2,
        rules: {
            Name: { required: true, message: "必须填写栏目名称" },
            DisplayOrder: { required: true, message: "必须填写排序号" },
        },
    },
    3: {
        label: "时间轴",
        desc: "该栏目的内容是一个时间轴上的节点，不可以为该栏目设置子栏目。在其中添加的所有内容都必须设置时间。",
        icon: "history-outlined",
        hide: false,
        allowSub: false,
        allowCreate: true,
        value: 3,
        rules: {
            Name: { required: true, message: "必须填写栏目名称" },
            DisplayOrder: { required: true, message: "必须填写排序号" },
        },
    },
    4: {
        label: "主页",
        desc: "该栏目的整个系统的主页，全系统有且只能有一个主页。这种栏目不能新建，也不能删除。",
        icon: "home-outlined",
        hide: hasHome,
        allowSub: true,
        allowCreate: true,
        value: 4,
        rules: {
            Name: { required: true, message: "必须填写栏目名称" },
            DisplayOrder: { required: true, message: "必须填写排序号" },
        },
    },
});

export function getTypeIcon(type) {
    return types[type] ? types[type].icon : "question-circle-outlined";
}
