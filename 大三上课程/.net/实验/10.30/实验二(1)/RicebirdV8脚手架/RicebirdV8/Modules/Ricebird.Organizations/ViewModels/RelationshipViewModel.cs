namespace Ricebird.Organizations.ViewModels
{
    public class RelationshipViewModel
    {
        public Guid ID { get; set; } = Guid.Empty;
        public Guid UserId { get; set; } = Guid.Empty;
        public string RealName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid RoleId { get; set; } = Guid.Empty;
        public string RoleName { get; set; } = string.Empty;
        public Guid DepartId { get; set; } = Guid.Empty;
        public string DepartName { get; set; } = string.Empty;
    }
}
