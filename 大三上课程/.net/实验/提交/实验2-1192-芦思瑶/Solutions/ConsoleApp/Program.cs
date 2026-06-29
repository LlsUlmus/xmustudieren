using System;
using ConsoleApp.Calculator;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var calculator = new SimpleCalculator();
            calculator.Run();
            Console.WriteLine("计算器程序已完成。按任意键结束...");
            Console.ReadKey();
        }
    }
}
