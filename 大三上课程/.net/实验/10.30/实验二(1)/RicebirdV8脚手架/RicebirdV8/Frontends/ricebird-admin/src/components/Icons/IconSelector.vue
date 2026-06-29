<template>
    <div class="icon-selector">
        <a-button type="link" @click="selectIcon">
            <a-icon :icon="value" />{{ value || "未选择图标" }}
        </a-button>
        <a-modal v-model:open="open" :footer="null" :maskClosable="false" :closable="false" :width="1000">
            <a-flex class="selector-searcher" gap="small">
                <a-radio-group v-model:value="style" @change="filterIcon()">
                    <a-radio-button value="outlined">虚线风格</a-radio-button>
                    <a-radio-button value="filled">实底风格</a-radio-button>
                </a-radio-group>
                <a-input v-model:value="filter" placeholder="输入图标名称以筛选" class="filter" @change="filterIcon()"></a-input>
            </a-flex>
            <a-flex class="icons" wrap="wrap" v-if="icons.length" gap="small">
                <div class="icon" v-for="(v, k) in icons" :key="k" @click="confirmIcon(v.value)" :class="{ selected: value === v.value }">
                    <div class="img">
                        <component :is="v.icon"></component>
                    </div>
                    <div class="title">{{ v.value }}</div>
                </div>
            </a-flex>
            <div class="icons" v-else>
                <a-empty></a-empty>
            </div>
        </a-modal>
    </div>
</template>

<script setup>
import { ref, watchEffect } from 'vue'
import { selector } from './index'
const props = defineProps({
    value: String
});
const emits = defineEmits(["update:value"]);
const value = ref("");
const open = ref(false);
const filter = ref("");
const style = ref("outlined")
const icons = ref([]);
watchEffect(() => {
    value.value = props.value;
});

function filterIcon (filterFn) {
    filterFn = filterFn || ((key, ele) => key.indexOf(style.value) >= 0 && (!filter.value || key.indexOf(filter.value) >= 0));
    icons.value = [];
    for (let key in selector) {
        let ele = selector[key];
        if (filterFn(key, ele)) {
            icons.value.push({
                value: key,
                icon: ele,
            });
        }
    }
    return icons;
}
filterIcon();

function selectIcon() {
    open.value = true;
}

function confirmIcon (nv) {
    value.value = nv;
    open.value = false;
    emits("update:value", nv);
}
</script>

<style lang="less">
.selector-searcher {
    .filter {
        flex: 1;
    }
}
.icons {
    margin-top: @margin;
    height: 600px;
    overflow-y: auto;

    .icon {
        width: 150px;
        height: 100px;
        border-radius: 10px;
        transition: .5s all;
        cursor: pointer;

        .img {
            font-size: 36px;
            text-align: center;
        }
        .title {
            text-align: center;
        }
        &:hover, &.selected {
            background: @primary-color;
            .img {
                color: #fff;
            }
        }
    }
}
</style>