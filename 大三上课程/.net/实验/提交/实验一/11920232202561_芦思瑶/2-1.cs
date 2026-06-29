using System;
namespace CSharpStructAndEnum
{
    // 目标1：使用结构体，构造一个二维向量
    struct Vector2D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }
        // 目标2：实现向量的相加，相减，标量乘法，向量积和相除
        // 相加
        public static Vector2D Add(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X + v2.X, v1.Y + v2.Y);
        }
        // 相减
        public static Vector2D Subtract(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.X - v2.X, v1.Y - v2.Y);
        }
        // 标量乘法
        public static Vector2D ScalarMultiply(Vector2D v, double scalar)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }
        // 向量积（二维向量叉积结果为标量，大小为|v1.X*v2.Y - v1.Y*v2.X|）
        public static double CrossProduct(Vector2D v1, Vector2D v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }
        // 相除
        public static Vector2D Divide(Vector2D v, double divisor)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException("除数不能为0");
            }
            return new Vector2D(v.X / divisor, v.Y / divisor);
        }
        // 目标3：为相加，相减，标量乘法和相除提供运算符重载
        public static Vector2D operator +(Vector2D v1, Vector2D v2)
        {
            return Add(v1, v2);
        }
        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return Subtract(v1, v2);
        }
        public static Vector2D operator *(Vector2D v, double scalar)
        {
            return ScalarMultiply(v, scalar);
        }
        public static Vector2D operator *(double scalar, Vector2D v)
        {
            return ScalarMultiply(v, scalar);
        }
        public static Vector2D operator /(Vector2D v, double divisor)
        {
            return Divide(v, divisor);
        }
        // 重写ToString方法，便于输出向量信息
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Vector2D v1 = new Vector2D(2, 3);
            Vector2D v2 = new Vector2D(4, 5);
            // 测试向量相加
            Vector2D addResult = v1 + v2;
            Console.WriteLine($"向量{v1} + 向量{v2} = {addResult}");
            // 测试向量相减
            Vector2D subtractResult = v1 - v2;
            Console.WriteLine($"向量{v1} - 向量{v2} = {subtractResult}");
            // 测试标量乘法
            double scalar = 2;
            Vector2D multiplyResult1 = v1 * scalar;
            Vector2D multiplyResult2 = scalar * v1;
            Console.WriteLine($"向量{v1} * {scalar} = {multiplyResult1}");
            Console.WriteLine($"{scalar} * 向量{v1} = {multiplyResult2}");
            // 测试向量积
            double crossProductResult = Vector2D.CrossProduct(v1, v2);
            Console.WriteLine($"向量{v1} 和 向量{v2}的向量积 = {crossProductResult}");
            // 测试相除
            double divisor = 2;
            try
            {
                Vector2D divideResult = v1 / divisor;
                Console.WriteLine($"向量{v1} / {divisor} = {divideResult}");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
