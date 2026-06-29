import { ref, reactive, watch } from 'vue'

let clientHeight = ref(document.documentElement.clientHeight);
let defaultTabHeight = 49;
const viewport = reactive({ 
    height: clientHeight, // 页面可见区高
    width: document.documentElement.clientWidth, // 页面可见共宽
    defaultTabHeight, // 默认的距离顶部的距离
    fullScreenUEditorHeight: 0
});

function setViewport () {
    clientHeight.value = document.documentElement.clientHeight;
    viewport.width = document.documentElement.clientWidth;
    viewport.fullScreenUEditorHeight = clientHeight.value - defaultTabHeight - 59 - 22 - 16 - 16; // 59是UE自己的编辑器头高，两个16是margin-top, bottom
}

window.addEventListener("resize", () => {
    setViewport();
});

setViewport();

export default viewport;
