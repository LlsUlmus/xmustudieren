namespace ConsoleApp.Calculator.Operators
{
    /// <summary>
    /// 减法运算符
    /// </summary>
    public class SubOperator : AbstractOperator
    {
        public override string Symbol => "-";

        public override double Execute(double left, double right)
        {
            return left - right;
        }
    }
}

