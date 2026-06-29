import toText from '@/cores/configurations/data-dictionary/useDictionary';
import app from '@/app';
export default {
    install() {
        // 这里的app指的是app.ts里的对象，而正常这个文件里的app指的是vue对象
        app.toText = toText;
    }
}