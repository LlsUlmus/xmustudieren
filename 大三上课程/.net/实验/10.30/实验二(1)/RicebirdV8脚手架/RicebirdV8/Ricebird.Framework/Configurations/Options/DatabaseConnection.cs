namespace Ricebird.Framework.Configurations.Options
{
    public class DatabaseConnection
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string DataSource
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Catalog
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserID
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get; set;
        } = string.Empty;

        public bool UseIntegratedSecurity
        {
            get; set;
        } = false;

        [JsonIgnore]
        public string ConnectionString => UseIntegratedSecurity ? $"Data Source={DataSource};Initial Catalog={Catalog};Integrated Security=True;TrustServerCertificate=True;"
            : $"Data Source={DataSource};Initial Catalog={Catalog};Persist Security Info=True;User ID={UserID};Password={Password};TrustServerCertificate=True;";
    }
}
