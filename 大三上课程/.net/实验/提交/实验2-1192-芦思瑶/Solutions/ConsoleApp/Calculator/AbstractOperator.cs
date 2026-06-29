namespace ConsoleApp.Calculator
{
    /// <summary>
    /// 抽象运算符基类
    /// </summary>
    public abstract class AbstractOperator
    {
        /// <summary>
        /// 运算符符号（如 +、-、*、/）
        /// </summary>
        public abstract string Symbol { get; }

        /// <summary>
        /// 执行计算
        /// </summary>
        /// <param name="left">左操作数</param>
        /// <param name="right">右操作数</param>
        /// <returns>计算结果</returns>
        public abstract double Execute(double left, double right);
    }
}

