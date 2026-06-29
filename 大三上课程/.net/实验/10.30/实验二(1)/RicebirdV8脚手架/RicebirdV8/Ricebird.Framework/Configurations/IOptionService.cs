namespace Ricebird.Framework.Configurations
{
    public interface IOptionService : ISingletonDependency
    {
        /// <summary>
        /// 读取一个配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T LoadOptions<T>() where T : IOption, new();

        /// <summary>
        /// 读取一个配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="opt"></param>
        /// <returns></returns>
        T LoadOptions<T>(T opt) where T : IOption, new();

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="opt"></param>
        void SaveOptions<T>(T opt) where T : IOption, new();
    }
}