import axios from '@/axios'
import { Ref, ref, computed } from 'vue'
import signalR from '@/signalR'
import { message, Modal } from 'ant-design-vue'
import app from '@/app'

interface Schedule {
    ID: string,
    Name: string,
    Duration: string,
    Progress: string,
    Status: number,
    DownloadPath: string,
    ExpiredOn?: string,
    Current: number,
    Total: number,
    Logs: string[],
    Type: string,
    CurrentLog?: string,
    OnCompleted?: ((schedule: Schedule) => {}),
    OnCancel?: ((schedule: Schedule) => {}),
}

let scheduleList : Ref<Schedule[]> = ref([]);
let modalRef : Ref<any> = ref();

let activeSchedule = computed(() => scheduleList.value.filter(e => e.Status === 1).length);

async function useScheduleService(m : Ref<any>) {
    modalRef = m;
}

async function showSchedule(id : string) {
    var entity = scheduleList.value.find(e => e.ID == id);
    if (entity == null) {
        message.warn("找不到这个任务");
    } else {
        if (modalRef.value && modalRef.value.showModal ) {
            modalRef.value.destroy();
            modalRef.value.showModal(entity);
        }
    }
}

async function reloadList(){
    let msg = await axios.post<any, any>("/api/schedules/GetSchedules");    
    scheduleList.value = msg.data;
    scheduleList.value.forEach(v => {
        v.CurrentLog = v.Logs.length ? v.Logs[0] : "";
    });
}
reloadList();

function onScheduleComplete(id : string, callback: ((arg : Schedule) => {})) {
    setTimeout(() => {
        var entity = scheduleList.value.find(e => e.ID == id);
        if (entity) {
            entity.OnCompleted = callback;
            if (entity.Status === 2) {
                // 已经完成
                callback(entity);
            }
        }
    }, 500);
}

function onScheduleCancel(id : string, callback: ((arg : Schedule) => {})) {
    var entity = scheduleList.value.find(e => e.ID == id);
    if (entity) {
        entity.OnCancel = callback;
    }
}

signalR.then(conn => {
    conn.on("ricebird-schedule-create", function ([id, name, type]) {
        let obj : Schedule = {
            ID: id,
            Name: name,
            Duration: "0秒",
            Progress: "0%",
            DownloadPath: "",
            Status: 0, // 等待中
            Current: 0,
            Total: 1,
            Type: type,
            Logs: [],
            CurrentLog: "",
        };
        scheduleList.value.unshift(obj);
        showSchedule(id);
    });

    conn.on("ricebird-schedule-progress", function ([log, id, current, total, duration, progressText]) {        
        var schedule = scheduleList.value.find(e => e.ID == id);
        if (schedule) {
            schedule.Progress = progressText;
            schedule.Duration = duration;
            schedule.Current = current;
            schedule.Total = total;
            schedule.CurrentLog = log;
            schedule.Status = 1; // 进行中
            if (import.meta.env.MODE !== "production") {
                schedule.Logs.unshift(`[调试]${log}`);
            }
        }
    })

    conn.on("ricebird-schedule-log", function ([log, id, current, total, duration, progressText]) {
        var schedule = scheduleList.value.find(e => e.ID == id);
        if (schedule) {
            schedule.Progress = progressText;
            schedule.Duration = duration;
            schedule.Current = current;
            schedule.Total = total;
            schedule.CurrentLog = log;
            schedule.Logs.unshift(log);
            schedule.Status = 1; // 进行中
        }
    })

    conn.on("ricebird-schedule-complete", function ([log, id, download, duration, progressText]) {
        var schedule = scheduleList.value.find(e => e.ID == id);
        if (schedule) {
            schedule.Progress = progressText;
            schedule.Duration = duration;
            schedule.CurrentLog = log;
            schedule.Current = schedule.Total;
            schedule.Logs.unshift(log);
            schedule.DownloadPath = download;
            schedule.Status = 2; // 已完成

            schedule.OnCompleted && schedule.OnCompleted(schedule);
        }
    })

    conn.on("ricebird-schedule-cancel", function ([log, id, duration, progressText]) {
        var schedule = scheduleList.value.find(e => e.ID == id);
        if (schedule) {
            console.log("cancel", log, id, duration, progressText )
            schedule.Progress = progressText;
            schedule.Duration = duration;
            schedule.CurrentLog = log;
            schedule.Current = schedule.Total;
            schedule.Logs.unshift(log);
            schedule.DownloadPath = "";
            schedule.Status = 3; // 已取消

            schedule.OnCancel && schedule.OnCancel(schedule);
        }
    })

    conn.on("ricebird-schedule-clear", function ([ id ]) {
        var index = scheduleList.value.findIndex(e => e.ID == id);
        if (index > -1) {
            scheduleList.value.splice(index, 1);
            modalRef.value.onClear(id);
        }
    })

    conn.on("ricebird-schedule-debug", function ([ log ]) {
        // var index = scheduleList.value.findIndex(e => e.ID == id);
        console.log("debug: ", log);
    })
});

export { reloadList, scheduleList, useScheduleService, showSchedule, activeSchedule, onScheduleComplete, onScheduleCancel }