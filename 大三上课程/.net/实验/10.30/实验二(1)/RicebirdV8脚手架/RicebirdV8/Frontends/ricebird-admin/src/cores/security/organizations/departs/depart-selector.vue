<template>
    <dict-select v-bind="$attrs" :dict="source" @change="change" :value="props.value" class="depart-selector"
        show-search v-if="props.schema" :prefix="props.prefix" :style :remove="disableEmpty ? app.GUID_EMPTY : false" />
    <a-tree-select v-else :value="props.value" @change="change" :treeData="tree" :style="style"
        :treeDefaultExpandAll="false" :dropdownMatchSelectWidth="false" placeholder="请选择部门" show-search
        treeNodeFilterProp="title">
    </a-tree-select>
</template>

<script setup>
import app from '@/app'
import { ref, onMounted, watch, reactive, toRefs } from 'vue'
import { pinyin } from 'pinyin-pro'

const props = defineProps({
    value: {
        type: String,
        default: app.GUID_EMPTY
    },
    schema: {
        type: String,
        default: ""
    },
    base: {
        type: String,
        default: app.baseDepart
    },
    empty: {
        type: String,
        default: "所有部门"
    },
    prefix: {
        type: [String, Boolean],
        default: true,
    },
    disableEmpty: {
        type: Boolean,
        default: false
    },
    width: {
        type: String,
    },
})

let style = reactive({
    width: ''
});

if (props.width) {
    style.width = props.width
} else {
    style.width = false
}

let emits = defineEmits(["change", "update:value"])
const source = reactive({
    name: "选择部门",
    entries: []
});
const { schema } = toRefs(props);
const tree = ref([]);

onMounted(async () => {
    let { createDataSource } = await import('./useDepartment');
    const treeSource = createDataSource(false);
    watch(() => treeSource.ver, () => {
        if (props.schema) {
            let entries = treeSource.query()
                .whereIf("SchemaName", schema)
                .orderBy([item => pinyin(item.Name, { toneType: 'none' })])
                .map((node) => {
                    return {
                        DataKey: node.ID,
                        DataValue: node.ShortName || node.Name,
                    }
                })
                .end();
            entries.splice(0, 0, {
                DataKey: app.GUID_EMPTY,
                DataValue: props.empty,
            });
            source.entries = entries;
        } else {
            const emtpyNode = {
                value: app.GUID_EMPTY,
                title: props.empty,
                disabled: props.disableEmpty
            };
            // 步骤1 找到base对应的节点
            let baseNode = searchTree(treeSource.data, props.base);
            let nodes = buildTree(baseNode);
            tree.value = [emtpyNode, ...nodes];
        }
    }, { immediate: true });
});

function buildTree(data) {
    if (data && !(data.children instanceof Array)) return undefined;
    let nodes = [];
    for (let item of data.children) {
        nodes.push({
            value: item.key,
            title: item.title,
            children: buildTree(item)
        });
    }
    return nodes;
}

function searchTree(data, base) {
    if (base === app.GUID_EMPTY) {
        let result = { children: [] };
        for (let item of data) {
            if (item.key !== app.GUID_EMPTY) result.children.push(item);
        }
        return result;
    }
    if (!(data instanceof Array)) return;
    for (let item of data) {
        if (item.key === base) return item;
        searchTree(data.children, base);
    }
}

function change(v) {
    emits("update:value", v);
    emits("change", v);
}
</script>

<style lang="less">
.depart-selector {
    .ant-select-selection-item {
        display: flex;

        .key {
            display: block;
        }

        .value {
            // width: 13em;
            display: block;
            .text-cut;
        }
    }
}
</style>