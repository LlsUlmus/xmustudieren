namespace Ricebird.Framework.DataValidator.Rules
{
    public abstract class AbstractTypeValidateRule<T> : AbstactValidateRule<T>
    {
        public abstract Type ForType
        {
            get;
        }
    }
}
