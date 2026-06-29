using System;

namespace ConsoleApp.Calculator
{
    public class SimpleCalculator
    {
        public void Run()
        {
            // 加法测试（直接写死）
            double left = 1, right = 2;
            var addOperator = new AddOperator();
            var result = addOperator.Execute(left, right);
            Console.WriteLine($"{left} {addOperator.Symbol} {right} = {result}");

            // 减法测试（直接写死）
            var subOperator = new Operators.SubOperator();
            var result2 = subOperator.Execute(left, right);
            Console.WriteLine($"{left} {subOperator.Symbol} {right} = {result2}");
        }
    }
}
