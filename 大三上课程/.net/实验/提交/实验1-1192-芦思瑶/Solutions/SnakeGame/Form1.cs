using System;
using System.Drawing;
using System.Windows.Forms;
using SnakeGame.Models;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        private SnakeGameModel _gameModel;
        private System.Windows.Forms.Timer _gameTimer;
        private bool _isTickEventBound = false;
        private Bitmap _gameBitmap;

        public Form1()
        {
            InitializeComponent();
            _gameModel = new SnakeGameModel(new Size(20, 20), new Size(600, 400));
            _gameTimer = new System.Windows.Forms.Timer { Interval = 200 };

            // 初始化游戏画布
            _gameBitmap = new Bitmap(gameCanvas.Width, gameCanvas.Height);
            gameCanvas.Image = _gameBitmap;

            // 绑定定时器事件
            if (!_isTickEventBound)
            {
                _gameTimer.Tick += GameTimer_Tick;
                _isTickEventBound = true;
            }

            this.Text = "贪吃蛇游戏（.NET版）";
            this.Size = new Size(660, 550); // 调整窗体尺寸匹配内容
            btnStart.Click += BtnStart_Click;
            btnPause.Click += BtnPause_Click;
            btnReset.Click += BtnReset_Click;

            // 初始渲染
            RenderGame();
        }

        /// <summary>
        /// 渲染游戏画面
        /// </summary>
        private void RenderGame()
        {
            if (_gameBitmap == null) return;

            using (Graphics g = Graphics.FromImage(_gameBitmap))
            {
                // 清空画布
                g.Clear(Color.Black);

                // 绘制网格边框
                g.DrawRectangle(Pens.Gray, 0, 0, _gameBitmap.Width - 1, _gameBitmap.Height - 1);

                // 绘制蛇身
                foreach (var segment in _gameModel.SnakeBody)
                {
                    int x = segment.X * _gameModel.GridSize.Width;
                    int y = segment.Y * _gameModel.GridSize.Height;
                    g.FillRectangle(Brushes.Lime, x, y, _gameModel.GridSize.Width - 1, _gameModel.GridSize.Height - 1);
                }

                // 绘制食物
                var food = _gameModel.FoodPosition;
                int foodX = food.X * _gameModel.GridSize.Width;
                int foodY = food.Y * _gameModel.GridSize.Height;
                g.FillEllipse(Brushes.Red, foodX, foodY, _gameModel.GridSize.Width - 1, _gameModel.GridSize.Height - 1);
            }

            // 刷新PictureBox显示
            gameCanvas.Invalidate();
        }


        /// <summary>
        /// 游戏定时器事件：移动蛇并更新UI
        /// </summary>
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_gameModel.IsPaused) return;

            bool moveSuccess = _gameModel.MoveSnake();
            if (!moveSuccess)
            {
                _gameTimer.Stop();
                MessageBox.Show($"游戏结束！得分：{_gameModel.Score}", "游戏结束", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnStart.Text = "重新开始";
                return;
            }

            lblScore.Text = $"当前得分：{_gameModel.Score}";
            
            // 渲染游戏画面
            RenderGame();
        }


        // 开始游戏按钮事件
        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!_gameTimer.Enabled)
            {
                _gameTimer.Start();
                btnStart.Text = "继续游戏";
            }
        }

        // 暂停游戏按钮事件
        private void BtnPause_Click(object sender, EventArgs e)
        {
            _gameModel.TogglePause();
            btnPause.Text = _gameModel.IsPaused ? "恢复游戏" : "暂停游戏";
            if (_gameModel.IsPaused)
            {
                _gameTimer.Stop();
            }
            else
            {
                _gameTimer.Start();
            }
        }

        // 重置游戏按钮事件
        private void BtnReset_Click(object sender, EventArgs e)
        {
            _gameTimer.Stop();
            _gameModel.ResetGame();
            lblScore.Text = $"当前得分：{_gameModel.Score}";
            btnStart.Text = "开始游戏";
            btnPause.Text = "暂停游戏";
            
            // 渲染初始游戏画面
            RenderGame();
        }

        // 键盘控制方向
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_gameModel.IsPaused) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Up:
                    _gameModel.ChangeDirection(SnakeGameModel.Direction.Up);
                    return true;
                case Keys.Down:
                    _gameModel.ChangeDirection(SnakeGameModel.Direction.Down);
                    return true;
                case Keys.Left:
                    _gameModel.ChangeDirection(SnakeGameModel.Direction.Left);
                    return true;
                case Keys.Right:
                    _gameModel.ChangeDirection(SnakeGameModel.Direction.Right);
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // 窗体关闭事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _gameTimer?.Dispose();
            _gameBitmap?.Dispose();
        }
    }
}
