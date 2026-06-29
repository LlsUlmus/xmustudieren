using Ricebird.Framework.Configurations.Options;

namespace Ricebird.Framework.Configurations
{
    public class RicebirdFrameworkOptions : IOption
    {
        /// <summary>
        /// 网站密钥
        /// </summary>
        public string SiteKey
        {
            get; set;
        } = GenerateKey(16);

        /// <summary>
        /// 用以从早期版本的米雀框架迁移使用
        /// </summary>
        public string OldKey
        {
            get; set;
        } = "";

        public string Domain
        {
            get; set;
        } = "";

        public SystemCode SystemId
        {
            get; set;
        } = new SystemCode();

        /// <summary>
        /// 逻辑上下文的连接字符串
        /// </summary>
        public DatabaseConnection Database
        {
            get; set;
        } = new DatabaseConnection();

        /// <summary>
        /// 审计上下文的连接字符串
        /// </summary>
        public DatabaseConnection? DiagnosticsDatabase
        {
            get; set;
        } = new DatabaseConnection();

        public string WebAssemblyName
        {
            get; set;
        } = "";

        public string EnvironmentName
        {
            get; set;
        } = "Production";

        public bool UseHttps
        {
            get; set;
        } = false;

        /// <summary>
        /// 该选项仅在开发环境下生效
        /// </summary>
        public bool ShowSqlInApi
        {
            get; set;
        } = false;

        /// <summary>
        /// 是否允许在运行环境使用GET访问Api
        /// </summary>
        public bool AlwaysAllowGet
        {
            get; set;
        } = false;


        /// <summary>
        /// 必须要加载的模块
        /// </summary>
        public string[] Modules
        {
            get; set;
        } = [];

        public OptionSaveTo OptionSaveTo => OptionSaveTo.FileSystem;

        public string SaveKey => "framework";
    }
}
