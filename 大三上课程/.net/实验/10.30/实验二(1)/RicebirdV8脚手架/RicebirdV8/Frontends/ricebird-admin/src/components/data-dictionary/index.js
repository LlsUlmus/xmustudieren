import { defineAsyncComponent } from 'vue'

export default function install (app) {
    app.component("dict-select", defineAsyncComponent(() => import("./dic-select.vue")))
    app.component("dict-radio-button", defineAsyncComponent(() => import("./radio-buttons.vue")))
}