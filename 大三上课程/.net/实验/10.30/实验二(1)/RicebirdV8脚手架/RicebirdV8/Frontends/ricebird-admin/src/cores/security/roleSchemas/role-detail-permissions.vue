<template>
    <a-row class="permissions" v-for="(v, k) in data" :key="k">
        <a-col :span="3" class="title-block">
            <span class="mr">{{ v.module }}</span>：
            <!-- <a-icon icon="check-circle-outlined" class="color-success btn mr" />
            <a-icon icon="close-circle-outlined" class="color-error btn" />： -->
        </a-col>
        <a-col :span="21">
            <RoleTag :enable="props.enable" @change="change" :name="v2.name" :title="v2.label" :value="getValue(v2.name)" :noSetAs="props.noSetAs" v-for="(v2, k2) in v.children" :key="k2"/>
        </a-col>
    </a-row>
</template>

<script setup>
import RoleTag from './components/role-tag.vue';
import { watchEffect, ref, } from 'vue'

const props = defineProps({
    data: Array,
    noSetAs: Number,
    value: Array,
    enable: {
        type: Boolean,
        default: true
    }
})

let noSetAs = ref(1);
let data = ref([]);
let selected = ref([]);
const emits = defineEmits(["update:value"]);
watchEffect(() => {
    noSetAs.value = props.noSetAs;
    data.value = props.data;
    selected.value = props.value;
});

function getValue (name) {
    var index = selected.value.findIndex(e => e.Name === name);
    if (index === -1) {
        return index;
    } else {
        return selected.value[index].Result;
    }
}

function change ({ Name, Result }) {
    var index = selected.value.findIndex(e => e.Name === Name);
    if (index === -1) {
        selected.value.push({
            Name, Result
        });
    } else {
        selected.value[index].Result = Result;
    }
    emits("update:value", selected.value);
}
</script>

<style lang="less" scoped>
.permissions {
    margin-bottom: 8px;
    .title-block {
        line-height: 40px;
        padding-right: 16px;
        box-sizing: border-box;
        text-align: right;

        .mr {
            margin-right: 8px;            
        }
        .btn {
            cursor: pointer;
        }
    }
}
</style>