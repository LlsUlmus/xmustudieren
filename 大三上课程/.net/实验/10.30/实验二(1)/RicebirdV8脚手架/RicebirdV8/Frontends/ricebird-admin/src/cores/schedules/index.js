import { defineAsyncComponent } from 'vue';

export default {
    install(app) {
        app.component("import-button", defineAsyncComponent(() => import("./import-button.vue")));
        app.component("export-button", defineAsyncComponent(() => import("./export-button.vue")));
    }
}