using System;
namespace CSharpNumericAndConversion
{
    class Program
    {
        static void Main(string[] args)
        {
            // （二）类型转换
            // 目标1：用户任意输入一个字符串（必定是TypeCode枚举里的一种）
            Console.WriteLine("请输入TypeCode枚举里的一种类型字符串（如Int32、String、DateTime等）：");
            string typeCodeStr = Console.ReadLine();
            // 目标2：用户任意输入一个类型名（.NET类型名，关键字均可）
            Console.WriteLine("请输入.NET类型名或关键字（如int、string、DateTime等）：");
            string typeName = Console.ReadLine();
            // 目标3：将用户输入的字符串，转换为对应类型的对象（支持TypeCode枚举里的所有类型，DateTime格式为“年-月-日 时:分:秒.毫秒”）
            Type targetType = Type.GetType($"System.{typeCodeStr}") ?? Type.GetType(typeName);
            if (targetType != null)
            {
                Console.WriteLine($"请输入要转换为{targetType.Name}类型的值（DateTime格式为：年-月-日 时:分:秒.毫秒）：");
                string valueStr = Console.ReadLine();
                object resultObj = null;
                try
                {
                    if (targetType == typeof(DateTime))
                    {
                        resultObj = DateTime.ParseExact(valueStr, "yyyy-M-d HH:mm:ss.fff", null);
                    }
                    else
                    {
                        resultObj = Convert.ChangeType(valueStr, targetType);
                    }
                    Console.WriteLine($"转换成功，转换后的对象值：{resultObj}，对象类型：{resultObj.GetType()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"转换失败：{ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("输入的类型名无效，无法获取对应类型");
            }
            Console.ReadLine();
        }
    }
}