namespace Ricebird.Framework
{
    /// <summary>
    /// 加了后， 就不会自动 注册了
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DontAutoRegistion : Attribute
    {
    }
}
