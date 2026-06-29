<template>
    <a-radio-group :value="value" @update:value="change" v-bind="$attrs">
        <a-radio-button :value="v.value" v-for="(v, k) in dict" :key="v.ID" :disabled="v.disabled">{{ v.label }}</a-radio-button>
    </a-radio-group>
</template>

<script setup>
import { toRefs } from 'vue'
import useDict from './useDict'
import { Form } from 'ant-design-vue';
const { id, onFieldChange, onFieldBlur } = Form.useInjectFormItemContext()
const props = defineProps({
    dict: String | Object,
    value: String | Number,
    remove: String | Number
});
let { dict } = useDict(props);
let emits = defineEmits(["change", "update:value"])
let { value } = toRefs(props);
function change (v) {
    onFieldChange();
    emits("update:value", v);
    emits("change", v);
}
</script>