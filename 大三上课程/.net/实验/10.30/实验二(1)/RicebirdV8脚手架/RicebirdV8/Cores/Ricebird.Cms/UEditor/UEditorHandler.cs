namespace UEditor.Services
{
    /// <summary>
    /// 用以处理的类
    /// </summary>
    public abstract class UEditorHandler : IUEditorHandler
    {
        protected IClient Client;
        public UEditorHandler(IClient workContext)
        {
            Client = workContext;
        }
        public abstract object DoProcess();
        public string Process()
        {
            object obj = DoProcess();
            if (obj != null)
            {
                return obj.SearializeJson();
            }

            return string.Empty;
        }
    }
}