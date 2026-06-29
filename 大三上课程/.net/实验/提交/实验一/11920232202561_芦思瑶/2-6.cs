using System;
namespace FlagEnumDemo
{
    [Flags]
    public enum FlagDayOfWeek
    {
        Monday = 1,      // 2^0 = 1
        星期二 = 2,      // 2^1 = 2
        礼拜3 = 4,       // 2^2 = 4
        星期四 = 8,      // 2^3 = 8
        星期五 = 16      // 2^4 = 16
    }
    class Program
    {
        static void Main(string[] args)
        {
            // 目标5：使用foreach循环枚举的值或者项
            Console.WriteLine("===== 使用foreach循环遍历枚举 =====");
            // 遍历枚举的所有项（值和名称）
            Console.WriteLine("枚举的所有项及对应值：");
            foreach (FlagDayOfWeek day in Enum.GetValues(typeof(FlagDayOfWeek)))
            {
                // 输出枚举项名称和对应的数值
                Console.WriteLine($"名称: {day}, 数值: {(int)day}");
                Console.WriteLine($"当前项的类型: {day.GetType().Name}");
            }
            Console.WriteLine("\n枚举的所有名称：");
            foreach (string name in Enum.GetNames(typeof(FlagDayOfWeek)))
            {
                Console.WriteLine(name);
            }
            Console.ReadLine();
        }
    }
}
