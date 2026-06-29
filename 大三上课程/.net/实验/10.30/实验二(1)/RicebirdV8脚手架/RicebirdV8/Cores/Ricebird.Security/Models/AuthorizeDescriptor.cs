namespace Ricebird.Security.Models
{
    public class AuthorizeDescriptor(string name = "", AuthorizeResult result = AuthorizeResult.NoSet)
    {

        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// 授权结果
        /// </summary>
        public AuthorizeResult Result { get; set; } = result;

        public static implicit operator AuthorizeDescriptor((string name, AuthorizeResult result) descriptor) => new AuthorizeDescriptor(descriptor.name, descriptor.result);
    }
}
