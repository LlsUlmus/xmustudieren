<template>
    <a-modal class="with-sider non-wrapper-modal" :open="open" v-bind="$attrs" @cancel="onCancel" :footer="null"
        ref="modalRef" :width="props.width || 1600" >
        <a-row type="flex" class="full-height">
            <a-col flex="256px">
                <div class="sider-modal-sider-wrapper">
                    <div class="sider-title">
                        {{ siderTitle }}
                    </div>
                    <a-menu @click="onSelectMenuItem" v-model:selectedKeys="internalActiveKeys">
                        <a-menu-item :key="v.title" v-for="v in subItems" :disabled="v.disabled">
                            <template #icon>
                                <a-icon :icon="v.icon" />
                            </template>
                            {{ v.title }}
                            <div class="tip-icon" v-if="v.extraIcon">
                                <a-icon :icon="extraIconShortcut(v.extraIcon)"
                                    :class="[extraTexoColorShortcut(v.extraIcon, v.disabled)]" />
                            </div>
                        </a-menu-item>
                    </a-menu>
                </div>
            </a-col>
            <a-col flex="auto" class="sider-container">
                <slot>
                </slot>
            </a-col>
        </a-row>
    </a-modal>
</template>

<script setup>
import { ref, watchEffect, reactive, provide } from "vue";
const props = defineProps({
    open: Boolean,
    width: {
        type: Number,
    },
    siderTitle: {
        type: String,
        required: true,
    },
    activeKey: String,
});

const emits = defineEmits(["update:activeKey", "update:open", "close"]);
const modalRef = ref();
const open = ref(false);
const internalActiveKeys = ref([]);
const subItems = ref([]);
const activeKey = ref("");
watchEffect(() => {
    open.value = props.open;
    internalActiveKeys.value = [props.activeKey];
    activeKey.value = props.activeKey;
});

provide("subItems", subItems);
provide("activeKey", activeKey);
provide("close", onCancel);

function onSelectMenuItem({ item, key }) {
    internalActiveKeys.value = [key];
    activeKey.value = key;
    emits("update:activeKey", key);
}

function onCancel() {
    open.value = false;
    emits("update:open", false);
    emits("close");
}

function extraIconShortcut(icon) {
    switch (icon) {
        case "success":
            return "check-circle-filled";
        case "error":
            return "close-circle-filled";
        case "warning":
            return "warning-filled";
        case "loading":
            return "loading-outlined";
        case "normal":
        default:
            return icon;
    }
}

function extraTexoColorShortcut(icon, disabled) {
    switch (icon) {
        case "success":
        case "error":
        case "warning":
            return disabled ? "" : `color-${icon}`;
        case "loading":
            return disabled ? "" : `color-info`;
        case "normal":
        default:
            return "";
    }
}
</script>