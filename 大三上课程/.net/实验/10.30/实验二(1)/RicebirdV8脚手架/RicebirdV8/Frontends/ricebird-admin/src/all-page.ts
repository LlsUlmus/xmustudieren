// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import MenuItem from '@/routers/MenuItem'

const dashboard = "/manage/security/menus"
const login = '/authen/login';

// 所有layout 都必须延迟调用
// const V4Layout = () => import('@layouts/V4Layout.vue');
const BasicLayout = () => import('@layouts/V4Layout.vue'); // BasicLayout
const LoginLayout = () => import('@layouts/LoginLayout.vue');



const modules : MenuItem[] = [];

const schedules : MenuItem[] = [
    {
        display: "任务管理",
        icon: "login-outlined",
        name: "schedule-base",
        component: BasicLayout,
        manual: true,
        children: [
            {
                display: "我的任务",
                name: "my-schedule",
                path: "/manage/schedules",
                allAccesss: true,
                component: () =>import(/* webpackChunkName: "cores" */"@vc/schedules/index.vue"),
            }
        ]
    }
];

const cms: MenuItem = {
    display: "内容管理",
    path: "/manage/cms",
    icon: "pic-left-outlined",
    name: "cms-manager",
    component: BasicLayout,
    
    children: [
        {
            display: "栏目管理",
            path: "/manage/cms/categories",

            name: "cms-categories",
            component: () =>
                import(/* webpackChunkName: "cores" */ "@vc/cms/Categories/index.vue"),
        },
        {
            display: "内容管理",
            path: "/manage/cms/content",

            name: "cms-content",
            component: () =>
                import(/* webpackChunkName: "cores" */ "@vc/cms/Contents/index.vue"),
        },
        {
            display: "内容详细",
            path: "/manage/cms/content-detail",
            as: "/manage/cms/content",
            manual: true,

            name: "cms-detail",
            component: () =>
                import(/* webpackChunkName: "cores" */ "@vc/cms/Contents/edit-content.vue"),
        }
    ]
};

const security : Array<MenuItem> = [
    {
        display: "组织机构",
        icon: "idcard-outlined",
        name: "security-modules",
        component: BasicLayout,
        children: [
            {
                display: "用户管理",
                path: "/manage/security/users",
                name: "user-manager",
                component: () =>
                    import(/* webpackChunkName: "cores" */ "@vc/security/organizations/users/index.vue"),
            },
            {
                display: "部门管理",
                path: "/manage/security/departs",
                name: "depart-manager",
                component: () =>
                    import(/* webpackChunkName: "cores" */ "@vc/security/organizations/departs/index.vue"),
            },
            {
                display: "组织关系管理",
                path: "/manage/security/relationships",
                name: "relationship-manager",
                component: () =>
                    import(/* webpackChunkName: "cores" */ "@vc/security/organizations/relationships/index.vue"),
            },
        ],
    },
];

const configurations: Array<MenuItem> = [
    {
        display: "系统配置",
        icon: "setting-outlined",
        name: "configurations",
        component: BasicLayout,
        children: [
            {
                display: "角色管理",
                path: "/manage/security/roles",
                name: "role-schema-manager",
                component: () =>
                    import(/* webpackChunkName: "cores" */ "@vc/security/roleSchemas/index.vue"),
            },
            {
                display: "菜单管理",
                path: "/manage/security/menus",
                name: "menu-manager",
                component: () =>
                    import(/* webpackChunkName: "cores" */ "@vc/security/menus/index.vue"),
            },
            {
                display: "数据字典管理",
                name: "data-dictionary",
                path: "/manage/config/dict",
                component: () =>
                    import(/* webpackChunkName: "cores" */ "@vc/configurations/data-dictionary/index.vue"),
            },
            {
                display: "系统功能列表",
                name: "api-list",
                path: "/manage/security/func-list",
                component: () =>
                    import(/* webpackChunkName: "cores" */ "@vc/security/apis/index.vue"),
            },
        ]
    }
];

const logins : Array<MenuItem> = [
    {
        display: "登录与注册",
        icon: "login-outlined",
        name: "login-base",
        component: LoginLayout,
        manual: true,
        children: [
            {
                display: "着陆页",
                name: "landing-page",
                path: "/",
                component: () =>
                    import(/* webpackChunkName: "authen" */ "@vc/login/login.vue"),
            },
            {
                display: "登录页",
                name: "login",
                path: "/authen/login",
                component: () =>
                    import(
            /* webpackChunkName: "authen"*/  import.meta.env.MODE === "production" 
                            ? "@vc/login/login.vue"
                            : "@vc/login/login-development.vue"
                    ),
            }
        ]
    }
];

export default [ ...modules, ...security, ...configurations, ...schedules, ...logins, cms];
export {
    dashboard,
    login
}