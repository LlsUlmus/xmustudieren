<template>
    <span>{{ depart ? depart.title : "" }}</span>
</template>

<script setup>
let { getDepart } = await import('./useDepartment');
const { id } = defineProps({
    id: {
        type: String,
        required: true
    }
});
const depart = getDepart(id);
</script>