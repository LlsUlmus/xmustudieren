<template>
    <div>
        <a-list :data-source="dataSource" size="small" style="margin-bottom: 10px;">
            <template #renderItem="{ item }">
                <a-list-item class="clickable" @click="fastLogin(item)">
                    <a-list-item-meta :description="item.desc">
                        <template #title>
                            {{ item.role }}：{{ item.name }}（{{ item.code }}）
                        </template>
                    </a-list-item-meta>
                    <template #actions>
                        <a-spin v-if="item.onloading" />
                    </template>
                </a-list-item>
            </template>
        </a-list>
        <a-form-item >
            <a-input                
            size="large"
            type="text"
            placeholder="直接输入用户名"
            :value="modelValue"
            @input="codeChange"
            @pressEnter="onEnter"
            >
            </a-input>
        </a-form-item>
    </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
const prop = defineProps(["modelValue"])
let emits = defineEmits(["OnSelect", "update:modelValue"])

let dataSource = reactive([
    {
        code: 'userAdmin',
        name: '系统管理员',
        role: '系统管理员',
        desc: '',
        onloading: false
    },
]);

function resetLoading () {
    dataSource.forEach(e => {
        e.onloading = false;
    });
}

function fastLogin(item) {
    dataSource.forEach(e => {
        e.onloading = false;
    });
    item.onloading = true;
    let code = ref(item.code);
    emits("update:modelValue", code);
    emits("OnSelect", code, resetLoading);
}

function codeChange(e) {
    let code = ref(e.target.value);
    emits("update:modelValue", code);
    emits("OnSelect", code, resetLoading);
}

function onEnter (e) {
    dataSource.forEach(e => {
        e.onloading = false;
    });
    let code = ref(e.target.value);
    emits("update:modelValue", code);
    emits("OnSelect", code, resetLoading);
}
</script>

<style lang="less" scoped>
.clickable {
    transition: all .5s;
}
.clickable:hover {
    background: white;
}
</style>