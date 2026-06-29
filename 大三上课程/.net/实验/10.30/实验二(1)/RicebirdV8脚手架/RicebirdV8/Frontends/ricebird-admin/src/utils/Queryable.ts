import type { Queryable } from './utils'
import lo from 'lodash'

interface FuncLink<T> {
    func : ((chain : lo.Collection<T>, desc : FuncLink<T>) => lo.Collection<T>) | string,
    params : any[]
}

function where<T> (chain : lo.Collection<T>, desc : FuncLink<T>) : lo.Collection<T> {
    chain = (chain.filter as Function).apply(chain, desc.params)
    return chain;
}

function whereIf<T> (chain : lo.Collection<T>, desc : FuncLink<T>) : lo.Collection<T> {
    let field = desc.params[0];
    let search = desc.params[1];
    const defaultValue = (typeof search.value === 'number' ? -1 : "");
    let empty : (string | number) = desc.params.length >= 3 ? desc.params[2] : defaultValue;

    if (search.value !== empty) {
        chain = (chain.filter as Function).call(chain, (e : any) => {
            return (e[field] && e[field].indexOf) ? e[field].indexOf(search.value) >= 0 : e[field] === search.value;
        });
    }

    return chain;
}

function orderBy<T> (chain : lo.Collection<T>, desc : FuncLink<T>) : lo.Collection<T> {
    chain = (chain.orderBy as Function).apply(chain, desc.params)
    return chain;
}

function map<T> (chain: lo.Collection<T>, desc : FuncLink<T>) : lo.Collection<T> {
    chain = (chain.map as Function).apply(chain, desc.params)
    return chain;
}

export default function createQuery<T> (source : T[], complete : (data : Array<T>, merge? : boolean) => void) : Queryable<T> {
    let array : T[] = source;
    let funcLinks : FuncLink<T>[] = [];
    let query : Queryable<T> = {
        restart () {
            funcLinks = [];
        },
        where (predicate) {
            funcLinks.push({
                func: where, 
                params: [predicate]
            })
            return this;
        },
        whereIf (field, search, non) {
            funcLinks.push({
                func: whereIf, 
                params: [ field, search, non ]
            })
            return this;
        },
        orderBy (fields, orders) {
            funcLinks.push({
                func: orderBy, 
                params: [ fields, orders ]
            })
            return this;
        },
        map (iterator) {
            funcLinks.push({
                func: map, 
                params: [iterator]
            })
            return this;
        },
        end (callback) {
            funcLinks.push({
                func: "end", 
                params: [ callback ]
            })
            return this.execute(array)
        },
        execute (data) {
            array = data || [];
            let result = array;
            let chain = lo(array);

            for (let link of funcLinks) {
                if (typeof link.func !== 'string') {
                    chain = link.func(chain, link);
                } else if (link.func === 'end') {
                    result = chain.value();
                    if (link.params.length === 1 && typeof link.params[0] === 'function') {
                        let callback = link.params[0] as unknown as ((arr : T[]) => any);
                        result = callback(result);
                        // chain = chain.map(selector);
                    }
                }
            }
            
            complete(result);
            return result;
        }
    };

    return query;
}