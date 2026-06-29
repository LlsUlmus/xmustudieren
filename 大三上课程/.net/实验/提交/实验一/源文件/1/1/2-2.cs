using System;
namespace CSharpStructAndEnum
{
    // 目标1：枚举的ToString的值
    public enum DayOfWeek
    {
        Monday = 1,
        星期二 = 2,
        礼拜3 = 3
    }
    class Program
    {
        static void Main(string[] args)
        {
            // 结构体相关测试代码（略，同上一步）

            // 目标1：枚举的ToString的值
            Console.WriteLine("\n===== 枚举ToString的值 =====");
            Console.WriteLine(DayOfWeek.Monday);
            Console.WriteLine(DayOfWeek.星期二);
            Console.WriteLine(DayOfWeek.礼拜3);

            Console.ReadLine();
        }
    }
}