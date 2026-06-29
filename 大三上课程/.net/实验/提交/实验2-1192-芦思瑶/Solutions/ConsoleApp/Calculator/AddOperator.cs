namespace ConsoleApp.Calculator
{
    /// <summary>
    /// 加法运算符
    /// </summary>
    public class AddOperator : AbstractOperator
    {
        public override string Symbol => "+";

        public override double Execute(double left, double right)
        {
            return left + right;
        }
    }
}

