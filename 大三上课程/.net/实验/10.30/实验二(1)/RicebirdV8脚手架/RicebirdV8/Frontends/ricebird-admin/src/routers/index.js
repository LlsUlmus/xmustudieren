// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import { createRouter, createWebHistory } from 'vue-router'
import { buildRouteRecord, allAccessPages } from './pageGenerator'
import { setRouter } from './menu-service'

let { routes } = buildRouteRecord();

const router = createRouter({
    history: createWebHistory(),
    routes,
});

const { currentPath } = setRouter(router);

router.beforeEach((to) => {
    let path = to.meta.as || to.path;
    currentPath.path = path;
    currentPath.allAccess = to.meta.allAccess;
});

export default router;