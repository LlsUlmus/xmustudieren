<template>
    <div v-if="props.readonly">{{ dayjsValue.format && dayjsValue.format(props.format) }}</div>
    <a-date-picker v-model:value="dayjsValue" v-bind="$attrs" style="width: 100%" :format="props.format" v-else>
        <template #suffixIcon>
            <slot v-if="props.withIcon" name="suffixIcon">
                <CalendarOutlined />
            </slot>
            <span v-else></span>
        </template>
    </a-date-picker>
</template>
<script setup>
import { computed } from 'vue';
import dayjs from 'dayjs';
import app from '@/app';

const props = defineProps({
    format: {
        type: String,
        default: 'YYYY-MM-DD'
    },
    withIcon: {
        type: Boolean,
        default: true
    },
    readonly: {
        type: Boolean,
        default: false
    }
});
const date = defineModel('value', {
    type: String,
    default: ''
});

initDate();

const dayjsValue = computed({
    get: () => {
        return isDateEmpty(date.value) ? '' : dayjs(date.value, props.format);
    },
    set: (newDate) => {
        date.value = isDateEmpty(newDate) ? app.DATE_EMPTY : newDate.format(props.format);
    }
});

function initDate () {
    if (isDateEmpty(date.value) || !isDateValid(date.value)) {
        date.value = app.DATE_EMPTY;
    }
    if (date.value.includes && date.value.includes('年')) {
        const { year, month, day } = date.value.match(/(?<year>\d+)年(?<month>\d+)月(?<day>\d+)日/).groups;
        date.value = `${year}-${month}-${day}`;
    }
}

function isDateEmpty(date) {
    return !date || dayjs(date).isSame(dayjs(app.DATE_EMPTY)) || date === '';
}

function isDateValid(date) {
    return dayjs(date, props.format).isValid();
}
</script>