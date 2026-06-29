
class FileDict {
    private static baseUrl: string = "https://cxw.xmu.edu.cn/templates/";
    private static filenameList: string[] = [
        "workflow.png",
        "个人竞赛PPT.ppt",
        "校级竞赛展示.ppt",
        "校外竞赛展示.ppt",
        "专利模板.ppt"
    ];
    private static fileDict: Map<string, string> = new Map<string, string>();

    static initialize() {
        for (const filename of this.filenameList) {
            this.fileDict.set(filename, `${this.baseUrl}${filename}`);
        }
    }


    static getFile(fileName: string): string | undefined {
        return this.fileDict.get(fileName);
    }

    static setFile(fileName: string, filePath: string): void {
        this.fileDict.set(fileName, filePath);
    }
}

// 在类外部调用 initialize 方法来初始化 fileDict
FileDict.initialize();

export  {FileDict};