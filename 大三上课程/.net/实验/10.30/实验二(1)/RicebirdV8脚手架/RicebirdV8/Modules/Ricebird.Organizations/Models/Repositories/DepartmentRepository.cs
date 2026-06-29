namespace Ricebird.Organizations.Models
{
    public class DepartmentRepository(RicebirdContext ctx, IServiceProvider scoped) : TreeRepositoryBase<Department>(ctx, scoped)
    {
        #region 数据表
        public DbSet<User> Users => DbContext.Set<User>();
        #endregion

        /// <summary>
        /// 仅供初始化代码用，正式代码里不要调用这个创建！
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public (bool success, string msg, Department depart) CreateDepart(string name)
        {
            Department? depart = FirstOrDefault(e => e.Name == name);
            if (depart != null)
            {
                return (true, "", depart);
            }

            depart = new Department()
            {
                Name = name,
                ParentId = Guid.Empty,
                Source = "系统生成"
            };

            DbSet.Add(depart);
            SaveChanges();

            return (true, "", depart);
        }

        public (bool success, string msg, List<Department> affectRows) MoveDepartment(Guid to, List<Guid> value, string opera)
        {
            var result = MoveNodes(to, value, opera);

            return result;
        }
    }
}
