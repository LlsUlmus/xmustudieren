<template>
    <a-flex class="submit-btn" :gap="16" align="center" v-if="!props.readonly">
        <a-button type="primary" @click="handleAdd">
            <a-icon icon="UploadOutlined"/>
            {{ buttonText }}
        </a-button>
        <div class="tip">{{ tip }}</div>
        <span class="a-btn" @click="showFileModal" v-if="files.length">[已上传 {{ files.length }}个附件]</span> <!-- 查看按钮 -->
    </a-flex>
    
    <input type="file" :multiple="!props.single" ref="fileInput" @change="handleFileChange" style="display: none;"
        v-if="!props.readonly"/>

    <!-- 弹窗查看已上传文件 -->
    <a-modal v-model:open="modalVisible" title="已上传的文件" width="500px" :footer="null">
        <div class="files">
            <div class="file-area" v-for="(file, index) in files" :key="index">
                <div class="head">
                    <a-icon class="file-icon" :icon="getFileIcon(file)"/>
                    <div class="file-info">
                        <h4 class="filename download-btn" @click="handleDownload(index)" :title="file.name">{{ file.name }}</h4>
                        <div class="breadcrumb">上传日期：{{ getTime(file) }}</div>
                    </div>
                </div>
                <div class="del-btn" @click="handleDelete(index)" v-if="!props.readonly">删除</div>
            </div>
        </div>
    </a-modal>
</template>
    
<script setup>
import app from '@/app';
import {axios} from '@/axios';
import {message, Modal} from 'ant-design-vue';
import {ref, watch, watchEffect} from 'vue';

const props = defineProps({
    buttonText: {
        type: String,
        default: '上传附件' // 按钮文字
    },
    single: {
        type: Boolean,
        default: false // 默认支持多文件上传
    },
    maxFileSize: {
        type: Number,
        default: 20 * 1024 * 1024 // 默认最大文件大小为 20MB，0 代表不限制
    },
    allowedSuffixes: {
        type: Array,
        default: null // 默认不限制文件类型
    },
    tip: {
        type: String,
        default:null // 默认提示
    },
    attachments: {
        type: Array,
        default: [] // 原本已经上传的附件，从后端获取到的
    },
    usage: {
        type: String,
        default: "" // 如果有值，则使用这个筛选 attachments 中 Usage 为这个值的附件
    },
    readonly: {
        type: Boolean,
        default: false // 是否只读 => 可以下载，但不能上传和删除
    },
    filePos:{
        type: String,
        default:"bottom"
    }
});

const uploadIDs = defineModel(); // 附件ID字符串，这个值会在上传文件时自动更新，与需要上传的文件字段绑定就行
const files = ref(getFiles(props.attachments));
const fileInput = ref(null);
const modalVisible = ref(false); // 控制弹窗显示的状态
const allFilesValid = ref(true);

watch(() => props.attachments, (attachments) => {
    files.value = getFiles(attachments);
});
watchEffect(() => {
    uploadIDs.value = files.value.map(file => file.code).join('|');
});

function getFiles(attachments) {
    return attachments?.filter(file => (!props.usage || file.Usage === props.usage)).map(file => ({
        name: file.DisplayName,
        url: file.DownloadPath,
        code: file.UniqueCode,
        time: file.CreatedOn,
    }));
}

function handleAdd() {
    fileInput.value.click();
}

function handleFileChange(event) {
    const selectedFiles = event.target.files;
    const filesData = [];
    const formData = new FormData();
    allFilesValid.value = true;
    for (let i = 0; i < selectedFiles.length; i++) {
        const file = selectedFiles[i];
        if (!checkFile(file)) {
            allFilesValid.value = false;
            break;
        }
        formData.append(file.name, file);
        filesData.push({name: file.name, time: new Date().toISOString().split('T')[0]});
    }
    if(allFilesValid.value){
        const handler = app.modals.loading('正在上传');
        axios.post('/api/proj/upload/UploadFile', formData).then(res => {
        handler();
        message.success('上传成功');
        for (let i = 0; i < filesData.length; i++) {
            filesData[i].url = res.data[i].Download;
            filesData[i].code = res.data[i].ID;
        }
        files.value = files.value.concat(filesData);
    });
    }
}

function handleDelete(index) {
    const comfirm = Modal.confirm({
        title: '删除文件',
        content: '确定要删除该文件吗？',
        onOk() {
            files.value.splice(index, 1);
        }
    });
}

function handleDownload(index) {
    window.open(files.value[index].url);
}

function checkFile(file) {
    if (file.size > props.maxFileSize && props.maxFileSize !== 0) {
        message.error(`上传内容不能超过 ${props.maxFileSize / 1024 / 1024} MB`);
        return false;
    }
    if (props.allowedSuffixes && !props.allowedSuffixes.includes(file.name.split('.').pop())) {
        message.error(`文件类型${file.name.split('.').pop()}不允许。此处只允许上传扩展名为：${props.allowedSuffixes.join(', ')}的文件`);
        return false;
    }
    return true;
}

function getTime(file) {
    return new Date(file.time).toLocaleDateString();
}

function getFileIcon(file) {
    const suffix = file.name.split('.').pop();
    switch (suffix) {
        case 'txt':
        case 'md':
            return 'FileTextOutlined';
        case 'zip':
        case 'rar':
        case '7z':
        case 'tar':
        case 'gz':
        case 'bz2':
        case 'xz':
            return 'FileZipOutlined';
        case 'doc':
        case 'docx':
            return 'FileWordOutlined';
        case 'xls':
        case 'xlsx':
            return 'ProfileOutlined';
        case 'ppt':
        case 'pptx':
            return 'FilePptOutlined';
        case 'pdf':
            return 'FilePdfOutlined';
        case 'jpg':
        case 'jpeg':
        case 'png':
        case 'gif':
        case 'bmp':
        case 'svg':
        case 'webp':
        case 'ico':
        case 'tiff':
            return 'FileImageOutlined';
        default:
            return 'FileOutlined';
    }
}

    // 显示文件弹窗
function showFileModal() {
    modalVisible.value = true;
}

// 关闭弹窗
function closeModal() {
    modalVisible.value = false;
}

defineExpose({
    uploadIDs
})
</script>

<style lang="less">
//.uploader {
    .file{
        display: flex;
        justify-content: left;

    }

    .tip {
        color: #555;
    }

    .file-area {
        margin: 10px 0;

        &:last-child {
            margin-bottom: 0;
        }

        //border: 1px solid #e8e8e8;
        border-radius: 5px;
        padding: 5px 0 10px 0;
        display: flex;
        align-items: center;
        justify-content: space-between; /* 使子元素在水平方向上分散排列 */

        .head {
            display: flex;

            .file-icon {
                font-size: 38px;
                color: #1890ff;
                padding: 2px 2px 2px 0;
                width: 40px;
                height: 40px;
            }

            .file-info {
                display: flex;
                flex-direction: column;
                //左对齐
                vertical-align: top;
                justify-content: flex-start;
                margin-left: 8px;

                .filename {
                    margin: 0;
                    font-size: 17px;
                    line-height: 20px;
                    //太长自动折叠末尾显示3个点
                    white-space: nowrap;
                    overflow: hidden;
                    text-overflow: ellipsis;
                    //
                    text-decoration: none;
                    width: 300px;
                }

                .breadcrumb {
                    margin-top: 4px;
                    color: #555;
                    font-size: 12px;
                    line-height: 14px;
                }
            }
        }

        .download-btn {
            //margin-left: 36px;
            color: #1890ff;
            cursor: pointer;
            &:hover {
                color: #167bd8;
            }
        }

        .del-btn {
            margin-right: 36px; /* 移除左边距 */
            color: #1890ff;
            cursor: pointer;

            &:hover {
                color: #ff592f;
            }
        }
    }

.submit-btn{
    margin-left: 10px;
    margin-top: 10px;
    margin-bottom: 10px;
}

//}
</style>