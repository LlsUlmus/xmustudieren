// 此文件在app.ts前初始化，不允许使用app.ts，axios.js
import sha256 from 'crypto-js/sha256';
import sha1 from 'crypto-js/sha1';

function hash256 (message : string) : string {
    var sha = sha256(message, {});
    return sha.toString();
}

function hash1 (message : string) : string {
    var sha = sha1(message, {});
    return sha.toString();
}

export {
    hash256 as sha256,
    hash1 as sha1,
};