<template>
    <dict-select v-bind="$attrs" :dict="dict" @change="change" :value="props.value" />
</template>
<script setup>
import { ref, reactive, watch, onMounted } from 'vue'
const GUID_EMPTY = "00000000-0000-0000-0000-000000000000";
const props = defineProps({
    value: {
        type: String,
        default: GUID_EMPTY
    },
    empty: {
        type: String,
        default: "任意角色"
    },
    disableEmpty: {
        type: Boolean,
        default: false
    },
    disableSetFor: {
        type: Number,
        default: -1000
    }
});

let emits = defineEmits(["change", "update:value"])
const dict = reactive({
    name: "选择角色",
    entries: []
});

onMounted(async () => {
    let { dataSource } = await import('../useRole');
    watch(() => dataSource.ver, () => {
        let entries = [{
            DataKey: GUID_EMPTY,
            DataValue: props.empty,
            SetFor: -1,
            Enable: !props.disableEmpty
        }];
        for (let item of dataSource.data) {
            let entry = {
                DataKey: item.key,
                DataValue: item.title,
                SetFor: item.setFor,
                Enable: true
            };
            if (props.disableSetFor === item.setFor) {
                entry.Enable = false;
            }
            entries.push(entry);
        }
        dict.entries = entries;
    }, { immediate: true });

    watch(() => props.disableSetFor, nv => {
        for (let item of dict.entries) {
            item.Enable = (item.SetFor !== nv);
        }
    });
});

function change(v) {
    emits("update:value", v);
    emits("change", v);
}
</script>