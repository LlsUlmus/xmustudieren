import { defineAsyncComponent } from 'vue';

export default {
    install(app) {
        app.component("no-permission", defineAsyncComponent(() => import("./no-permission.vue")));
        app.component("role-select", defineAsyncComponent(() => import("./roleSchemas/components/role-select.vue")));
        app.component("depart-select", defineAsyncComponent(() => import("./organizations/departs/depart-selector.vue")));
        app.component("depart-span", defineAsyncComponent(() => import("./organizations/departs/depart-span.vue")));
    }
}