// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
export interface WebConfig {
    name: string,
    requestKey: string,
}

const webConfig : WebConfig = {
    name: "米雀管理系统",
    requestKey: "access_token",
}

export default webConfig;