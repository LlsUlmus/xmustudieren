namespace Ricebird.FileStorage.Services
{
    public class FileStorageService(IServiceProvider provider, MimeTypeService mts) : IFileStorageService
    {
        private IOptionService OptionService { get; init; } = provider.Resolve<IOptionService>();
        private string PermanentDir => Option.GetPhyicPermanentDirectory();
        private string TemporaryDir => Option.GetPhyicTemporaryDirectory();
        //private readonly string TemporaryDir = string.Empty;
        private FileStorageOption Option => OptionService.LoadOptions<FileStorageOption>();
        private MimeTypeService MimeTypeService { get; set; } = mts;

        public (string msg, IFile? file) CreateFile(Stream stream, string srcFileName, string module, IClient client)
        {
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);

            return CreateFile(data, srcFileName, module, client);
        }

        public (string msg, IFile? file) CreateFile(string base64Str, string srcFileName, string module, IClient client)
        {
            try
            {
                byte[] data = Convert.FromBase64String(base64Str);
                return CreateFile(data, srcFileName, module, client);
            }
            catch
            {
                return ($"输入字符串不是正确的BASE64格式字符串", null);
            }
        }

        public (string msg, IFile? file) CreateFile(byte[] bytes, string srcFileName, string module, IClient client)
        {
            // 先确定数据库里是不是有完全一样的文件
            int size = bytes.Length;
            if (size == 0)
            {
                return ($"没有大小的文件不能保存", null);
            }

            if (size > Option.MaxSizeLimit)
            {
                return ($"超过限制大小的文件不能保存。当前文件大小限制：{Option.MaxSizeLimit / 1024 / 1024}MB", null);
            }

            string md5 = SecureHelper.MD5(bytes);
            var repo = client.Resolve<PermanentFileRepository>();

            // 同模块，同MD5不能重复
            var entity = repo.DbSet.FirstOrDefault(e => e.MD5 == md5 && e.ModuleName == module && e.Size == size);
            if (entity != null)
            {
                if (CheckFileExist(entity))
                {
                    return ("", entity);
                }
                else
                {
                    return ("该文件发布在其它服务器上", entity);
                }
            }
            else
            {
                entity = new PermanentFile();
                repo.DbSet.Add(entity);
            }

            // 数据库里找不到，则保存这个文件
            DateTime now = DateTime.Now;
            string finalDir = Path.Combine(PermanentDir, module, now.Year.ToString(), now.Month.ToString());

            Directory.CreateDirectory(finalDir);

            //第一步，验证后缀名
            string extensionName = Path.GetExtension(srcFileName);
            string mimeType = MimeTypeService.GetMimeType(extensionName);
            if (mimeType == MimeTypeService.DisabledMimeType)
            {
                return ($"不支持后缀名为“{extensionName}”的文件类型", null);
            }

            //第二步，生成附件保存地址
            Guid fileId = entity.ID;
            string fileName = $"{extensionName.Replace(".", "")}+{fileId.To62String()}";
            string save = $@"{finalDir}\{fileName}";

            //步骤四，生成文件并且写入
            using (FileStream stream = new FileStream(save, FileMode.CreateNew))
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            entity.Size = size;
            entity.DisplayName = srcFileName;
            entity.MD5 = md5;
            entity.ModuleName = module;
            entity.PhysicPath = save;
            entity.MimeType = mimeType;
            entity.CreateBy = client.CurrentUser.ID;
            entity.CreatedOn = now;

            try
            {
                repo.SaveChanges();
            }
            catch (Exception ex)
            {
                client.LogException(ex, "FileStorageService", "UploadFile");
                return ($"保存失败，原因是: {ex.Message}", null);
            }

            return ("", entity);
        }

        public void DeleteFile(Guid code)
        {
            var repo = provider.Resolve<PermanentFileRepository>();
            var entity = repo.DbSet.FirstOrDefault(e => e.ID == code);
            if (entity == null) return;

            var path = entity.PhysicPath;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                repo.DbSet.Remove(entity);
                repo.SaveChanges();
            }
            catch
            {

            }
        }

        public (string msg, IFile? file) CreateTemporaryFile(Stream stream, string srcFileName)
        {
            return CreateTemporaryFile(stream.ReadAllBytes(), srcFileName);
        }

        public (string msg, IFile? file) CreateTemporaryFile(byte[] bytes, string srcFileName)
        {
            // 先确定数据库里是不是有完全一样的文件
            int size = bytes.Length;
            if (size == 0)
            {
                return ($"没有大小的文件不能保存", null);
            }

            // 数据库里找不到，则保存这个文件
            string finalDir = TemporaryDir;

            Directory.CreateDirectory(finalDir);

            //第一步，验证后缀名
            string extensionName = Path.GetExtension(srcFileName);
            string mimeType = MimeTypeService.GetMimeType(extensionName);
            if (mimeType == MimeTypeService.DisabledMimeType)
            {
                return ($"不支持后缀名为“{extensionName}”的文件类型", null);
            }

            //第二步，生成附件保存地址
            TemporaryFile temp = new TemporaryFile();
            Guid fileId = temp.ID;
            string fileName = $"{extensionName.Replace(".", "")}+{fileId.To62String()}";
            string save = $@"{finalDir}\{fileName}";
            temp.PhysicPath = save;

            using (FileStream stream = new FileStream(save, FileMode.CreateNew))
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            return (srcFileName, temp);
        }

        public (FileStream stream, IFile file) CreateTemporaryFile(string srcFileName)
        {
            string finalDir = TemporaryDir;
            Directory.CreateDirectory(finalDir);

            //第一步，验证后缀名
            string extensionName = Path.GetExtension(srcFileName);
            string mimeType = MimeTypeService.GetMimeType(extensionName);
            if (mimeType == MimeTypeService.DisabledMimeType)
            {
                throw new NotSupportedException($"不支持后缀名为“{extensionName}”的临时文件");
            }

            TemporaryFile temp = new TemporaryFile();
            Guid fileId = temp.ID;
            string fileName = $"{extensionName.Replace(".", "")}+{fileId.To62String()}";
            string save = $@"{finalDir}\{fileName}";
            temp.PhysicPath = save;

            FileStream stream = new FileStream(save, FileMode.Create);
            return (stream, temp);
        }

        public void DeleteTemporaryFile(IFile file)
        {
            if (file.StorageType != FileStorageType.Temporary)
            {
                return;
            }

            if (CheckFileExist(file))
            {
                File.Delete(file.PhysicPath);
            }
        }

        public void ClearTemporaryDir()
        {
            if (Directory.Exists(TemporaryDir))
            {
                Directory.Delete(TemporaryDir, true);
            }

            Directory.CreateDirectory(TemporaryDir);
        }

        public static bool CheckFileExist(IFile file) => File.Exists(file.PhysicPath);

        public IFile? GetFile(Guid id)
        {
            var repo = provider.Resolve<PermanentFileRepository>();
            return repo.DbSet.FirstOrDefault(e => e.ID == id);
        }

        public bool IsFileInStorage(PathString pathString) =>
            pathString.StartsWithSegments("/permanent") || pathString.StartsWithSegments("/tempory") || pathString.StartsWithSegments("/Attachments");

        public (byte[]? bytes, string mimeType, string downloadFileName, string displayName) GetFileBytes(PathString pathString)
        {
            PermanentFileRepository repo = provider.Resolve<PermanentFileRepository>();

            if (pathString.StartsWithSegments("/permanent", out PathString remaining))
            {
                var path = PermanentDir;
                if (remaining.StartsWithSegments("/virtual"))
                {
                    remaining.PopSegment(out remaining);
                    string fileName = Path.GetFileNameWithoutExtension(remaining);
                    string ext = Path.GetExtension(remaining);
                    fileName.TryParseToGuid(out Guid fileId);
                    var file = repo.DbSet.FirstOrDefault(e => e.ID == fileId);
                    if (file == null)
                    {
                        return (null, string.Empty, string.Empty, string.Empty);
                    }

                    path = Path.Combine(path, file.PhysicPath);
                    return GetFinalFile(path, file.DisplayName);
                }
                else
                {
                    // 永久保存的文件路径：
                    return ReturnFile(path, remaining);
                }
            }

            if (pathString.StartsWithSegments("/tempory", out remaining))
            {
                // 临时保存的文件路径：
                var path = TemporaryDir;
                return ReturnFile(path, remaining);
            }

            //if (pathString.StartsWithSegments("/Attachments"))
            //{
            //    return TryGetOldCxwFile(pathString.ToString());
            //}

            return (null, string.Empty, string.Empty, string.Empty);
        }

        private (byte[]? bytes, string mimeType, string downloadFileName, string displayName) ReturnFile(string path, PathString remaining)
        {
            string module = remaining.PopSegment(out remaining);
            path = Path.Combine(path, module);
            // 找不到这个目录的情况下
            if (!Directory.Exists(path) || !remaining.HasValue)
            {
                return (null, string.Empty, string.Empty, string.Empty);
            }

            // 处理文件名, 如果找不到，直接返回
            path = Path.Combine(path, remaining.Value[1..].Replace('/', '\\'));
            return GetFinalFile(path);
        }

        public (byte[]? bytes, string mimeType, string downloadFileName, string displayName) GetFinalFile(string path, string displayName = "")
        {
            IMimeTypeService mimeTypeService = mts;
            string dir = Path.GetDirectoryName(path) ?? "";
            string fileName = Path.GetFileName(path);
            string[] array = fileName.Split(".");
            string actualFileName = array.Length == 1 ? array[0] : $"{array[1]}+{array[0]}";
            array = actualFileName.Split("+");
            string downloadFileName = array.Length == 1 ? array[0] : $"{array[1]}.{array[0]}";
            string mimeType = mimeTypeService.GetMimeType(downloadFileName);
            path = Path.Combine(dir, actualFileName);
            displayName = displayName.HasValue() ? displayName : downloadFileName;
            if (!File.Exists(path))
            {
                return (null, string.Empty, string.Empty, displayName);
            }

            try
            {
                using Stream stream = new FileStream(path, FileMode.Open);
                byte[] buffer = stream.ReadAllBytes();
                return (buffer, mimeType, downloadFileName, displayName);
            }
            catch
            {
                return (null, string.Empty, string.Empty, displayName);
            }
        }

        //private (byte[]? bytes, string mimeType, string downloadFileName) TryGetOldCxwFile(string path)
        //{
        //    HttpClient httpClient = new HttpClient()
        //    {
        //        BaseAddress = new Uri("http://cxw.xmu.edu.cn")
        //    };

        //    IMimeTypeService mimeTypeService = mts;
        //    string mimeType = mimeTypeService.GetMimeType(path);
        //    string fileName = Path.GetFileName(path);

        //    HttpResponseMessage response = httpClient.GetAsync(path).GetAwaiter().GetResult();
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return (null, string.Empty, string.Empty);
        //    }

        //    Stream content = response.Content.ReadAsStream();
        //    return (content.ReadAllBytes(), mimeType, fileName);
        //}
    }
}
