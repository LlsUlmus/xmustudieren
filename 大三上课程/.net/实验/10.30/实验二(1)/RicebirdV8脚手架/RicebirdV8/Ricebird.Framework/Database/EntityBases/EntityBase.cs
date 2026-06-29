using Microsoft.EntityFrameworkCore;
using Ricebird.Framework.Clients;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ricebird.Framework.Database
{
    [PrimaryKey(nameof(ID))]
    public abstract class EntityBase : IDependency
    {
        public virtual Guid ID
        {
            get;
            set;
        } = SequentialGuid.NewSuid();

        [NotMapped, JsonIgnore]
        public virtual EntityStatus EntityStatus { get; set; } = EntityStatus.Unknown;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        [NotMapped, JsonIgnore]
        public virtual IClient Client { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        ///// <summary>
        ///// 对变更进行同步
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="entity">变更后的实体，由OnEntityChanged事件提供</param>
        ///// <param name="changeProperty">变更的属性，由OnEntityChanged提供</param>
        ///// <param name="ignoreProperty">在记录时需要忽略的属性</param>
        //public virtual void AcceptChange<T>(T entity, List<string> changeProperty, params string[] ignoreProperty)
        //    where T : EntityBase => this.CopyPropertiesFrom(entity, changeProperty, ignoreProperty);

        public virtual void OnModelCreating(ModelBuilder builder)
        {

        }

        /// <summary>
        /// 用户自定义的绑定代码。在请求发生时，会调用此绑定代码。
        /// <para>
        /// 适用情况：1. 直接在Action的参数中输入EnityBase的子类。 2. 调用IClient.FillObject系列方法。
        /// </para>
        /// </summary>
        /// <param name="client"></param>
        public virtual void BindClientData(IClient client)
        {

        }
    }
}
