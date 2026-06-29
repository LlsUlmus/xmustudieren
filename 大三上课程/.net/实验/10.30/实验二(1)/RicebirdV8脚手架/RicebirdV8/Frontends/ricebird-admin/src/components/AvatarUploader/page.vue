<template>
    <div class="avatar-uploader">
        <label class="shower" :style="style" for="uploader">
            <a-icon class="icon" icon="camera-outlined" :style="{ fontSize: iconSize + 'px', height: iconSize + 'px' }"></a-icon>
        </label>
        <input type="file" id="uploader" ref="uploader" @change="onImageChanged($event, 1)" accept="image/png,image/jpeg,image/gif,image/jpg" />
        <p v-if="text">{{ text }}</p>
        <p v-else-if="showText">点击上传png, jpg, jpeg格式的图片，推荐尺寸为{{width}}px &times; {{height}}px。</p>
        <a-modal title="裁剪图片" v-model:open="showDialog" width="600px" class="clip-dialog" :mask-closable="false">
            <VueCropper ref="cropper" class="cropper" :high="true" :img="image" :autoCrop="true" :centerBox="true"
             :autoCropWidth="width" :autoCropHeight="height" :fixed="true" :fixedBox="fixedBox"
             :fixedNumber="[width, height]" />
             <template #footer>
                <div >
                    <a-button type="primary" :loading="isloading" @click="confirm()" >确认</a-button>
                    <a-button @click="showDialog=false" >取消</a-button>
                </div>
             </template>
        </a-modal>
    </div>
</template>

<script>
import 'vue-cropper/dist/index.css'
import { VueCropper } from 'vue-cropper';
import axios from "@/axios";
import { message } from 'ant-design-vue';
import nopic from '@/assets/images/no-pic.jpg';

export default {
    components: { VueCropper },
    methods: {
        confirm () {
            let cropper = this.$refs.cropper;
            this.isloading = true;
            cropper.getCropData(data => {
                data = data.substring(data.indexOf(',') + 1)
                this.uploadImage(data);
            });
        },
        onImageChanged (e, num) {
            var file = e.target.files[0];
            if (!/\.(gif|jpg|jpeg|png|bmp|GIF|JPG|PNG)$/.test(e.target.value)) {
                message.warning("图片类型必须是.gif,jpeg,jpg,png,bmp中的一种");
                return false;
            }
            var reader = new FileReader();
            let base64 = function (buffer) {
                var binary = '';
                var bytes = new Uint8Array(buffer);
                var len = bytes.byteLength;
                for (var i = 0; i < len; i++) {
                    binary += String.fromCharCode(bytes[i]);
                }
                return window.btoa(binary);
            }
            reader.onload = e => {
                let data;
                this.$refs.uploader.value = "";
                if (typeof e.target.result === "object") {
                    // 把Array Buffer转化为blob 如果是base64不需要
                    data = window.URL.createObjectURL(new Blob([e.target.result]));
                } else {
                    data = e.target.result;
                }
                if (this.needCrop) {
                    this.image = data;
                    this.showDialog = true;
                } else {
                    let code = base64(e.target.result);
                    this.uploadImage(code);
                }
            };
            reader.readAsArrayBuffer(file);
        },
        uploadImage (base64Code) {
            let post = axios.post(this.action, {
                file: "avatar.jpg",
                usage: this.usage,
                table: this.relateTable,
                id: this.id,
                single: this.single,
                module: this.module,
                base64: base64Code
            });
            post.then(msg => {
                if (!msg.success) {
                    this.isloading = false;
                    this.showDialog = false;
                    message.warning(msg.msg);
                } else {
                    this.isloading = false;
                    this.showDialog = false;
                    message.success("上传成功");
                    this.style.backgroundImage = `url(${msg.download})`;
                    this.$emit("input", msg.download);
                    this.$emit("update:value", msg.download);
                }
            });
        },
        setValue (v) {
            let size = 'cover';
            let img = v;
            if (!v) {
                size = 'contain';
                img = this.nopic;
            }
            this.style.backgroundImage = `url(${img})`;
            this.style.backgroundSize = size;
        }
    },
    props: {
        width: {
            type: Number,
            required: true
        },
        height: {
            type: Number,
            required: true
        },
        displayWidth: {
            type: Number,
            required: false
        },
        displayHeight: {
            type: Number,
            required: false
        },
        action: {
            type: String,
            default: "/api/cms/upload/Base64"
        },
        relateTable: String,
        usage: {
            type: String,
            required: true
        },
        module: {
            type: String,
            default: "Cms",
        },
        value: String,
        fixedBox: Boolean,
        id: String,
        single: Boolean,
        needCrop: {
            type: Boolean,
            default: true
        },
        showText: {
            type: Boolean,
            default: true
        },
        text: {
            type: String
        }
    },
    computed: {
        iconSize: {
            get () {
                let height = this.displayHeight || this.height;
                let width = this.displayWidth || this.width;
                let size = height > width ? width : height;
                return size / 4;
            }
        }
    },
    data () {
        let data = {
            showDialog: false,
            image: "",
            isloading: false,
            params: {
                relateTable: this.relateTable,
                usage: this.usage ? this.usage : "imgAvatar",
                single: this.single
            },
            style: {
                width: (this.displayWidth || this.width) + 'px',
                height: (this.displayHeight || this.height) + 'px',
                lineHeight: (this.displayHeight || this.height) + 'px',
                "background-size": "cover",
                backgroundImage: ""
            },
            pic: this.value,
            nopic: nopic,
        };
        return data;
    },
    watch: {
        value (v) {
            this.setValue(v);
        }
    },
    mounted () {
        this.setValue(this.pic);
    }
}
</script>

<style lang="less">
@import '@/assets/less/xmu-themes.less';
.clip-dialog {
    .ant-modal-body {
        height: 300px;
    }
}
.avatar-uploader {
    .shower {
        display: block;
        background-size: contain;
        background-position: 50% 50%;
        background-repeat: no-repeat !important;
        border: 1px dashed @border-color-base;
        border-radius: @border-radius-base;
        text-align: center;
        cursor: pointer;
        position: relative;
        overflow: hidden;
        transition: border-color 1s ease;
        .icon {
            opacity: 0;
            transition: opacity 1s ease;
            position: absolute;
            left: 0;
            right: 0;
            top: 0;
            bottom: 0;
            margin: auto;
            display: block;
        }
        &:hover {
            border: 1px dashed @primary-color;
            .icon {
                opacity: 1;
            }
        }
    }

    input[type='file'] {
        width: 0px;
        height: 0px;
        position: absolute;
        overflow: hidden;
    }
}
</style>
