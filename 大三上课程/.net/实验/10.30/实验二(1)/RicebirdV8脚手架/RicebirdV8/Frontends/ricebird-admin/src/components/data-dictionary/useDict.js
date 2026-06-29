import { h, ref, watchEffect } from 'vue'
import { ensureDictionary } from '@/cores/configurations/data-dictionary/useDictionary'
import lo from 'lodash'

export default function useDictionary(props, searcher) {
    const dict = ref([]);
    watchEffect(() => {
        if (typeof props.dict === 'string') {
            getDict(props.dict, props.prefix, props.remove, searcher);
        } else {
            getDictEntry(props.dict, props.prefix, props.remove, searcher);
        }
    });

    async function getDict(v, prefix, remove) {
        let d = await ensureDictionary(v);
        getDictEntry(d, prefix, remove, searcher)
    }

    function getDictEntry(d, prefix, remove) {
        let result = d ? d.entries : [];
        let showPrefix = !!prefix;
        if (remove !== 0 && !remove) {
            remove = [];
        } else if (!Array.isArray(remove)) {
            remove = [ remove.toString() ];
        }

        let hasGroup = false;
        
        let arr = lo(result)
                    .filter(e => remove.findIndex(x => x.toString() === e.DataKey.toString() || x.toString() === e.DataValue.toString()) === -1)
                    .filter(e => !searcher || searcher.value === "" || (e.DataValue.indexOf(searcher.value) > -1))
                    .filter(e => typeof e.Visible === 'undefined' || e.Visible)
                    .filter(e=> !props.filterFunc ? true : props.filterFunc(e))
                    .map(e => {                        
                        let selected = e.DataValue;
                        if (showPrefix) {
                            let name = typeof prefix === 'string' ? prefix : d.name;
                            let valueStyle = undefined;
                            if (typeof props.valueWidth) {
                                valueStyle = { width: `${props.valueWidth}em` }
                            }
                            selected = [h('span', { class: { ["key"]: true } }, [`${name}：`]), h('span', { class: { ["value"]: true }, style: valueStyle }, [e.DataValue])];
                        }
                        let dataKey = e.DataKey;
                        if (d.type === 'number') dataKey = parseInt(e.DataKey);
                        hasGroup = hasGroup || !!e.Group;
                        return {
                            value: dataKey,
                            label: e.label || e.DataValue,
                            disabled: typeof e.Enable !== 'undefined' && !e.Enable,
                            Group: e.Group,
                            selected
                        }
                    })
                    .value();
        
        if (hasGroup) {
            let final = [];
            for (let item of arr) {
                if (!item.Group) {
                    final.push(item);
                    continue;
                }
                var group = final.find(e => e.label === item.Group);
                if (group) {
                    group.options.push(item);
                } else {
                    final.push({
                        label: item.Group,
                        options: [ item ]
                    });
                }
            }
            arr = final;
        }

        dict.value = arr;
    }

    return {
        dict,
    }
}