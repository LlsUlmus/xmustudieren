<template>
    <a-form class="paper-container">
        <div :class="[{ 'review-enabled': canReview && PermissionLevel >=3 }, 'page-div', 'narrow']" ref="page">
            <a-alert class="error" style="margin-bottom: 15px ;"  v-if="errors && errors.length" type="error" showIcon
            :message="`保存申请时发生错误：`">
                <template #description>
                    <ul>
                        <li v-for="(v, k) in errors" :key="k">{{ v }}</li>
                    </ul>
                 </template>
             </a-alert>
            <a-alert class="mb16" type="info" v-if="isAllowEdit(articlemodel.Status , articlemodel.DepartId) && !startEdit" showIcon message="填写提示">
                <template #description>
                    当前页面您仅可查看，点击页面最下方开始编辑按钮后即可开始编辑。<br/>
                </template>
            </a-alert>
            <a-alert class="mb16" type="info" v-if="startEdit" showIcon message="填写提示">
                <template #description>
                    <ul>
                        <li>1. 填写“申请创新学分”字段，并且此字段的数字在0-5之间，则系统会自动新建创新学分项目。反之，系统不会自动新建创新学分项目。</li>
                        <li>2. 刊物级别以人事处核心学术刊物相关规定为标准。</li>
                        <li>3. 创新成果论文的附件一般为两个：
                            <ol>
                                <li>论文发表PDF原文</li>
                                <li>图书馆出具的论文检索证明（SCI\EI需要提供）</li>
                            </ol>
                        </li>
                        <li>4. 仅在有指导老师时，方可勾选“导师是否为一作”的确认框。</li>
                    </ul>                    
                </template>
            </a-alert>
            <table class="main-table basic-info-table">
                <colgroup>
                    <col style="width: 100px;">
                    <col style="width: 200px;">
                    <col style="width: 100px;">
                    <col style="width: 200px;">
                </colgroup>

                <tr>
                    <th>姓名</th>
                    <td>
                        {{ articlemodel.StuName }}
                    </td>
                    <th>学号</th>
                    <td>
                        {{ articlemodel.StuCode }}
                    </td>
                </tr>
                <tr>
                    <th>学院<span class="need-field">*</span></th>
                    <td>
                        {{ articlemodel.DepartName }}
                    </td>
                    <th>专业<span class="need-field">*</span></th>
                    <td>
                        {{ articlemodel.Major }}
                    </td>
                </tr>
                <tr>
                    <th>手机号码<span class="need-field">*</span></th>
                    <td>
                        {{ articlemodel.Telephone }}
                    </td>
                    <th>发表时间<span class="need-field">*</span></th>
                    <td>
                        <date-picker v-if="!readonly" :bordered="false" format="YYYY-M-D" v-model:value="articlemodel.ReleaseTime" style="width:200px" :readonly/>
                        <span v-else>{{ articlemodel.ReleaseTime }}</span>
                    </td>
                </tr>
                <tr>
                    <th>论文名称<span class="need-field">*</span></th>
                    <td colspan="3">
                        <a-input v-if="!readonly" placeholder="请输入论文名称" v-model:value="articlemodel.Achieved" :show-count="true"
                                 :bordered="false" :readonly/>
                        <span v-else>{{ articlemodel.Achieved }}</span>
                    </td>
                </tr>
                <tr>
                    <th>指导老师工号</th>
                    <td>
                        <a-input v-if="!readonly" v-model:value="articlemodel.GuideTeacherCode" placeholder="输入工号以搜索"
                                 :bordered="false" :maxlength="30"
                                 @preeEnter="getUserByCode(articlemodel)" @blur="getUserByCode(articlemodel)"
                                 :show-count="true" :readonly
                        />
                        <span v-else>{{ articlemodel.GuideTeacherCode }}</span>
                    </td>
                    <th>指导老师姓名</th>
                    <td>
                        {{ articlemodel.GuideTeacher }}
                    </td>
                </tr>
                <tr>
                    <th>发表刊物名称 <span class="need-field">*</span></th>
                    <td colspan="3">
                        <a-input v-if="!readonly" placeholder="此项必填" v-model:value="articlemodel.Journal"
                                 :maxlength="50" :show-count="true"
                                 :bordered="false" :readonly/>
                        <span v-else>{{ articlemodel.Journal }}</span>
                    </td>
                </tr>
                <tr>
                    <th>论文卷号（期号）起止页码<span class="need-field">*</span></th>
                    <td colspan="3">
                        <a-input v-if="!readonly" placeholder="此项必填" v-model:value="articlemodel.PageNum"
                                 :maxlength="50" :show-count="true"
                                 :bordered="false" :readonly/>
                        <span v-else>{{ articlemodel.PageNum }}</span>
                    </td>
                </tr>
                <tr>
                    <th>导师是否为一作</th>
                    <td>
                        <a-checkbox v-if="!readonly" :bordered="false" v-model:checked="articlemodel.GuideTeacherIsFirstAuthor" :disabled="!isGuideTeacherCodePresent || readonly" />
                        <span>{{ articlemodel.GuideTeacherIsFirstAuthor ? '是' : '否' }}</span>
                    </td>

                    <th>第几作者 <span class="need-field">*</span></th>
                    <td>
                        <a-select v-if="!readonly" :bordered="false" v-model:value="articlemodel.AuthorSort"
                                  :options="filteredAuthorSortOptions" class="info-select" :disabled="readonly" />
                        <span v-else>{{ articlemodel.AuthorSort }}</span>
                    </td>
                </tr>
                <tr>
                    <th>获得何种资助</th>
                    <td>
                        <a-select v-if="!readonly" :bordered="false" v-model:value="articlemodel.FundType" @change="setFundType"
                                  :options="FundTypeOption" default-value="" class="info-select" :disabled="readonly" />
                        <span v-else>{{ articlemodel.FundType }}</span>
                    </td>

                    <th>立项时间<span class="need-field" v-if="articlemodel.FundType !== '无'">*</span></th>
                    <td>
                        <date-picker v-if="!readonly && articlemodel.FundType !== '无'" :bordered="false" format="YYYY-M-D" v-model:value="articlemodel.ProTime" style="width:200px" :readonly/>
                        <span v-else>{{ (['1970年1月1日', '1970-1-1', ''].indexOf(articlemodel.ProTime) >= 0) ? "无" : articlemodel.ProTime }}</span>
                    </td>
                </tr>
                <tr>
                    <th>发表刊物级别<span class="need-field">*</span></th>
                    <td>
                        <a-select v-if="!readonly" :bordered="false" v-model:value="articlemodel.JournalLevel"
                                  :options="JournalLevelOption" default-value="" class="info-select" :disabled="readonly" />
                        <span v-else>{{ articlemodel.JournalLevel }}</span>
                    </td>

                    <th>其它级别</th>
                    <td>
                        <a-input v-if="!readonly" v-model:value="articlemodel.OtherLevel"
                                 :maxlength="50" :show-count="true"
                                 :bordered="false" :readonly/>
                        <span v-else>{{ articlemodel.OtherLevel }}</span>
                    </td>
                </tr>
                <tr>
                    <th>发表文章影响因子<span class="need-field">*</span></th>
                    <td colspan="3">
                        <a-input v-if="!readonly" placeholder="此项必填" v-model:value="articlemodel.Effect"
                                 :maxlength="50" :show-count="true"
                                 :bordered="false" :readonly/>
                        <span v-else>{{ articlemodel.Effect }}</span>
                    </td>
                </tr>
                <tr>
                    <th>论文网络链接<span class="need-field">*</span></th>
                    <td colspan="3">
                        <a-input v-if="!readonly" placeholder="此项必填" v-model:value="articlemodel.ArticleUrl"
                                  :show-count="true"
                                 :bordered="false" :readonly/>
                        <span style="overflow-wrap: break-word" v-else>
                            <template v-if="articlemodel.ArticleUrl.startsWith('http')">
                                <a :href="articlemodel.ArticleUrl" target="_blank">
                                    {{ articlemodel.ArticleUrl }}
                                </a>
                            </template>
                            <template v-else>
                                {{ articlemodel.ArticleUrl }}
                            </template>
                        </span>
                    </td>
                </tr>
                <tr>
                    <th>论文收录情况<span class="need-field">*</span></th>
                    <td colspan="3">
                        <a-select v-if="!readonly" :bordered="false" v-model:value="articlemodel.Captures"
                                  :options="CapturesOption" default-value="" class="info-select" style="width:550px" :disabled="readonly" />
                        <span v-else>{{ articlemodel.Captures }}</span>
                    </td>
                </tr>
                <tr>
                    <th>申请学分
                        (0-5)<span class="need-field">*</span>
                        <span v-if="!isNew" @click="toCreditDetail(articlemodel.RelateId)" class="pointer-cursor"
                              title="点击跳转学分详情" >点击跳转</span>
                    </th>
                    <td>
                        <a-input-number v-if="!readonly" min="1" v-model:value="articlemodel.Score"
                                        placeholder="请输入申请学分" :bordered="false"
                                        default-value="1" class="info-select" :readonly/>
                        <span v-else>{{ articlemodel.Score }}</span>
                    </td>
                    <th>项目状态</th>
                    <td>
                        {{ statusLabel }}
                    </td>
                </tr>
            </table>
            <table class="main-table basic-info-table" style="border-top:none;">
                <colgroup>
                    <col style="width: 116.762px;">
                    <col style="width: 450px;">
                </colgroup>
                <tr style="height: 100px; overflow-y: auto" class="no-top">
                    <th>论文发表PDF原文</th>
                    <td colspan="2">
                        <div class="attachment">
                            <Uploader :allowed-suffixes="['pdf'] " :readonly="readonly" ref="articlePdf"
                            :attachments="articlemodel.Attachments" :usage="'论文原文'" />
                        </div>
                        <div class="write-tip" v-if="!readonly" >
                            论文发表原文以PDF格式上传
                        </div>
                    </td>
                </tr>
                <tr style="height: 100px; overflow-y: auto" class="no-top">
                    <th>图书馆出具的论文检索证明</th>
                    <td colspan="2">
                        <div class="attachment">
                            <Uploader :readonly="readonly" ref="libraryPdf" :allowed-suffixes="['pdf'] " 
                            :attachments="articlemodel.Attachments" :usage="'论文检索证明'" />
                        </div>
                        <div class="write-tip" v-if="!readonly">
                            检索证明以PDF格式上传
                        </div>
                    </td>
                </tr>
                <tr style="height: 100px; overflow-y: auto" class="no-top">
                    <th>指导老师审核意见</th>
                    <td colspan="2">
                        <a-textarea :autosize="{minRows: 4, maxRows: 10}" :bordered="false"
                        v-model:value="articlemodel.GuideTeacherOpinion" :readonly="(readonly || PermissionLevel<2) && !canReview"/>
                    </td>
                </tr>
                <tr style="height: 100px; overflow-y: auto" class="no-top">
                    <th>学院审核意见</th>
                    <td colspan="2">
                        <a-textarea :autosize="{minRows: 4, maxRows: 10}" :bordered="false"
                        v-model:value="articlemodel.DepartmentOpinion" :readonly="(readonly || PermissionLevel<3) && !canReview"/>
                    </td>
                </tr>
            </table>
            <a-flex justify="space-between" class="submit-btn" >
                <!-- 教师和院级管理员二选按钮-->
                <a-space v-if="canReview && !props.isNew && !isStudent">
                        <a-button class="orange" @click="save(0)" :loading="submitLoading"> 打回至学生重新填写 </a-button>
                        <a-button class="green" @click="save(articlemodel.Status === 1 ? 16 : 8 )" :loading="submitLoading">
                            {{ articlemodel.Status === 1 ? '指导老师' : (articlemodel.Status === 16 ? '学院' : "") }}审核通过
                        </a-button>
                        <a-button class="red" v-if="PermissionLevel>=3" @click="save(11)" :loading="submitLoading">不通过</a-button>
                </a-space>
                <a-space v-if="startEdit">
                    <!-- 审核按钮 -->
                    <a-select :options="getAllowedWinnerStatusOptions(PermissionLevel)" v-model:value="articlemodel.Status" class="info-select"
                    style="width: 200px" v-if="!props.isNew && !isStudent" :disabled="readonly"/>
                    <!-- 审核按钮 -->
                    <a-button type="primary" @click="submitCredit(articlemodel.Status)" :loading="submitLoading">
                        提交
                    </a-button>
                </a-space>
            </a-flex>
            <div class="submit-btn" v-if="isAllowEdit(articlemodel.Status,articlemodel.DepartId) && !startEdit">
                <a-button @click="startEdit = true">
                    开始编辑
                </a-button>
            </div>
        </div>
    </a-form>
    <ReviewCreditList v-if="canReview && PermissionLevel>=3" :student-code="articlemodel.StuCode" :competition-award="articlemodel.Journal"
        :competition-name="articlemodel.Achieved" :student-name="articlemodel.StuName"
    />
</template>

<script setup>
import {h, onMounted, reactive, ref, watch, watchEffect ,computed} from "vue";
import Uploader from "@/modules/components/uploader.vue";
import app from "@/app.ts";
import axios from "@/axios";
import {message, Modal} from "ant-design-vue";
import  Articles from "@vm/article/Articles";
import ReviewCreditList from "@vm/components/review-credit-list.vue";
import { useRouter, useRoute } from "vue-router";
import {achievementStatusToText , getCompetitionPermissionLevel , isAllowEdit , getAllowedWinnerStatusOptions} from "./useArticles";
import {toCreditDetail} from "@vm/credits/CreditOption";

const errors = ref([]);
const page = ref(null);
const props = defineProps({
    ID:{
        type: String,
        default: app.GUID_EMPTY
    },
    data:{
        type: Object,
        default: Articles.getEmptyArticle()
    },
    isNew:{
        type: Boolean,
        default: true
    }
});

const articlemodel = reactive(props.data);
const status = ref(articlemodel.Status);

const route = useRoute();
const startEdit = ref(props.isNew || route.query.startEdit === '1');

const router = useRouter();
const articlePdf=ref();
const libraryPdf=ref();
//const PermissionLevel = computed(() => getCompetitionPermissionLevel(articlemodel.DepartId));
const PermissionLevel = ref(1);
const isStudent = computed(() => {return (PermissionLevel.value<=1);});
const readonly = ref(true);
const canReview = ref(false);

const submitLoading = ref(false);

const filteredAuthorSortOptions = computed(() => {
      if (articlemodel.GuideTeacherIsFirstAuthor) {
        return AuthorSortOption.value.filter(option => option.value !== '1');
      }
      return AuthorSortOption.value;
});

const isGuideTeacherCodePresent = computed(()=>{
    return articlemodel.GuideTeacherCode.trim() !== '';
})

const AuthorSortOption =ref([
    { value: '0', label: '请选择第几作者'},
    { value: '1', label: '1' },
    { value: '2', label: '2' },
    { value: '3', label: '3' },
    { value: '4', label: '4' }]);
const FundTypeOption = ref([
    { value: '大学生创新创业训练计划', label: '大学生创新创业训练计划' },
    { value: '校长基金本科生项目', label: '校长基金本科生项目' },
    { value: '拔尖贵仪项目', label: '拔尖贵仪项目' },
    { value: '开放创新项目', label: '开放创新项目' },
    { value: '院级资助', label: '院级资助' },
    { value: '其他', label: '其他' },
    { value: '无', label: '无' }]);
const JournalLevelOption = ref([
    { value: '一类核心学术刊物', label: '一类核心学术刊物' },
    { value: '二类核心学术刊物', label: '二类核心学术刊物' },
    { value: '其它公开出版的学术刊物', label: '其它公开出版的学术刊物' }]);
const CapturesOption =ref([
    { value: "无", label: "无" },
    { value: "SCI（科学引文索引）", label: "SCI（科学引文索引）" },
    { value: "SSCI（社会科学引文索引）", label: "SSCI（社会科学引文索引）" },
    { value: "EI（工程索引）", label: "EI（工程索引）" },
    { value: "ISTP（科技会议录索引）", label: "ISTP（科技会议录索引）" },
    { value: "CSCD（中国科技期刊引证报告）", label: "CSCD（中国科技期刊引证报告）" },
    { value: "CSSCI（中文社会科学引文索引）", label: "CSSCI（中文社会科学引文索引）" }
]);
articlemodel.GuideTeacherIsFirstAuthor = false;
articlemodel.AuthorSort = "0";
articlemodel.DepartId=app.GUID_EMPTY;
const statusLabel = computed(() => achievementStatusToText(Number(articlemodel.Status)));
const currentYear = new Date().getFullYear();

function setFundType () {
    if (articlemodel.FundType === '无') articlemodel.ProTime = '1970-1-1';
}

async function computeStatus() {
    readonly.value = !isAllowEdit(articlemodel.Status,articlemodel.DepartId) || !startEdit.value ;
    canReview.value = isAllowEdit(articlemodel.Status,articlemodel.DepartId) && [1, 16, 4].includes(articlemodel.Status);
}

async function load() {
    if(props.isNew){
        const response = await axios.post("/api/credit/common/GetCredit", articlemodel);
        articlemodel.StuCode=response.data.OwnerCode;
        articlemodel.StuName=response.data.OwnerName;
        articlemodel.DepartName=response.data.OwnerCollege;
        articlemodel.Major=response.data.OwnerSpecialty;
        articlemodel.Telephone=response.data.OwnerMobile;
        articlemodel.GuideTeacher=response.data.GuideTeacherName;
        console.log(response.data);
        await getUserByCode(articlemodel);
    }
};

async function getUserByCode(item) {
    if (!item.GuideTeacherCode) {
        item.GuideTeacher = '工号为空代表无指导老师';
        return;
    }
    let msg = await axios.post("/api/proj/common/GetUserByCode", {code: item.GuideTeacherCode, role: '教职工'});
    let member = {...msg.data};
    item.GuideTeacherCode = member.code;
    item.GuideTeacher = !!member.code ? member.realName : `找不到该工号`;
};

function submitCredit() {
    if (isStudent.value) {
        if (articlemodel.GuideTeacherCode) {
            save(1);//老师
        } else {
            save(16);//学院
        }
    }else{
        save(articlemodel.Status);
    }
}

async function save(status) {
    validate();
    if (errors.value.length > 0) {
        scrollToPageDiv();
        return;
    }
    const temp = Object.assign({}, articlemodel);
    temp['论文原文'] = articlePdf.value.uploadIDs;
    temp['论文检索证明'] = libraryPdf.value.uploadIDs;
    temp['Attachments'] = [];
    temp.Status = status;
    const confirmed = await app.modals.confirm({
        title: `您确定保存修改吗？`,
        content: h('div', {}, [
            h('p', {style: {color: 'red'}}, '提交后不可更改。')
        ]),
    });
    if (!confirmed) {
        return;
    }
    submitLoading.value = true;
    try {
        temp.ApplyYear = !temp.ApplyYear ? currentYear : temp.ApplyYear;
        let msg = await axios.post("/api/achievements/article/SaveArticle", temp);
        if (msg.success) {
            message.success("提交成功");
            articlemodel.Status = status;
            router.push('/manage/article/list');
        } else {
            errors.value = msg.errorStrings;
            scrollToPageDiv();
        }
    }catch (e) {
        message.error(errors.value || "提交失败，请稍后再试。");
    } finally {
        submitLoading.value = false;
    }
}

function validate() {
    errors.value = [];
    if (!articlePdf.value.uploadIDs) {
        errors.value.push("请上传论文原文PPT");
    }
    if (!libraryPdf.value.uploadIDs) {
        errors.value.push("请上传检索证明PPT");
    }
    if (status.value === -1) {
        errors.value.push("请选择评审意见");
    }
    if (articlemodel.FundType !== '无' && (['1970年1月1日', '1970-1-1', ''].indexOf(articlemodel.ProTime) >= 0)) {
        errors.value.push("必须填写立项时间");
    }
}

function scrollToPageDiv() {
    if (page.value) {
        page.value.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }
}

watch(articlemodel , ()=>{
    PermissionLevel.value = getCompetitionPermissionLevel(articlemodel.DepartId);
    computeStatus();

}, { deep: true })

watch(startEdit,computeStatus);

onMounted(() => {
      load();
    });
</script>

<style scoped lang="less">
@import "@/assets/less/paper/paper.less";
@import "@/assets/less/paper/progress-paper.less";
@import "@/assets/less/colorful-button.less";

.info-select {
    width: 200px;
    background-color: white;
    
    :deep( .ant-input-number-input) {
        padding-left: 0;
    }
}

.upload-btn {
    margin-top: 20px;
    margin-right: 20px;
}

.submit-btn {
    margin-top: 20px;
}

.write-tip{
    padding-bottom: 10px;
}

.review-enabled  {
    position: relative;
    left: -100px;
    margin-left: auto;
    margin-right: auto;
    width: fit-content; /* 确保内容宽度适应 */
}

.pointer-cursor {
    cursor: pointer;
    color: dodgerblue; 
}

.space-right-align {
    justify-content: flex-end;
}
</style>