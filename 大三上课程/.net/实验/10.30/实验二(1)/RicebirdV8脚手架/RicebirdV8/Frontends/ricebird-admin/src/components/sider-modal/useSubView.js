import { ref, watch, inject, } from "vue";

export default function useSubView (props) {
    let sub = inject("subItems");
    if (sub.value.findIndex(e => e.title == props.title) === -1) {
        sub.value.push(props);
    }

    // -- 选中页面切换 -- //
    let activeKey = inject("activeKey");
    let activeState = ref(false);
    watch(activeKey, _ => {
        resetActiveState();
    });
    function resetActiveState() {
        activeState.value = (activeKey.value === props.title);
    }
    resetActiveState();

    return { activeState, };
}