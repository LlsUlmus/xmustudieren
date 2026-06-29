
using Ricebird.Framework.FileStorage;

namespace UEditor.Services
{
    /// <summary>
    /// UploadHandler 的摘要说明
    /// </summary>
    public class UploadHandler : UEditorHandler
    {

        public UploadConfig UploadConfig { get; protected set; }
        public UploadResult Result { get; protected set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public UploadHandler(IClient workContext)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            : base(workContext)
        {
            //this.UploadConfig = config;
            this.Result = new UploadResult() { State = UploadState.Unknown };
        }

        public void Execute()
        {
            if (Client.Request == null)
            {
                return;
            }

            byte[] uploadFileBytes;
            string uploadFileName;

            if (UploadConfig.Base64)
            {
                uploadFileName = UploadConfig.Base64Filename;

                uploadFileBytes = Convert.FromBase64String(Client.Get(UploadConfig.UploadFieldName, string.Empty));
            }
            else
            {
                //var file = WorkContext.Request.Files[UploadConfig.UploadFieldName];
                var file = Client.Request.Form.Files[UploadConfig.UploadFieldName];
                if (file == null)
                {
                    return;
                }

                uploadFileName = file.FileName;
                if (!CheckFileType(uploadFileName))
                {
                    Result.State = UploadState.TypeNotAllow;
                    WriteResult();
                    return;
                }

                int length = (int)file.Length;
                if (!CheckFileSize(length))
                {
                    Result.State = UploadState.SizeLimitExceed;
                    WriteResult();
                    return;
                }

                uploadFileBytes = new byte[file.Length];
                try
                {
                    file.OpenReadStream().Read(uploadFileBytes, 0, length);
                }
                catch (Exception)
                {
                    Result.State = UploadState.NetworkError;
                    WriteResult();
                    return;
                }
            }

            Result.OriginFileName = uploadFileName;

            //var savePath = PathFormatter.Format(uploadFileName, UploadConfig.PathFormat);

            try
            {
                IFileStorageService am = Client.Resolve<IFileStorageService>() ?? throw new ArgumentNullException("attachmentService", "找不到对应的附件管理服务。");
                var (msg, file) = am.CreateFile(uploadFileBytes, uploadFileName, MODULE_NAME, Client);
                if (file == null)
                {
                    Result.State = UploadState.NotSupported;
                    return;
                }

                Result.Url = file.DownloadPath;
                Result.State = UploadState.Success;
                //SmsService.SendNotification(WorkContext, "保存成功", "", MsgImportance.Success);
            }
            catch (Exception e)
            {
                Result.State = UploadState.FileAccessError;
                Result.ErrorMessage = e.Message + "\n" + e.StackTrace;
            }
            finally
            {
                WriteResult();
            }
        }

        private object WriteResult()
        {
            return new
            {
                state = GetStateMessage(Result.State),
                url = Result.Url,
                title = Result.OriginFileName,
                original = Result.OriginFileName,
                error = Result.ErrorMessage
            };
        }

        private static string GetStateMessage(UploadState state)
        {
            return state switch
            {
                UploadState.Success => "SUCCESS",
                UploadState.FileAccessError => "文件访问出错，请检查写入权限",
                UploadState.SizeLimitExceed => "文件大小超出服务器限制",
                UploadState.TypeNotAllow => "不允许的文件格式",
                UploadState.NetworkError => "网络错误",
                UploadState.NotSupported => "不支持操作",
                _ => "未知错误",
            };
        }

        private bool CheckFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return UploadConfig.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }

        private bool CheckFileSize(int size)
        {
            return size < UploadConfig.SizeLimit;
        }

        public override object DoProcess()
        {
            Execute();
            return WriteResult();
        }
    }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public class UploadConfig
    {
        /// <summary>
        /// 文件命名规则
        /// </summary>
        public string PathFormat { get; set; }


        /// <summary>
        /// 上传表单域名称
        /// </summary>          
        public string UploadFieldName { get; set; }

        /// <summary>
        /// 上传大小限制
        /// </summary>
        public int SizeLimit { get; set; }

        /// <summary>
        /// 上传允许的文件格式
        /// </summary>
        public string[] AllowExtensions { get; set; }

        /// <summary>
        /// 文件是否以 Base64 的形式上传
        /// </summary>
        public bool Base64 { get; set; }

        /// <summary>
        /// Base64 字符串所表示的文件名
        /// </summary>
        public string Base64Filename { get; set; }
    }

    public class UploadResult
    {
        public UploadState State { get; set; }
        public string Url { get; set; }
        public string OriginFileName { get; set; }

        public string ErrorMessage { get; set; }
    }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public enum UploadState
    {
        Success = 0,
        SizeLimitExceed = -1,
        TypeNotAllow = -2,
        FileAccessError = -3,
        NetworkError = -4,
        Unknown = 1,
        NotSupported = 2
    }

}