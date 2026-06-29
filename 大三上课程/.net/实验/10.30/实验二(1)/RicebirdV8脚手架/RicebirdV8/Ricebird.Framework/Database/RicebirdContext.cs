using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ricebird.Framework.Diagnostics;
using System.Text.RegularExpressions;

namespace Ricebird.Framework.Database
{
    /// <summary>
    /// 逻辑上下文，此上下文的数据库用以保存数据
    /// </summary>
    public partial class RicebirdContext(DbContextOptions<RicebirdContext> options, HostEnv HostEnv, IMemoryLogger Logger) : DbContext(options), IScopedDependency
    {
        #region 数据跟踪
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo((EventId evId, LogLevel lv) => evId == RelationalEventId.CommandExecuted, (EventData data) =>
            {
                Logger.Add(data.ToString());
            });
            optionsBuilder.ConfigureWarnings(warings => warings.Ignore(CoreEventId.FirstWithoutOrderByAndFilterWarning));
        }

        private DatabaseDiagnostic? _dbDiagnostic = null;
        public DatabaseDiagnostic DbDiagnostic
        {
            get
            {
                if (_dbDiagnostic == null)
                {
                    List<string> logs = Logger.Logs;
                    int total = 0;
                    int count = logs.Count;

                    Regex r = FindDbCommand();
                    foreach (string log in logs)
                    {
                        Match match = r.Match(log);
                        if (match.Success)
                        {
                            if (int.TryParse(match.Groups["time"].Value, out int time))
                            {
                                total += time;
                            }
                        }
                    }
                    _dbDiagnostic = new DatabaseDiagnostic(total, count, logs);
                }

                return _dbDiagnostic;
            }
        }

        [GeneratedRegex(@"Executed DbCommand \((?<time>\d+)ms\)")]
        private static partial Regex FindDbCommand();
        #endregion

        #region 注册函数
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityTypes = HostEnv.AllEntities;

            RegisterEntities(modelBuilder, entityTypes);

            foreach (var type in entityTypes)
            {
                EntityBase? entity = type.CreateInstance<EntityBase>();
                if (entity != null && entity.GetType().GetMethod("OnModelCreating", [typeof(ModelBuilder)]) is MethodInfo mi)
                {
                    mi.Invoke(entity, [modelBuilder]);
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<SequentialGuid>()
                .HaveConversion<SequentialGuidConverter>();

        }

        private void RegisterEntities(ModelBuilder modelBuilder, IEnumerable<Type> entityTypes)
        {
            foreach (var type in entityTypes)
            {
                modelBuilder
                        .Entity(type);
                HostEnv.WriteLog("实体注册", $"注册了实体{type.FullName}");
            }
        }
        #endregion

        private class SequentialGuidConverter : ValueConverter<SequentialGuid, Guid>
        {
            public SequentialGuidConverter() : base(v => SequentialGuidToGuid(v), v => GuidToSequentialGuid(v))
            {

            }

            public static Guid SequentialGuidToGuid(SequentialGuid value)
            {
                return value;
            }

            public static SequentialGuid GuidToSequentialGuid(Guid value)
            {
                return value;
            }
        }
    }
}
