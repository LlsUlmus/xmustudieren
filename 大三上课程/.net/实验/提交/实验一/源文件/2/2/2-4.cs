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
//            try
//            {
//                FlagDayOfWeek workDays = FlagDayOfWeek.Monday | FlagDayOfWeek.星期二 | FlagDayOfWeek.礼拜3;
//                Console.WriteLine("是否包含星期二：" + workDays.HasFlag(FlagDayOfWeek.星期二));
//                Console.WriteLine("组合项名称：" + workDays); 
//                if (Enum.TryParse<FlagDayOfWeek>("Monday, 星期四", out var parsedDays))
//                {
//                    Console.WriteLine("转换成功，数值：" + (int)parsedDays);
//                }
//                else
//                {
//                    Console.WriteLine("转换失败：无效的枚举组合");
//                }
//            }
//            catch (ArgumentException ex)
//            {
//                Console.WriteLine("报错原因：" + ex.Message);
//            }

//            Console.ReadLine();
//        }
//    }
//}