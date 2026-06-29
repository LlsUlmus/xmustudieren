<template>
    <a-form :model="entity" :rules="rules" ref="formRef" class="edit-category-form">
        <a-alert class="error" v-if="errors.length" type="error" showIcon :message="`${isCreate ? '新建' : '修改'}栏目时发生错误：`">
            <template #description>
                <ul>
                    <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                </ul>
            </template>
        </a-alert>
        <a-form-item label="栏目名称" name="Name">
            <a-input v-model:value="entity.Name" />
        </a-form-item>
        <a-row :gutter="32">
            <a-col :span="12">
                <a-form-item label="栏目编号">
                    {{ isCreate ? "等待自动生成" : entity.UniqueCode }}
                    <div class="text-secondary">
                        该代码是由系统自动生成的，仅由开发者使用，普通用户不用管此字段。
                    </div>
                </a-form-item>
            </a-col>
            <a-col :span="12">
                <a-form-item label="上级分类">
                    {{ entity.ParentName || "根栏目" }}
                    <div class="text-secondary">
                        修改上级分类，请使用“概览”里的剪切和复制功能。
                    </div>
                </a-form-item>
            </a-col>
        </a-row>
        <a-row :gutter="32">
            <a-col :span="12">
                <a-form-item label="排序号" name="DisplayOrder">
                    <a-input v-model:value="entity.DisplayOrder" :disabled="entity.CategoryType === 4" />
                    <div class="text-secondary">
                        排序号是由升序的，数值越小越在前。{{ entity.CategoryType === 4 ? "首页类型的栏目，排序号恒定为0" : "" }}
                    </div>
                </a-form-item>
            </a-col>
            <a-col :span="12">
                <a-form-item label="可见性" name="CategoryStatus" v-if="entity.CategoryType !== 4">
                    <a-switch v-model:checked="entity.CategoryStatus" checked-children="开" un-checked-children="关" :checkedValue="0" :unCheckedValue="1" />
                </a-form-item>
            </a-col>
        </a-row>
        <a-form-item label="链接位置" v-if="entity.CategoryType === 1" name="LinkTo">
            <!-- 仅链接类型的分类需要填写此字段 -->
            <a-input v-model:value="entity.LinkTo" placeholder="填写链接向目指向的链接" />
        </a-form-item>
        <a-divider />
        <a-row :gutter="16">
            <a-col span="16">
                <a-form-item label="关键字" name="SeoKeyword">
                    <a-input v-model:value="entity.SeoKeyword" placeholder="该字段是提供给搜索引擎使用的，可以不填写"/>
                </a-form-item>
                <a-form-item label="栏目描述" name="SeoDescription">
                    <a-textarea v-model:value="entity.SeoDescription" placeholder="这里填写栏目的介绍，如果没有则不填" :rows="4" />
                </a-form-item>
                <a-divider />
                <a-form-item label="栏目事项" name="SpecialMessageInCategory">
                    <a-textarea v-model:value="entity.SpecialMessageInCategory" placeholder="显示在栏目页的注意事项" :rows="4" />
                    <div class="text-secondary">
                        注意事项之间，使用回车符分隔。
                    </div>
                </a-form-item>
                <a-form-item label="内容事项" name="SpecialMessageInContent">
                    <a-textarea v-model:value="entity.SpecialMessageInContent" placeholder="显示在内容页的注意事项" :rows="4" />
                    <div class="text-secondary">
                        注意事项之间，使用回车符分隔。
                    </div>
                </a-form-item>
                <a-form-item>
                    <a-space>
                        <a-button type="primary" @click="submit" :loading="loading">提交</a-button>
                        <a-button @click="cancel">取消</a-button>
                    </a-space>
                </a-form-item>
            </a-col>
            <a-col span="8">
                <h1>
                    <a-space>
                        <span>栏目封面</span>
                        <a href="javascript:void(0);" @click="removeFeaturedImage">[删除]</a>
                    </a-space>
                </h1>
                <avatar-uploader :width="1920" :height="1080" :displayWidth="300"
                    :displayHeight="170" usage="head-image" :needCrop="false"
                    v-model:value="entity.FeaturedImage" />
            </a-col>
        </a-row>
    </a-form>
</template>

<script setup>
import { inject, reactive, ref, watch, watchEffect } from 'vue'
import { types } from './categoryType'
import axios from '@/axios'
import { message } from 'ant-design-vue'

const props = defineProps({
    model: Object,
});
const emit = defineEmits();
const parent = inject("current");
const treeRef = inject("treeRef")
const rules = reactive({});
const formRef = ref();
const guidEmpty = "00000000-0000-0000-0000-000000000000";
const isCreate = ref(false);
const errors = ref([]);
const loading = ref(false);

let entity = {};
watchEffect(() => {
    formRef.value && formRef.value.clearValidate();
    errors.value = [];
    entity = props.model;
    var type = types[entity.CategoryType];
    Object.assign(rules, type.rules);

    if (entity.ID === guidEmpty) {
        isCreate.value = true;
        entity.ParentId = parent.ID;
        entity.ParentName = parent.Name === "所有栏目" ? "根栏目" : parent.Name;
    }
    
    if (entity.CategoryType === 4) {
        entity.DisplayOrder = 0;
    }
})

 function submit () {
    errors.value = [];
    loading.value = true;
    var valid = formRef.value.validate();
    valid.then(async res => {
        let msg = await axios.post("/api/cms/category/SaveCategory", entity);
        if (!msg.success) {
            errors.value = msg.errorStrings;
        } else {
            message.success("保存成功！");
            treeRef.value.reSync(msg.data.ID);
        }
        
        loading.value = false;
        emit("onComplete", msg.data.ID);
    }).catch(res => {
        loading.value = false;
    });
}

function cancel () {
    emit("cancel");
}

function removeFeaturedImage () {
    entity.FeaturedImage = "";
}

</script>

<style lang="less">
.edit-category-form {
    .error {
        margin: 10px 0px;
    }
    .ant-form-item-label label {
        width: 6em;
        text-align-last: justify;

        &:before {
            display: inline-block;
            margin-right: 4px;
            color: #fff;
            font-size: 14px;
            font-family: SimSun, sans-serif;
            line-height: 1;
            content: '*';
        }
    }
}
</style>