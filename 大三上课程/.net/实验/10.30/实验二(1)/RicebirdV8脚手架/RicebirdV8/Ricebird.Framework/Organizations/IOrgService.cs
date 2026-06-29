using Ricebird.Framework.Clients;
using Ricebird.Framework.DataValidator;

namespace Ricebird.Framework.Organizations
{
    public interface IOrgService : ISingletonDependency
    {
        IEnumerable<IDepart> AllDepartments
        {
            get;
        }

        IDepart DefaultDepart { get; }

        IDepart? GetDepartById(Guid id);
        IDepart? GetDepartByName(string name);
        /// <summary>
        /// 将部门ID转换为名字。这个转换调用静态字典，查询速度极快
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string this[Guid id]
        {
            get;
        }
        IDepart? GetOrgByDeptId(Guid id);
        List<IDepart> GetOrgs();

        bool TryGetOrgByDeptId(Guid id, [NotNullWhen(true)] out IDepart? depart);

        bool TryGetOrgByName(string name, [NotNullWhen(true)] out IDepart? depart);

        (bool success, string msg, object data) GetDepartTree(string categoryName, Guid parentId, bool force, IClient client);

        (bool success, string msg, ValidateResult result, IDepart? data) SaveDepartment(IClient client, bool force);

        void RemoveDepartment(Guid id, IClient client);
    }
}
