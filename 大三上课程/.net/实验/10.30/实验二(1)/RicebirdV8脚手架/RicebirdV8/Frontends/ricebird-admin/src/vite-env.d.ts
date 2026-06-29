/// <reference types="vite/client" />
declare module "*.vue" {
    import type { DefineComponent } from 'vue';
    const vueComponent : DefineComponent<{}, {}, any>;
    export default vueComponent;
}

declare module "crypto-js/sha256" {
    function sha256(message : string, cfg: any) : string
    export default sha256;
}

declare module "crypto-js/sha1" {
    function sha1(message : string, cfg: any) : string
    export default sha1;
}
