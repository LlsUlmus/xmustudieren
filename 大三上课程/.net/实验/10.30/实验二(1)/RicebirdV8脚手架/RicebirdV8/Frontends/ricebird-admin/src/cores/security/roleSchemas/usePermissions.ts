import app from '@/app';
import { createDataSource as createApi } from '@/cores/security/apis/useApi'
import { createDataSource as createMenu } from '@/cores/security/menus/useMenu'
import { ref } from 'vue'

let group = ref("");
let apis = createApi(true);
let menus = createMenu(true);
function apiFilter () {
    apis.query()
        .whereIf("AuthorizeLevel", ref(3))
        .whereIf("Status", ref(1))
        .whereIf("ApiGroup", group)
        .orderBy([ "DisplayOrder" ])
        .end(arr => {
            let result = [];
            let obj : any = {};
            for (let item of arr) {
                obj[item.Module] = obj[item.Module] || [];

                obj[item.Module].push({
                    name: item.Name,
                    label: item.Name
                });
            }
        
            for (let key in obj) {
                let ele = obj[key];
                result.push({
                    module: key,
                    children: ele
                })
            }
            return result;
        })
}

function menuFilter() {
    menus.query()
        .end(arr => {
            let result = [];
            let obj : any = {};
            for (let item of arr) {
                if (item.ParentId === app.GUID_EMPTY) {
                    obj[item.ID] = {
                        module: item.Name,
                        children: []
                    }

                    if (!arr.find(e => e.ParentId === item.ID)) {
                        obj[item.ID].children.push({
                            name: item.ID,
                            label: item.Name
                        });
                    }
                } else {
                    obj[item.ParentId].children.push({
                        name: item.ID,
                        label: item.Name
                    })
                }
            }
        
            for (let key in obj) {
                let ele = obj[key];
                result.push(ele)
            }
            return result;
        })
}

apiFilter();
menuFilter();

export { apis, menus }