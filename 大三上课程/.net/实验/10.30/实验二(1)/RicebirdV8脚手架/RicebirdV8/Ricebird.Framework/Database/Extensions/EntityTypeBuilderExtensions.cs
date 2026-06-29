using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ricebird.Framework.Database
{
    public static class EntityTypeBuilderExtensions
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        internal static HostEnv HostEnv { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        public static ReferenceNavigationBuilder HasOneByType(this EntityTypeBuilder builder, string typeName)
        {
            Type? type = HostEnv.AllEntities.FirstOrDefault(e => e.Name == typeName) ?? throw new ArgumentException($"{typeName} 指向的表不存在");
            return builder.HasOne(type);
        }

        /// <summary>
        /// 构建一个外键，这个函数必须在从表的OnModelCreating处调用
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="this">从表的this对象</param>
        /// <param name="primaryType">主表的类型</param>
        /// <param name="foreignKey">外键的键名</param>
        /// <returns></returns>
        public static ReferenceCollectionBuilder BuildForeignKey(this ModelBuilder builder, EntityBase @this, string primaryType, string foreignKey)
        {
            return builder.Entity(@this.GetType())
                .HasOneByType(primaryType)
                .WithMany()
                .HasForeignKey(foreignKey);
        }

        /// <summary>
        /// 构建一个外键，这个函数必须在从表的OnModelCreating处调用
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="T">从表的类型</typeparam>
        /// <param name="primaryType">主表的类型</param>
        /// <param name="foreignKey">外键的键名</param>
        /// <returns></returns>
        public static ReferenceCollectionBuilder BuildForeignKey<T>(this ModelBuilder builder, string primaryType, string foreignKey)
            where T : class
        {
            return builder.Entity<T>()
                .HasOneByType(primaryType)
                .WithMany()
                .HasForeignKey(foreignKey);
        }
    }
}
