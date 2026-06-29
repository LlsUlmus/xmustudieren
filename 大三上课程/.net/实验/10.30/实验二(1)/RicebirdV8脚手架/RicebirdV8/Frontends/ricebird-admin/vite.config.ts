import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import path from "path";
import { manualChunksPlugin } from 'vite-plugin-webpackchunkname';

function resolve(dir: string): string {
    return path.join(__dirname, dir);
}

const env = process.env.NODE_ENV || "development";
const isProd = env === "production";

const useRemote = false;
const useHttps = useRemote ? false : true;
const proxyRemote = useRemote ? "远端地址，不需要http打头" : "localhost:7113";

// https://vitejs.dev/config/
export default defineConfig({
    base: ["production", "staging"].indexOf(env) > -1 ? "/manage/" : "/",
    plugins: [ vue() ],
    build: {
        target: ["edge90", "chrome90", "firefox90", "safari15"], // 2021年以后的浏览器均可使用
        minify: "terser",
        terserOptions: {
            compress: {
                drop_console: true,
                drop_debugger: true,
            },
        },
        rollupOptions: {
            output: {
                chunkFileNames: 'assets/[name]-[hash].js',
                assetFileNames: 'assets/[name]-[hash].[ext]',
            }
        }
    },    
    css: {
        preprocessorOptions: {
            less: {
                charset: false,
                additionalData: '@import "@/assets/mixin.less";',
            },
        },
    },
    resolve: {
        alias: [
            {
                find: "@",
                replacement: resolve("src"),
            },
            {
                find: "@vc",
                replacement: resolve("src/cores"),
            },
            {
                find: "@vm",
                replacement: resolve("src/modules"),
            },
            {
                find: "@layouts",
                replacement: resolve("src/layouts"),
            },
            {
                find: "@proj",
                replacement: resolve("src/modules/projects"),
            },
            {
                find: "@prog",
                replacement: resolve("src/modules/progress"),
            },
        ],
    },
    server: {
        proxy: {
            "/api": {
                target: `${useHttps ? "https" : "http"}://${proxyRemote}`,
                changeOrigin: true,
                secure: false,
            },
            "/signalr": {
                target: `${useHttps ? "wss" : "ws"}://${proxyRemote}`,
                changeOrigin: true,
                ws: true,
                secure: false,
            },
            "/debug": {
                target: `${useHttps ? "https" : "http"}://${proxyRemote}`,
                changeOrigin: true,
                secure: false,
            },
            "/permanent": {
                target: `${useHttps ? "https" : "http"}://${proxyRemote}`,
                changeOrigin: true,
                secure: false,
            },
        },
    },
});
