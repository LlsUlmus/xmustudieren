import modal from './modal.vue'
import subView from './sub-view.vue'
import formView from './form-view.vue'

function install(app) {
    app.component("sider-modal", modal);
    app.component("sub-view", subView);
    app.component("form-view", formView);
}

export default install;