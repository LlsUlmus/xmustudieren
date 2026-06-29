namespace Ricebird.Security.Services
{
    public class SecurityOption : IOption
    {
        public OptionSaveTo OptionSaveTo => OptionSaveTo.Database;

        public string SaveKey => "SecurityOption";

        /// <summary>
        /// 无操作的过期时间。
        /// <para>
        /// 这个时间即不能小于1分钟，也不能多于24小时。
        /// </para>
        /// </summary>
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// 每个用户最多同时拥有多少Token
        /// </summary>
        public int MaxTokenForOneUser { get; set; } = 10;

        /// <summary>
        /// 初始密码，用户新建时就有的密码
        /// </summary>
        public string InitalizePassword
        {
            get; set;
        } = GenerateId(8);

        /// <summary>
        /// 超级密码，此密码可以登录所有人的账号
        /// </summary>
        public string SuperPassword
        {
            get; set;
        } = GenerateId(16);

        ///// <summary>
        ///// 是否允许用户使用初始密码登录
        ///// </summary>
        //public bool AllowLoginByInitalizePassword
        //{
        //    get; set;
        //} = false;

        /// <summary>
        /// 必须在使用前完善用户信息
        /// </summary>
        public bool FillUserInformationBeforeUse
        {
            get; set;
        } = true;
    }
}
