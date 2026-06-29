using System;
namespace CSharpNumericAndConversion
{
    class Program
    {
        static void Main(string[] args)
        {
            // 目标1：给定一个类型，输出它的.NET类型
            int intNum = 10;
            Console.WriteLine($"int类型对应的.NET类型：{intNum.GetType()}");
            string str = "test";
            Console.WriteLine($"string类型对应的.NET类型：{str.GetType()}");
            double doubleNum = 3.14;
            Console.WriteLine($"double类型对应的.NET类型：{doubleNum.GetType()}");
            // 目标2：枚举一个由值类型装箱的Object类型对象，确认它的基本类型
            object boxedInt = (object)20;
            Console.WriteLine($"装箱后的int类型对象的基本类型：{boxedInt.GetType()}");
            object boxedBool = (object)true;
            Console.WriteLine($"装箱后的bool类型对象的基本类型：{boxedBool.GetType()}");
            Console.ReadLine();
        }
    }
}
