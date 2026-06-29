<template>
    <a-popover overlayClassName="popover-schedule-menu">
        <template #content>
            <a-menu :selectable="false" class="menu">
                <a-menu-item v-if="scheduleList.length" v-for="(v, k) in scheduleList.slice(0, 5)" :key="k" @click="showSchedule(v.ID)">
                    {{ v.Name }}
                    <div class="tip-icon">
                        {{ v.Progress }}
                    </div>
                </a-menu-item>
                <a-empty v-else />
                <a-menu-divider />
                <a-menu-item @click="toDetail">
                    查看所有任务
                </a-menu-item>
            </a-menu>
        </template>
        <div class="commander-icon schedule-icon">
            <a-badge :count="activeSchedule" class="icon">
                <a-icon icon="CloudSyncOutlined" />
            </a-badge>
        </div>
    </a-popover>
    <scheduleModal ref="modalRef" />
</template>
<script setup>
import { useScheduleService, scheduleList, activeSchedule, showSchedule } from './schedule-service'
import { ref } from 'vue'
import scheduleModal from './schedule-modal.vue'

let modalRef = ref();
useScheduleService(modalRef);

function toDetail () {
    const url = "/manage/schedules";
    window.open(url);
}
</script>
<style lang="less">
.popover-schedule-menu {
    .ant-menu {
        border-inline-end: 0px !important;
        width: 260px;

        .tip-icon {
            float: right;
            height: 40px;
            line-height: 40px;
        }
    }
    .ant-menu-item:hover {
        background: @nav-header-menu-item-hover;
        color: @text-color !important;
    }

    .ant-empty {
        margin-bottom: 12px;
    }
}
.schedule-icon {
    display: grid;

    .icon {
        font-size: 24px;
        display: block;
        place-self: center;
        color: #000;
    }
}
</style>