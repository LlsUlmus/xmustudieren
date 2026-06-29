<template>
    <div v-if="props.readonly">{{ label ? label : value }}</div>
    <a-select v-else :value="value" @change="change" v-bind="$attrs" :options="dict" option-label-prop="selected"
        @dropdownVisibleChange="onFocus" class="dict-single-select" :showSearch="false" :style="style">
        <template #dropdownRender="{ menuNode: menu }">
            <a-input class="dropdown-input" allow-clear v-model:value="searcher" placeholder="输入关键字搜索"
                v-if="props.showSearch" @pressEnter="onEnter" ref="inputRef" />
            <a-divider class="dropdown-divider" v-if="props.showSearch" />
            <v-nodes :vnodes="menu" />
        </template>
    </a-select>
</template>

<script setup>
import {toRefs, ref, reactive, computed, watch} from 'vue'
import useDict from './useDict'

const props = defineProps({
    dict: String | Object,
    prefix: {
        type: [String, Boolean],
        default: true,
    },
    value: String | Number,
    remove: String | Number,
    valueWidth: String | Number,
    showSearch: Boolean,
    width: {
        type: String,
        default: ""
    },
    readonly: {
        type: Boolean,
        default: false
    },
    filterFunc: {
        type: Function,
        default: (item) => {
            return true;
        }
    }
});

let emits = defineEmits(["change", "update:value"])
let searcher = ref("");
let { value } = toRefs(props);
let { dict } = useDict({
    ...props,
    ...(props.readonly ? { remove: undefined } : { remove: props.remove })
}, searcher);
let label = computed(() => dict.value.find(item => item.value === value.value)?.label);
let style = reactive({
    width: ''
});
const inputRef = ref(null);

if (props.width) {
    style.width = props.width
} else {
    style.width = false
}

function change(v) {
    searcher.value = "";
    emits("update:value", v);
    emits("change", v);
}

function onEnter() {
    change(dict.value[0].value);
}

function onFocus() {
    // inputRef.value.focus();
}
</script>

<style lang="less">
.dict-single-select {
    .ant-select-selection-item {
        display: flex;

        .key {
            display: block;
            margin-right: 8px;
            color: rgba(0, 0, 0, 0.88) !important;
        }

        .value {
            padding-right: 12px;
            display: block;
            .text-cut;
        }
    }
}

.ant-select-dropdown {
    .dropdown-input {
        padding: 4px 8px;
        margin: 4px 0px 0px 4px;
        box-sizing: border-box;
        width: calc(100% - 8px);
    }

    .dropdown-divider {
        margin: 4px 0px;
    }
}
</style>