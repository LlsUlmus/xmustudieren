import app from "@/app";
import toText from './useDictionary'

export async function setAppFunction () {
    app.toText = toText;
}