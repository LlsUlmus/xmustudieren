namespace Ricebird.Framework.Security
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiGroupAttribute(string name) : Attribute
    {
        public string Name => name;
    }
}
