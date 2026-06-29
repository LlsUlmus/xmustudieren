<template>
    <div class="add-category-wizard full-right-area">
        <a-tabs v-model:activeKey="activeKey" tabPosition="right">
            <a-tab-pane key="step1" tab="选择类型" class="step-1">
                <h1>步骤（1/2）：选择栏目的类型</h1>
                <a-alert class="m" showIcon message="由于内部新闻类型的不同，栏目在创建后就不能再修改分类。如果选择有误，需要删除分类后重新创建。删除分类时，与分类关联的所有新闻也会同时删除。"></a-alert>             
                <ul>
                    <template v-for="(v, k) in types">
                        <li class="media" v-if="v.allowCreate && !v.hide" :key="k" @click="choose(v.value)">
                        <div class="icon-area">
                            <a-icon :icon="v.icon" />
                        </div>
                        <div class="media-body">
                            <h2 class="media-title">
                                {{ v.label }}
                            </h2>
                            <div class="desc">
                                {{ v.desc }}
                            </div>
                        </div>
                    </li>
                    </template>
                    <li class="media" @click="cancel">
                        <div class="icon-area">
                            <rollback-outlined />
                        </div>
                        <div class="media-body">
                            <h2 class="media-title">
                                取消添加栏目
                            </h2>
                            <div class="desc">
                                不再添加栏目，直接返回上层。
                            </div>
                        </div>
                    </li>
                </ul>
            </a-tab-pane>
            <a-tab-pane key="step2" tab="设置栏目属性">
                <edit-category :model="entity" @cancel="activeKey = 'step1';" @onComplete="onComplete"/>
            </a-tab-pane>
        </a-tabs>
    </div>
</template>

<script setup>;
import app from '@/app'
import { reactive, ref, watch, watchEffect } from 'vue';
import { types } from './categoryType'
import { loadCategory } from '../useCategories'
import editCategory from './edit-category.vue';

const activeKey = ref("step1");
const categoryType = ref(0);
const emit = defineEmits();
let entity = reactive({});
function cancel () {
    emit("cancel");
}

async function choose (value) {
    let msg = await loadCategory(app.GUID_EMPTY);
    const cate = reactive(msg.data);
    cate.CategoryType = value;
    categoryType.value = value;
    
    Object.assign(entity, cate);
    activeKey.value = "step2";
}

function onComplete (id) {
    emit("onComplete", id);
}

</script>

<style lang="less" scoped>
.add-category-wizard {
    top: 115px;
    
    background: #fff;
    z-index: 99;
    overflow-y: auto;

    .step-1 {
        h1 {
            margin-bottom: 5px;
        }
    }

    .m {
        margin: 20px 0px;
    }
}
</style>