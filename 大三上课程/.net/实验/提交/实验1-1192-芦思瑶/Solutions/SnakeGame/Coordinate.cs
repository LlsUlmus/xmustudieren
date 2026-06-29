using System;

namespace SnakeGame.Models
{
    /// <summary>
    /// 自定义坐标类（解决Point序列化异常问题）
    /// </summary>
    [Serializable] // 允许JSON序列化
    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        // 无参构造函数（JSON反序列化必需）
        public Coordinate() { }

        // 带参构造函数（初始化坐标）
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        // 重写Equals和GetHashCode（用于检测蛇身包含和食物碰撞）
        public override bool Equals(object obj)
        {
            return obj is Coordinate coordinate &&
                   X == coordinate.X &&
                   Y == coordinate.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}