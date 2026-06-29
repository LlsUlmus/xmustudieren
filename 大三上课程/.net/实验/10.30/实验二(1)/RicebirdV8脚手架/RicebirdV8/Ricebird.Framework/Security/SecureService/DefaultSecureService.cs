using Ricebird.Security.Services;

namespace Ricebird.Framework.Security.SecureService
{
    public class DefaultSecureService(IOptionService optionService) : ISecureService
    {
        private SecurityOption SecurityOption => optionService.LoadOptions<SecurityOption>();

        public string DefaultHash(string text)
        {
            return SecureHelper.GetSha256(text);
        }


        public string InitializePasssword
        {
            get => SecurityOption.InitalizePassword;
        }

        public string SuperPassword
        {
            get => SecurityOption.SuperPassword;
        }

        public void SetPasssword(string initPwd, string superPwd)
        {
            var opt = SecurityOption;
            if (!string.IsNullOrWhiteSpace(initPwd))
            {
                opt.InitalizePassword = SecureHelper.GetSha1(initPwd);
            }

            if (!string.IsNullOrWhiteSpace(superPwd))
            {
                opt.SuperPassword = SecureHelper.GetSha1(superPwd);
            }

            optionService.SaveOptions(opt);
        }
    }
}
