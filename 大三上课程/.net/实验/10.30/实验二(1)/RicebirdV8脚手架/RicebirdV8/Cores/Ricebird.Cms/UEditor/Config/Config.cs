using System.Text.Json.Nodes;

namespace UEditor.Services
{
    public class Config : UEditorHandler
    {
        public Config(IClient wc)
            : base(wc)
        {
        }

        private static string BuildConfig()
        {
            return configBase;
        }

        private static JsonNode? items = null;
        public static JsonNode Items
        {
            get
            {
                if (items == null)
                {
                    items = JsonNode.Parse(BuildConfig()) ?? new JsonObject();
                }

                return items;
            }
        }
        public static String[] GetStringList(string key)
        {
            var node = Items[key];
            if (node == null)
            {
                return Array.Empty<string>();
            }

            List<string> list = new List<string>();
            foreach (var strNode in node.AsArray())
            {
                if (strNode == null) continue;

                list.Add(strNode.GetValue<string>());
            }

            return list.ToArray();
        }

        public static string GetString(string key)
        {
            return Items[key]?.GetValue<string>() ?? "";
        }

        public static int GetInt(string key)
        {
            return Items[key]?.GetValue<int>() ?? 0;
        }

        public override object DoProcess()
        {
            return Items;
        }

        public const string configBase = @"
{
    ""imageActionName"": ""uploadimage"",
    ""imageFieldName"": ""upfile"",
    ""imageMaxSize"": 20480000, 
    ""imageAllowFiles"": ["".png"", "".jpg"", "".jpeg"", "".gif"", "".bmp""], 
    ""imageCompressEnable"": true, 
    ""imageCompressBorder"": 1600, 
    ""imageInsertAlign"": ""none"",
    ""imageUrlPrefix"": """",
    ""imagePathFormat"": ""upload/image/{yyyy}{mm}{dd}/{time}{rand:6}"",

    ""scrawlActionName"": ""uploadscrawl"",
    ""scrawlFieldName"": ""upfile"",
    ""scrawlPathFormat"": ""upload/image/{yyyy}{mm}{dd}/{time}{rand:6}"",
    ""scrawlMaxSize"": 2048000,
    ""scrawlUrlPrefix"": ""/ueditor/net/"",
    ""scrawlInsertAlign"": ""none"",

    ""snapscreenActionName"": ""uploadimage"",
    ""snapscreenPathFormat"": ""upload/image/{yyyy}{mm}{dd}/{time}{rand:6}"",
    ""snapscreenUrlPrefix"": ""/ueditor/net/"",
    ""snapscreenInsertAlign"": ""none"",

    ""catcherLocalDomain"": [""127.0.0.1"", ""localhost"", ""img.baidu.com""],
    ""catcherActionName"": ""catchimage"",
    ""catcherFieldName"": ""source"",
    ""catcherPathFormat"": ""upload/image/{yyyy}{mm}{dd}/{time}{rand:6}"",
    ""catcherUrlPrefix"": ""/ueditor/net/"", 
    ""catcherMaxSize"": 2048000,
    ""catcherAllowFiles"": ["".png"", "".jpg"", "".jpeg"", "".gif"", "".bmp""],

    ""videoActionName"": ""uploadvideo"",
    ""videoFieldName"": ""upfile"",
    ""videoPathFormat"": ""upload/video/{yyyy}{mm}{dd}/{time}{rand:6}"", 
    ""videoUrlPrefix"": ""/ueditor/net/"",
    ""videoMaxSize"": 102400000, 
    ""videoAllowFiles"": [
        "".flv"", "".swf"", "".mkv"", "".avi"", "".rm"", "".rmvb"", "".mpeg"", "".mpg"",
        "".ogg"", "".ogv"", "".mov"", "".wmv"", "".mp4"", "".webm"", "".mp3"", "".wav"", "".mid""],

    ""fileActionName"": ""uploadfile"", 
    ""fileFieldName"": ""upfile"",
    ""filePathFormat"": ""upload/file/{yyyy}{mm}{dd}/{time}{rand:6}"",
    ""fileUrlPrefix"": ""/ueditor/net/"",
    ""fileMaxSize"": 51200000,
    ""fileAllowFiles"": [
        "".png"", "".jpg"", "".jpeg"", "".gif"", "".bmp"",
        "".flv"", "".swf"", "".mkv"", "".avi"", "".rm"", "".rmvb"", "".mpeg"", "".mpg"",
        "".ogg"", "".ogv"", "".mov"", "".wmv"", "".mp4"", "".webm"", "".mp3"", "".wav"", "".mid"",
        "".rar"", "".zip"", "".tar"", "".gz"", "".7z"", "".bz2"", "".cab"", "".iso"",
        "".doc"", "".docx"", "".xls"", "".xlsx"", "".ppt"", "".pptx"", "".pdf"", "".txt"", "".md"", "".xml""
    ], 

    ""imageManagerActionName"": ""listimage"",
    ""imageManagerListPath"": ""upload/image"", 
    ""imageManagerListSize"": 20, 
    ""imageManagerUrlPrefix"": ""/ueditor/net/"", 
    ""imageManagerInsertAlign"": ""none"", 
    ""imageManagerAllowFiles"": ["".png"", "".jpg"", "".jpeg"", "".gif"", "".bmp""], 

    ""fileManagerActionName"": ""listfile"", 
    ""fileManagerListPath"": ""upload/file"", 
    ""fileManagerUrlPrefix"": ""/ueditor/net/"",
    ""fileManagerListSize"": 20, 
    ""fileManagerAllowFiles"": [
        "".png"", "".jpg"", "".jpeg"", "".gif"", "".bmp"",
        "".flv"", "".swf"", "".mkv"", "".avi"", "".rm"", "".rmvb"", "".mpeg"", "".mpg"",
        "".ogg"", "".ogv"", "".mov"", "".wmv"", "".mp4"", "".webm"", "".mp3"", "".wav"", "".mid"",
        "".rar"", "".zip"", "".tar"", "".gz"", "".7z"", "".bz2"", "".cab"", "".iso"",
        "".doc"", "".docx"", "".xls"", "".xlsx"", "".ppt"", "".pptx"", "".pdf"", "".txt"", "".md"", "".xml""
    ] 
}";
    }
}