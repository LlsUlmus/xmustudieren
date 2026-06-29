//using System;
//namespace FlagEnumDemo
//{
//    [Flags]
//    public enum FlagDayOfWeek  
//    {
//        Monday = 1,      // 2^0 = 1
//        星期二 = 2,      // 2^1 = 2
//        礼拜3 = 4,       // 2^2 = 4
//        星期四 = 8,      // 2^3 = 8
//        星期五 = 16      // 2^4 = 16
//    }

//    class Program
//    {
//        static void Main(string[] args)
//        {
//            // 目标4：将对应的数字值转换为枚举值
//            Console.WriteLine("\n===== 数字值转换为枚举值 =====");
   
//            object enumFromName = Enum.Parse(typeof(FlagDayOfWeek), "星期二");
//            Console.WriteLine($"通过名称“星期二”转换的枚举值：{enumFromName}，数值：{(int)enumFromName}");

//            object enumFromNumStr = Enum.Parse(typeof(FlagDayOfWeek), "2");
//            Console.WriteLine($"通过数字字符串“2”转换的枚举值：{enumFromNumStr}，数值：{(int)enumFromNumStr}");

//            Console.ReadLine();
//        }
//    }
//}