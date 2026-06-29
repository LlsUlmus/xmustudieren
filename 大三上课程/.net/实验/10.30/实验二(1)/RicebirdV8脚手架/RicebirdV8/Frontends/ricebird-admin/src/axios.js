import axios from 'axios'
import { accessToken } from './cores/security/useCurrentUser';
import notification from 'ant-design-vue/es/notification';
import dayjs from 'dayjs';

const maxTimeout = import.meta.env.MODE === "production" ? 6000 : 99999999;
const request = axios.create({
    timeout: maxTimeout
});

// 异常拦截处理器
const errorHandler = (error) => {
    if (error.response) {
        const data = error.response.data
        if (error.response.status === 401) {
            notification.error({
                message: "权限验证不通过",
                description: data.msg
            })
        }

        if (error.response.status === 403) {
            notification.error({
                message: "禁止访问此接口",
                description: data.msg
            })
        }

        if (error.response.status === 500) {
            notification.error({
                message: '服务器不见了！',
                description: `服务器出现了一些问题，请联系管理员解决，并且附上现在的时间：${dayjs().format("YYYY-M-D H:mm:ss")}。`
            });
            return Promise.reject(`服务器出现了一些问题，请联系管理员解决，并且附上现在的时间：${dayjs().format("YYYY-M-D H:mm:ss")}。`)
        }
    }

    return Promise.reject(error)
}

request.interceptors.request.use(req => {
    // 从 localstorage 获取 token
    try {
        const { requestKey, token } = accessToken;
        if (token) {
            req.headers[requestKey] = token;
        }
    }
    catch { }
    return req;
}, errorHandler);

request.interceptors.response.use(res => {
    return res.data
}, errorHandler);

export default request;
export {
    request as axios
};