using Ricebird.Framework.DataValidator;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Ricebird.Cms.Models
{
    [Index(nameof(GuidOrder), AllDescending = true)]
    public class Article : ValidateEntityWithAttachment<CmsAttachment>, IValidatable
    {
        #region 数据库字段
        #region 基本信息
        /// <summary>
        /// 分类ID
        /// </summary>        
        public Guid CategoryId
        {
            get;
            set;
        } = Guid.Empty;

        /// <summary>
        /// 标题
        /// nvarchar(255)
        /// </summary>
        [DisplayName("标题"), MaxLength(60)]
        public string Topic
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// 副标题
        /// </summary>
        [DisplayName("副标题"), MaxLength(60)]
        public string SubTopic
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 标题图Id
        /// bigint
        /// </summary>
        public Guid FeaturedImageAttachmentId
        {
            get;
            set;
        } = Guid.Empty;

        /// <summary>
        /// 标题图文件（带部分路径）
        /// nvarchar(255)
        /// </summary>
        [MaxLength(200)]
        public string FeaturedImage
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// 发布者UserId
        /// bigint
        /// </summary>
        public Guid CreatedBy
        {
            get;
            set;
        }

        /// <summary>
        /// 发布者DisplayName
        /// nvachar(64)
        /// </summary>
        [DisplayName("作者"), MaxLength(40)]
        public string Author
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// 摘要
        /// nvarchar(512)
        /// </summary>
        [DisplayName("摘要")]
        public string Abstract
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// 内容
        /// nvarchar(MAX)
        /// </summary>
        [DisplayName("内容")]
        public string Content
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// 是否允许评论
        /// </summary>
        [DisplayName("是否允许评论")]
        public CommentStatus EnableComment
        {
            get;
            set;
        } = CommentStatus.Disable;

        /// <summary>
        /// 是否为外链
        /// </summary>
        public bool IsOutLink
        {
            get; set;
        } = false;

        /// <summary>
        /// 外链
        /// </summary>
        public string OutLink
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 审核状态
        /// smallint
        /// </summary>
        [DisplayName("审核状态")]
        public VerifyStatus VerifyStatus
        {
            get;
            set;
        } = VerifyStatus.NotSet;

        #region 顺序号
        [JsonIgnore]
        public Guid GuidOrder
        {
            get; set;
        }

        private DateTime _release = DateTime.Now;
        /// <summary>
        /// 发布时间
        /// </summary>
        [DisplayName("发布时间")]
        public DateTime ReleaseTime
        {
            get => _release;
            set
            {
                _release = value;
                GuidOrder = GuidGenerator.ReplaceOrderInfo(GuidOrder, (int)_topMost, _release, DisplayOrder);
            }
        }

        private int _displayOrder = 0;
        /// <summary>
        /// 排列顺序(降序)，在时间轴的情况下，此字段视为唯一排序字段，升序排列。
        /// 注：此字段不需要索引
        /// </summary>
        [DisplayName("排列顺序")]
        public int DisplayOrder
        {
            get => _displayOrder;
            set
            {
                _displayOrder = value;
                GuidOrder = GuidGenerator.ReplaceOrderInfo(GuidOrder, (int)_topMost, _release, _displayOrder);
            }
        }

        private TopMostType _topMost = TopMostType.Normal;
        /// <summary>
        /// 是否置顶(降序)
        /// </summary>
        [DisplayName("是否置顶")]
        public TopMostType TopMost
        {
            get => _topMost;
            set
            {
                _topMost = value;
                GuidOrder = GuidGenerator.ReplaceOrderInfo(GuidOrder, (int)_topMost, _release, _displayOrder);
            }
        }
        #endregion

        /// <summary>
        /// 创建时间
        /// </summary>
        [DisplayName("创建时间")]
        public DateTime CreatedOn
        {
            get;
            set;
        } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [DisplayName("最后更新时间")]
        public DateTime UpdatedOn
        {
            get;
            set;
        } = DateTime.Now;

        /// <summary>
        /// 关键字
        /// </summary>
        [DisplayName("关键字")]
        public string Keyword
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// 来源
        /// </summary>
        [DisplayName("来源")]
        public string Source
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// 点击数
        /// </summary>
        [DisplayName("点击数")]
        public int Hits
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// 是否显示
        /// </summary>
        [DisplayName("是否显示")]
        public bool IsDisplay
        {
            get;
            set;
        } = true;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [DisplayName("最后更新时间")]
        public DateTime LastModified
        {
            get;
            set;
        } = DateTime.Now;

        /// <summary>
        /// 语言
        /// </summary>
        [DisplayName("使用语言"), MaxLength(10)]
        public string Language
        {
            get; set;
        } = "zh-cn";

        /// <summary>
        /// 关联文章
        /// </summary>
        [DisplayName("关联新闻")]
        public string Involved
        {
            get; set;
        } = string.Empty;
        /// <summary>
        /// 新闻类型
        /// </summary>
        [DisplayName("新闻类型")]
        public string ArtType
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// 与其他模块的关联ID
        /// </summary>
        public Guid RelateId
        {
            get; set;
        } = Guid.Empty;

        /// <summary>
        /// 发布的学院ID
        /// </summary>
        public Guid DepartId
        {
            get; set;
        } = Guid.Empty;
        #endregion

        #region 上下线时间
        /// <summary>
        /// 新闻的自动发布时间
        /// </summary>
        public DateTime AvailableStart
        {
            get; set;
        } = DateTime.Now;

        /// <summary>
        /// 新闻的自动发布时间
        /// </summary>
        public DateTime AvailableEnd
        {
            get; set;
        } = new DateTime(2090, 12, 31);
        #endregion

        public Guid SrcId
        {
            get; set;
        } = Guid.Empty;
        #endregion

        #region 非数据库字段
        /// <summary>
        /// 类别名称
        /// </summary>
        [DisplayName("类别名称"), NotMapped]
        public string CategoryName
        {
            get;
            set;
        } = string.Empty;

        [NotMapped]
        public string UniqueCode => ID.To62String();

        [NotMapped]
        public string ActualLink => IsOutLink ? OutLink : $"/cms/{CategoryId.To62String()}/{ID.To62String()}";
        #endregion

        public Article()
        {
            GuidOrder = GuidGenerator.Next(0, DateTime.Now, 0);
        }

        public Article(IUserPrincipal user, Category cate)
        {
            ID = Guid.Empty;
            GuidOrder = GuidGenerator.Next(0, DateTime.Now, 0);
            Author = user.RealName;
            CreatedBy = user.ID;
            CategoryId = cate.ID;
            CategoryName = cate.Name;
        }

        public void BeNewEntity(IUserPrincipal user, Category toCate)
        {
            ID = Guid.NewGuid();
            CreatedOn = DateTime.Now;
            CreatedBy = user.ID;
            CategoryId = toCate.ID;
            GuidOrder = GuidGenerator.ReplaceOrderInfo(GuidOrder, (int)_topMost, _release, _displayOrder);
        }

        public override FluentValidator BuildValidator()
        {
            FluentValidator<Article> fv = new FluentValidator<Article>();
            fv.AutoRulesByAttributes();
            fv.RuleFor(e => e.CategoryId).CategoryMustExist(true);
            fv.RuleFor(e => e.Topic).Required("必须填写新闻标题");
            fv.RuleFor(e => e.Author).Required("必须填写新闻作者");
            fv.RuleFor(e => e.VerifyStatus).Should((c, r, o) =>
            {
                if (!c.CurrentUser.Succeed(Permissions.AuditArticle))
                {
                    o.VerifyStatus = VerifyStatus.NotSet;
                }
            });
            return fv;
        }
    }
}
