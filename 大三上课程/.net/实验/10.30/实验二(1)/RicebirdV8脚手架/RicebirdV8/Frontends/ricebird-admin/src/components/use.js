import { defineComponent } from 'vue'
import icon from './Icons'
import siderModal from './sider-modal'
// import orgTree from './OrgnizationTree'
// import iScroll from './directives/iScroll'
// import orgSelect from './OrgSelect'
// import multiCheckbox from './MultiCheckbox'
// import auth from './directives/auth'
// import UEditor from './UEditor/components/vue-ueditor-wrap.vue'
import avatar from './AvatarUploader'
import dic from './data-dictionary'
import datePicker from './DatePicker/date-picker.vue'
import UEditor from './UEditor/components/vue-ueditor-wrap.vue'

export default {
    install(app) {
        const VNodes = defineComponent({
            props: {
                vnodes: {
                    type: Object,
                    required: true,
                },
            },
            render() {
                return this.vnodes;
            },
        });

        app.component("v-nodes", VNodes);
        icon(app);
        siderModal(app);
        // orgTree(app);
        // iScroll(app);
        // orgSelect(app);
        // multiCheckbox(app);
        // auth(app);
        avatar(app);
        dic(app);
        app.component("date-picker", datePicker);
        app.component("ueditor", UEditor);
    }
}