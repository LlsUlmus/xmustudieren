import axios from '@/axios';
import fromatFromMSWord from './FromatFromMSWord';

function analysizeBase64(str) {
    // data:image/gif;base64,
    let pattern = /^data:image\/(?<format>[a-zA-Z-]+);base64,(?<base64>.*)/;
    let match = pattern.exec(str);
    let format = match.groups["format"];
    return {
        format,
        base64: match.groups["base64"],
        fileName: `clipboard.${format}`,
    }
}

function addTempFileInput (src, img) {
}

function analysizeFilePath(str, img) {
    if (!addTempFileInput(str, img)) {
        return {
            file: false,
            bytes: false
        };
    }

    const fr = new FileReader();
}

export default function UEditorUploadHelper(ueditor) {
    const methods = {
        ueditor,
        async uploadBase64Image (src) {
            let result = analysizeBase64(src);

            let msg = await axios.post("/api/cms/upload/Base64", { file: result.fileName, base64: result.base64 });
            // console.log(msg);
            if (msg.success) {
                return {
                    download: msg.download,
                    id: msg.id
                };
            } else {
                // 这里要放一个上传失败的图
                return null;
            }
        },
        async uploadFileImage (src, img) {
            let result = analysizeFilePath(src, img);
            if (result.file) return null;

            // let msg = await axios.post("/api/cms/upload/Base64", { file: result.fileName, base64: result.base64 });
            // // console.log(msg);
            // if (msg.success) {
            //     return msg.download;
            // } else {
            //     // 这里要放一个上传失败的图
            //     return null;
            // }
        },
        async uploadAllImages () {
            const nopic = `${ueditor.options.UEDITOR_HOME_URL}lang/zh-cn/images/figure-nopic.png`;
            const domUtils = UE.dom.domUtils;
            const utils = UE.utils;
            const imgs = domUtils.getElementsByTagName(ueditor.document, "img");
            let hasChange = false;
            for (let img of imgs) {
                // console.log(img.removeAttribute, img.getAttribute, img);
                const src = img.getAttribute("word_img");
                const method = img.getAttribute("method");
                const serverId = img.getAttribute("serverId")
                if (!!methods[method] && !serverId) {
                    let msg = await methods[method].call(null, src, img);
                    let serverPath = msg && msg.download;
                    if (serverPath) {
                        img.setAttribute("src", serverPath);
                        img.setAttribute("serverId", msg.id);
                        hasChange = true;
                        domUtils.removeAttributes(img, ['style']);
                    } else {
                        img.setAttribute("style", `background:url(${nopic}) no-repeat center center;border:1px solid #ddd`);
                    }
                } else {
                    img.setAttribute("style", `background:url(${nopic}) no-repeat center center;border:1px solid #ddd`);
                }
                domUtils.removeAttributes(img, ['word_img', 'method', '_src']);
            }

            if (hasChange) { 
                ueditor.fireEvent("selectionchange");
            }
        }
    };

    ueditor.addListener('afterprocesshtml', async function () {
        await methods.uploadAllImages();
    });

    ueditor.addListener('contentpaste', function (event, div, data) {
        fromatFromMSWord(div, data);
    });

    return methods;
}