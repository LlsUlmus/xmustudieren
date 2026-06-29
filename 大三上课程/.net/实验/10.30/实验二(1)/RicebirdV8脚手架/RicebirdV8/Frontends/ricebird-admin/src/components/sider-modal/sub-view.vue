<template>
    <div class="modal-sub-view" v-show="activeState">
        <slot>
        </slot>
    </div>
</template>

<script setup>
import useSubView from './useSubView'

const props = defineProps({
    icon: String,
    title: {
        type: String,
        required: true,
    },
    disabled: {
        type: Boolean,
        default: false
    },
    extraIcon: {
        type: String,
        default: ""
    }
});

let { activeState, } = useSubView(props);
</script>