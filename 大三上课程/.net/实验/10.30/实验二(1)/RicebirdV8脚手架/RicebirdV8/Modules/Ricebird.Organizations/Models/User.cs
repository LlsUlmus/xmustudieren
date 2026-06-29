using Ricebird.Framework.DataValidator;
using Ricebird.Framework.DataValidator.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ricebird.Organizations.Models
{
    [Table("Users")]
    public class User : ValidateEntity, IUser
    {
        #region 数据库字段

        #region 登录凭证
        /// <summary>
        /// 真实姓名
        /// </summary>
        [StringLength(60)]
        public string RealName { get; set; } = string.Empty;

        /// <summary>
        /// 证件类型
        /// <para>
        /// "学工号", "居民身份证", "外国护照", "中国护照", "港澳居民来往内地通行证", "台湾居民来往大陆通行证", "外国人永久居留身份证"
        /// </para>
        /// </summary>
        [StringLength(20)]
        public string CodeType
        {
            get; set;
        } = "学工号";

        /// <summary>
        /// 标记代码，证件号，必填
        /// </summary>
        [StringLength(30)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 升序
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 手机号码
        /// </summary>
        [StringLength(30)]
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// 登录用用户名
        /// </summary>
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 第三方登录ID
        /// </summary>
        [StringLength(50)]
        public string OpenId { get; set; } = string.Empty;

        /// <summary>
        /// 用户密码
        /// </summary>
        [Required, JsonIgnore, StringLength(40)]
        public string UserPassword { get; set; } = "NONE";

        /// <summary>
        /// 用户的头像
        /// </summary>
        [StringLength(100)]
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// 访问级别
        /// </summary>
        public AccessLevel Level { get; set; } = AccessLevel.Max;

        /// <summary>
        /// 根部门
        /// </summary>
        public Guid RootDepartId
        {
            get; set;
        } = Guid.Empty;

        /// <summary>
        /// 账号锁定的触发次数
        /// </summary>
        [NonValidation]
        public int LockCount { get; set; } = 0;

        /// <summary>
        /// 账号锁定截止时间
        /// </summary>
        [NonValidation]
        public DateTime LockTo { get; set; } = ConstKeys.MinDate;

        /// <summary>
        /// 审核状态
        /// </summary>
        public UserStatus AuditStatus
        {
            get; set;
        } = UserStatus.Enable;

        /// <summary>
        /// 用户来源
        /// <para>
        /// 后台添加，自行注册，数据同步，微信注册，统一认证，后台导入，系统生成
        /// </para>
        /// </summary>
        [StringLength(6)]
        public string UserSource
        {
            get; set;
        } = "";

        /// <summary>
        /// 用户类型
        /// <para>
        /// 由系统同步自动维护：本科生，研究生，教职工
        /// </para>
        /// </summary>
        [StringLength(5)]
        public string UserType
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 用户类型代码，判断用：本科生，研究生，教职工， 预毕业名单
        /// <para>
        /// 由系统自动维护：B, Y, J, G
        /// </para>
        /// </summary>
        [StringLength(1)]
        public string UserTypeCode
        {
            get; set;
        } = String.Empty;
        #endregion

        #region 基本信息
        /// <summary>
        /// 性别
        /// </summary>
        [StringLength(4)]
        public string Gender
        {
            get; set;
        } = "男";

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime Birthday
        {
            get; set;
        } = ConstKeys.MinDate;
        #endregion

        #region 学生专用
        /// <summary>
        /// 年级
        /// </summary>
        [StringLength(10)]
        public string Grade
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 专业
        /// </summary>
        [StringLength(30)]
        public string Major
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 所在学院
        /// </summary>
        [StringLength(30)]
        public string Department
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 预计毕业时间
        /// </summary>
        public DateTime GraduadeTime
        {
            get; set;
        } = ConstKeys.MinDate;

        /// <summary>
        /// 教改类型
        /// </summary>
        [StringLength(10)]
        public string ReformType
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 国籍
        /// </summary>
        [StringLength(20)]
        public string Nationality
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 民族
        /// </summary>
        [StringLength(10)]
        public string Ethnic
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 籍贯
        /// </summary>
        [StringLength(20)]
        public string BirthPlace
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 政治面貌
        /// </summary>
        [StringLength(20)]
        public string Politics
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 学生来源
        /// </summary>
        [StringLength(20)]
        public string StudentFrom
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 港澳台侨
        /// </summary>
        [StringLength(20)]
        public string Gatq
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 生源地
        /// </summary>
        [StringLength(20)]
        public string StudentSource
        {
            get; set;
        } = string.Empty;
        #endregion

        #region 教师专用
        /// <summary>
        /// 职称
        /// </summary>
        public string Title
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 职务
        /// </summary>
        public string Position
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 学历
        /// </summary>
        public string Education
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// QQ号，学业竞赛用
        /// </summary>
        public string QQ
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 办公室电话，学业竞赛用
        /// </summary>
        public string OfficePhone
        {
            get; set;
        } = string.Empty;
        #endregion

        #endregion

        #region 创建数据库索引/隐式转换
        public override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(e => e.Code);
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Code) ? RealName : $"({RealName}/{Code})";
        #endregion

        public override FluentValidator BuildValidator()
        {
            FluentValidator<User> fluent = new FluentValidator<User>();
            fluent.AutoRulesByAttributes();
            fluent.RuleFor(e => e.UserSource).MustInDict("用户来源");
            fluent.RuleFor(e => e.RealName).Required("必须填写姓名");
            fluent.RuleFor(e => e.Code).Required("必须填写证件号").Should((client, result, obj) =>
            {
                UserRepository repo = client.Resolve<UserRepository>();
                obj.Code = obj.Code.Trim().ToUpper();
                if (repo.ExistCode(obj.Code, obj.ID))
                {
                    result.SetFailure(nameof(Code), $"系统中已经存在证件号为{obj.Code}的用户");
                }
            });
            fluent.RuleFor(e => e.UserName).Should((client, result, obj) =>
            {
                UserRepository repo = client.Resolve<UserRepository>();
                obj.UserName = obj.UserName.Trim();
                if (repo.ExistUserName(obj.UserName, obj.ID))
                {
                    result.SetFailure(nameof(UserName), $"系统中已经存在用户名为{obj.Code}的用户");
                }
            });
            fluent.RuleFor(e => e.Email).Required("必须填写邮箱").IsEmail().Should((client, result, obj) =>
            {
                UserRepository repo = client.Resolve<UserRepository>();
                obj.Email = obj.Email.Trim();
                if (repo.ExistEmail(obj.Email, obj.ID))
                {
                    result.SetFailure(nameof(Email), $"系统中已经存在邮箱为{obj.Email}的用户");
                }
            });
            fluent.RuleFor(e => e.Mobile).Required("必须填写手机号").IsMobile().Should((client, result, obj) =>
            {
                UserRepository repo = client.Resolve<UserRepository>();
                obj.Mobile = obj.Mobile.Trim();
                if (repo.ExistMobile(obj.Mobile, obj.ID))
                {
                    result.SetFailure(nameof(Mobile), $"系统中已经存在手机号为{obj.Mobile}的用户");
                }
            });
            fluent.RuleFor(e => e.ID).Should((client, result, obj) =>
            {
                if (!AnyStringHasContent(obj.Code, obj.UserName, obj.Email, obj.Mobile))
                {
                    result.SetFailure(nameof(Code), "用户名，手机号，邮箱和学工号不能同时为空");
                    result.SetFailure(nameof(UserName), "用户名，手机号，邮箱和学工号不能同时为空");
                    result.SetFailure(nameof(Email), "用户名，手机号，邮箱和学工号不能同时为空");
                    result.SetFailure(nameof(Mobile), "用户名，手机号，邮箱和学工号不能同时为空");
                }
            });
            return fluent;
        }

        public Department GetOrg(IOrgService orgService) => ((orgService.GetOrgByDeptId(RootDepartId) ?? orgService.DefaultDepart) as Department)!;

        public CommonUser ToCommonUser() => new CommonUser(ID, RealName, Avatar, Code, Mobile, Email, AuditStatus, UserPassword, Level, OpenId, RootDepartId);
    }
}
