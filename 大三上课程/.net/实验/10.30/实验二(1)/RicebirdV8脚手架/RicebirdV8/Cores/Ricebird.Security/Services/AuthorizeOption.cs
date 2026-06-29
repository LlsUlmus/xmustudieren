namespace Ricebird.Security.Services
{
    internal class AuthorizeOption : IOption
    {
        public OptionSaveTo OptionSaveTo => OptionSaveTo.Database;

        public string SaveKey => "AuthorizeOption";
    }
}
