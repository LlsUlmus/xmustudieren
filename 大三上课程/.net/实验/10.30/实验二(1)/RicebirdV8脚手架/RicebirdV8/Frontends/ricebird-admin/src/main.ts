import { createApp } from 'vue';
import antd from 'ant-design-vue';
import router from '@/routers';
import AppComponent from '@/App.vue';
import { login, dashboard } from '@/all-page'
/* 上述内容是生成APP必须要的资源，不能将其移入 app.ts 中！ */
import app from './app';

// 这一行代码向下，才可以使用需要网络连接的资源
import components from '@/components/use.js';
import security from '@/cores/security';
import schedules from '@/cores/schedules';
import './global.less'
import { validateCurrentUser } from './cores/security/useCurrentUser';

validateCurrentUser().then(res => {
    const modules: any = [components, security, schedules];
    let vue = createApp(AppComponent);
    vue.use(antd);
    vue.use(router);

    // 引用模块
    for (let mod of modules) {
        vue.use(mod);
    }

    app.vue = vue;
    app.router = router;
    vue.mount("#app");

    if (!res) {
        app.router.push(login);
    }
});
