export default function fromatFromMSWord(div, rtf) {
    let images = findAllImages(div);

    let rtfImages = [];
    if (rtf) {
        rtfImages = extractImageDataFromRtf(rtf);
    }

    let j = 0;
    for (let i = 0; i < images.length; i++) {
        let img = images[i];
        const src = img.getAttribute("src");
        if (!src.startsWith('file://')) {
            continue;
        }

        if (rtfImages.length <= j) {
            alert("无法从剪切板中加载图片，请联系管理员处理。联系时，请附上有问题的Word文档及浏览器名称版本。\n注：IE系列浏览器无法使用此功能，需要改用Edge浏览器。");
            return;
        }

        let imgSrc = rtfImages[j].imgSrc;
        j++;
        img.setAttribute("src", imgSrc)
    }
}

function findAllImages(div) {
    const domUtils = UE.dom.domUtils;
    const utils = UE.utils;
    const imgs = domUtils.getElementsByTagName(div, "img");
    // const imgs = domUtils.getElementsByTagName(div, "v-shape");
    return imgs;
}

function extractImageDataFromRtf(rtfData) {
    if (!rtfData) {
        return [];
    }

    const regexPictureHeader = /{\\pict[\s\S]+?\\bliptag-?\d+(\\blipupi-?\d+)?({\\\*\\blipuid\s?[\da-fA-F]+)?[\s}]*?/;
    const regexPicture = new RegExp('(?:(' + regexPictureHeader.source + '))([\\da-fA-F\\s]+)\\}', 'g');
    const images = rtfData.match(regexPicture);
    const result = [];

    if (images) {
        for (const image of images) {
            let imageType = false;

            if (image.includes('\\pngblip')) {
                imageType = 'image/png';
            } else if (image.includes('\\jpegblip')) {
                imageType = 'image/jpeg';
            }

            if (imageType) {
                let obj = {
                    base64: toBase64(image.replace(regexPictureHeader, '').replace(/[^\da-fA-F]/g, '')),
                    type: imageType,
                };
                obj.imgSrc =`data:${obj.type};base64,${obj.base64}`;
                result.push(obj);
            }
        }
    }

    return result;
}

export function toBase64(hexString) {
    return btoa(hexString.match(/\w{2}/g).map(char => {
        return String.fromCharCode(parseInt(char, 16));
    }).join(''));
}