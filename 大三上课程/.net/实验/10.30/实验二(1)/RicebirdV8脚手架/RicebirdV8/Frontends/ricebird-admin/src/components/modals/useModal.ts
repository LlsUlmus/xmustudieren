import { ref } from 'vue'

export default function useModal(onOpen: Function, processor: Function, onClosing : Function | null) {
    const open = ref(false);
    const loading = ref(false);
    // 这个promise里的两个函数是用来关闭窗口的，无论调用哪个
    let resolve : ((result? : any) => void) = () => { };
    let reject : ((result? : any) => void) = () => { }
    
    function onOk() {
        loading.value = true;
        try {
            let res = processor();
            if (res instanceof Promise) {
                res
                .then(() => { loading.value = false })
                .catch(err => { loading.value = false });
            } else {
                loading.value = false;
            }
        } catch {
            loading.value = false;
        }
    }
    function onCancel() {
        close();
    }
    function close(result? : unknown) {
        loading.value = false;
        open.value = false;
        if (typeof onClosing === "function") {
            onClosing();
        }
        resolve(result);
    }
    function showModal(...para : any[]) {
        let promise = new Promise((res, rej) => {
            resolve = res;
            reject = rej;
        });

        let res = onOpen.apply(null, para);
        if (res instanceof Promise) {
            res.then(() => {
                open.value = true
            })
        } else {
            open.value = true;
        }
        return promise;
    }

    return {
        open, loading, 
        onOk, onCancel,
        close, showModal
    }
}
