
declare type ResolveParameter<T> = T | PromiseLike<T>;
declare type ResolveFunc<T> = (result: ResolveParameter<T>) => void;
declare type RejectFunc = (reason : any) => void;
declare interface resolveResult<T> {
    promise: Promise<T>,
    resolve: ResolveFunc<T>,
    reject: RejectFunc
}

function withResolvers<T> () : resolveResult<T> { 
    let resolve : ResolveFunc<T> = res => {};
    let reject : RejectFunc = (rej : any) => {};
    let promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });

    return {
        promise,
        resolve,
        reject
    }
}

export type { resolveResult }
export default withResolvers;