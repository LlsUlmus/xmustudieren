<template>
    <!-- :color="style[checked].color" -->
    <a-tag class="role-tag-selector" :class="{ [style[checked].color]: true }" @click="change" :title
        :style="{ cursor: props.enable ? 'pointer' : 'default' }">
        <template #icon>
            <a-icon :icon="style[checked].icon" class="icon" />
        </template>
        <span class="tag-name">
            {{ finalTitle }}
        </span>
    </a-tag>
</template>
<script setup>
import { watchEffect, reactive, ref, computed, } from 'vue'
const props = defineProps({
    value: {
        type: Number,
        default: -1
    },
    noSetAs: {
        type: Number,
        default: 1
    },
    title: String,
    name: String,
    enable: {
        type: Boolean,
        default: true
    }
})

const finalTitle = computed(() => {
    let result = props.title ? props.title : "未知";
    if (result.length > 9) {
        result = `${result.substring(0, 8)}...`;
    }
    return result;
});

const style = reactive({
    [-1] : {
        color: "default",
        icon: "minus-circle-outlined"
    },
    0: {
        color: "success",
        icon: "check-circle-outlined"
    },
    1: {
        color: "error",
        icon: "close-circle-outlined"
    },
})
const checked = ref(-1);
watchEffect(() => {
    checked.value = props.value;
    style[-1].color = style[props.noSetAs].color;
});

const emits = defineEmits(["update:value", "change"]);
function change () {
    if (!props.enable) {
        return;
    }
    if (checked.value === -1) {
        checked.value = props.noSetAs;
    }
    checked.value = checked.value === 1 ? 0 : 1;
    emits("update:value", checked.value);
    emits("change", {
        Name: props.name,
        Result: checked.value
    });
}
</script>
<style lang="less">
.role-tag-selector {
    font-size: 14px;
    line-height: 35px;
    cursor: pointer;
    margin-bottom: 8px;
    &.success {
        border-color: #b7eb8f;
        .icon {
            color: #52c41a;
        }
    }
    &.error {
        border-color: #ffccc7;
        .icon {
            color: #ff4d4f;
        }
    }
    .tag-name {
        width: 10em;
        display: inline-block;
    }
}
</style>