import { Ref } from 'vue'

interface Data {
    [prop: string]: any
}
interface TreeNode extends Data {
    children?: []
}

type Many<T> = T | readonly T[];
type PropertyName = string | number | symbol;
type IterateeShorthand<T> = PropertyName | [PropertyName, any] | PartialShallow<T>;
type PartialShallow<T> = {
    [P in keyof T]?: T[P] extends object ? object : T[P]
};
type PredicateFunc<T> = (ele : T) => boolean;
type Predicate<T> = PredicateFunc<T> | PartialShallow<T> | IterateeShorthand<T>;

interface Queryable<T> {
    restart: () => void,
    // source: Array<T>,
    /** 根据条件查询，条件为 lodash 的写法。此处的查询不会访问远程端口
     * @param { Predicate<T> } predicate : 查询条件
     * @returns { Queryable<T> } 链式查询
     */
    where: (predicate : Predicate<T>) => Queryable<T>,
    /** 
     * 此函数相当于 predicate (e => search.value == non || e[field].contains(search.value))
     * @param { string } field : 用以判断的字段
     * @param { Ref<string | number> } search : 待查询的内容
     * @param { any? } non : 如何判断内容为空，如果不填写，对于 string型数据，默认为""。对于number型数据，默认为-1
     * @returns { Queryable<T> } 链式查询
     */
    whereIf: (field : string, search : Ref<string | number>, non? : any) => Queryable<T>,
    orderBy: (fields: string[], orders?: Many<boolean|"asc"|"desc">) => Queryable<T>,
    map: (iterator : ((value : T, index : number, collection : T[]) => any)) => Queryable<T>,
    end: (callback? : ((result : Array<T>) => Array<T>)) => void,
    execute: (data : T[]) => T[]
}

/** 用以绑定的数据源 */
interface DataSource<T> {
    id: number,
    /** 用以绑定的数据 */
    data: Array<T>,
    /** 查询：用法
     * query().where().whereIf().end()
     * 在查询完成后，必须要写 end!
     * @param { shouldMerge : boolean } 当设置为true时，返回已展开的树
     */
    query: () => Queryable<T>,
    ver: number,
}

export {
    Data,
    TreeNode,
    DataSource,
    Queryable
}