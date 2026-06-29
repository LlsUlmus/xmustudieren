using System;
using System.Collections.Generic;
using System.Drawing;

namespace SnakeGame.Models
{
    public class SnakeGameModel
    {
        public enum Direction { Up, Down, Left, Right }

        // 关键修改：替换Point为Coordinate
        private List<Coordinate> _snakeBody;
        private Coordinate _foodPosition;
        private Size _gridSize;
        private Size _containerSize;
        private Direction _currentDirection;
        private bool _isPaused;
        private Random _random;
        private int _score;

        // 属性访问器：返回Coordinate类型
        public List<Coordinate> SnakeBody => _snakeBody;
        public Coordinate FoodPosition => _foodPosition;
        public Size GridSize => _gridSize;
        public bool IsPaused => _isPaused;
        public int Score => _score;

        public SnakeGameModel(Size gridSize, Size containerSize)
        {
            _gridSize = gridSize;
            _containerSize = containerSize;
            _random = new Random();
            _currentDirection = Direction.Right;
            _isPaused = false;
            _score = 0;
            InitializeGame();
        }

        // 初始化游戏：蛇身初始位置用Coordinate
        private void InitializeGame()
        {
            _snakeBody = new List<Coordinate>
            {
                new Coordinate(5, 5),  // 蛇头
                new Coordinate(4, 5),  // 蛇身1
                new Coordinate(3, 5)   // 蛇身2
            };
            GenerateFood();
        }

        // 生成食物：用Coordinate
        private void GenerateFood()
        {
            int maxX = _containerSize.Width / _gridSize.Width;
            int maxY = _containerSize.Height / _gridSize.Height;
            Coordinate newFood;

            do
            {
                newFood = new Coordinate(_random.Next(0, maxX), _random.Next(0, maxY));
            } while (SnakeBodyContains(newFood));

            _foodPosition = newFood;
        }

        // 检查坐标是否在蛇身上（用Coordinate的Equals方法）
        private bool SnakeBodyContains(Coordinate point)
        {
            return _snakeBody.Contains(point);
        }

        // 移动蛇：所有坐标操作改用Coordinate
        public bool MoveSnake()
        {
            if (_isPaused) return true;

            Coordinate head = _snakeBody[0];
            Coordinate newHead = new Coordinate(head.X, head.Y); // 复制当前蛇头坐标

            // 根据方向更新新蛇头
            switch (_currentDirection)
            {
                case Direction.Up:
                    newHead.Y--;
                    break;
                case Direction.Down:
                    newHead.Y++;
                    break;
                case Direction.Left:
                    newHead.X--;
                    break;
                case Direction.Right:
                    newHead.X++;
                    break;
            }

            // 边界碰撞检测
            if (newHead.X < 0 || newHead.X >= _containerSize.Width / _gridSize.Width ||
                newHead.Y < 0 || newHead.Y >= _containerSize.Height / _gridSize.Height)
            {
                return false;
            }

            // 自身碰撞检测
            if (SnakeBodyContains(newHead))
            {
                return false;
            }

            // 添加新蛇头
            _snakeBody.Insert(0, newHead);

            // 吃食物检测（坐标对比用Equals）
            if (newHead.Equals(_foodPosition))
            {
                _score += 10;
                GenerateFood();
            }
            else
            {
                // 未吃食物，删除尾节
                _snakeBody.RemoveAt(_snakeBody.Count - 1);
            }

            return true;
        }

        // 改变方向（逻辑不变）
        public void ChangeDirection(Direction direction)
        {
            if ((_currentDirection == Direction.Up && direction == Direction.Down) ||
                (_currentDirection == Direction.Down && direction == Direction.Up) ||
                (_currentDirection == Direction.Left && direction == Direction.Right) ||
                (_currentDirection == Direction.Right && direction == Direction.Left))
            {
                return;
            }
            _currentDirection = direction;
        }

        // 切换暂停/恢复（逻辑不变）
        public void TogglePause()
        {
            _isPaused = !_isPaused;
        }

        // 重置游戏（逻辑不变）
        public void ResetGame()
        {
            _score = 0;
            _currentDirection = Direction.Right;
            _isPaused = false;
            InitializeGame();
        }
    }
}