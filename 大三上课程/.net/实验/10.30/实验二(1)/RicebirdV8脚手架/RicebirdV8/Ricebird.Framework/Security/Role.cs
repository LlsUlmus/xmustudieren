namespace Ricebird.Framework.Security
{
    public class Role
    {
        [JsonIgnore]
        public Guid ID
        {
            get; set;
        } = Guid.Empty;

        public string Name
        {
            get; set;
        } = string.Empty;

        public Guid ForDepart
        {
            get; set;
        } = Guid.Empty;

        public string DepartName
        {
            get; set;
        } = string.Empty;

        public string DisplayName
        {
            get; set;
        } = string.Empty;

        [JsonIgnore]
        public bool UseAsPrinciple
        {
            get; set;
        } = false;

        public AuthorizeResult NotSetEquals
        {
            get;
            set;
        } = AuthorizeResult.Deny;

        public List<string> Permissions
        {
            get; set;
        } = [];

        public List<object> Menus
        {
            get; set;
        } = [];
    }
}
