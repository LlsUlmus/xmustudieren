import app from "@/app.ts";
import axios from '@/axios'

export class Articles {
    ID: string;
    DisplayOrder: number;
    StuCode: string;
    StuName: string;
    CollageName: string;
    DepartId: string;
    DepartName: string;
    Major: string;
    ReleaseTime: string;
    Telephone: string;
    Achieved: string;
    Journal: string;
    PageNum: string;
    AuthorSort: string;
    GuideTeacherCode: string;
    GuideTeacher: string;
    GuideTeacherId: string;
    GuideTeacherIsFirstAuthor: string;
    ProTime: string;
    FundType: string;
    JournalLevel: string;
    OtherLevel: string;
    Effect: string;
    Captures: string;
    ArticleUrl: string;
    ApplyYear: string;
    Batch: string;
    DeleteMark: number;
    Status: number;
    Score: string;
    RelateId: string;
    GuideTeacherOpinion: string;
    GuideTeacherSignature: string;
    GTSignatureTime: string;
    DepartmentOpinion: string;
    DepartmentSignature: string;
    DepartmentSignatureTime: string;
    // 论文原文: string;
    // 论文检索证明: string;
    "Attachments": Attachments[] = []

    constructor() {
        this.ID = app.GUID_EMPTY;
        this.StuCode = "";
        this.DisplayOrder = 0;
        this.StuName = "";
        this.CollageName = "";
        this.DepartId = "";
        this.DepartName = "";
        this.Major = "";
        this.ReleaseTime = "";
        this.Telephone = "";
        this.Achieved = "";
        this.Journal = "";
        this.PageNum = "";
        this.AuthorSort = "";
        this.GuideTeacherCode = "";
        this.GuideTeacher = "";
        this.GuideTeacherId = "";
        this.GuideTeacherIsFirstAuthor = "";
        this.ProTime = "";
        this.FundType = "";
        this.JournalLevel = "";
        this.OtherLevel = "";
        this.Effect = "";
        this.Captures = "";
        this.ArticleUrl = "";
        this.ApplyYear = "";
        this.Batch = "";
        this.DeleteMark = 0;
        this.Status = 0 ;
        this.Score = "";
        this.RelateId = "";
        this.GuideTeacherOpinion = "";
        this.GuideTeacherSignature = "";
        this.GTSignatureTime = "";
        this.DepartmentOpinion = "";
        this.DepartmentSignature = "";
        this.DepartmentSignatureTime = "";
    }
    static getEmptyArticle(): Articles {
        return new Articles();
    }
}
class Attachments {

}
export default Articles;
