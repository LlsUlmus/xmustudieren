#include "circularprogressbar.h"

/**
 * @brief Circularprogressbar构造函数 - 自定义圆形进度条控件初始化
 * 核心功能：创建QTimer定时器，连接timeout信号到decreaseColorProgress槽
 */
Circularprogressbar::Circularprogressbar(QWidget *parent)
    : QWidget(parent)
{
    myTimer = new QTimer(this);
    connect(myTimer, &QTimer::timeout, this, &Circularprogressbar::decreaseColorProgress);
}

/**
 * @brief drawBiggestCircle函数 - 绘制外圈大圆
 * 核心功能：绘制暗灰色背景圆环
 */
void Circularprogressbar::drawBiggestCircle(QPainter &painter, int radius) {
    painter.save();
    QPainterPath path;
    path.addEllipse(-radius, -radius, 2 * radius, 2 * radius);
    painter.setBrush(QColor(54, 54, 54)); 
    painter.drawPath(path);
    painter.restore();
}

/**
 * @brief drawLittleCircle函数 - 绘制内圈小圆
 * 核心功能：绘制内圈小圆（半径=外圈-50），使用窗口背景色填充，形成环形效果
 */
void Circularprogressbar::drawLittleCircle(QPainter &painter, int radius) {
    painter.save();
    QPainterPath path;
    int reducedRadius = radius - 50; 
    path.addEllipse(-reducedRadius, -reducedRadius, 2 * reducedRadius, 2 * reducedRadius);
    QColor ringColor = palette().color(QPalette::Window); 
    painter.setBrush(ringColor);
    painter.drawPath(path);
    painter.restore();
}

/**
 * @brief drawColor函数 - 绘制彩色进度环
 * 核心功能：使用圆锥渐变绘制彩虹色扇形进度条，角度由currentColorProgress控制
 */
void Circularprogressbar::drawColor(QPainter &painter, int radius)
{
    QRect rect(-radius,-radius,2*radius,2*radius);
    QConicalGradient Conical(0, 0, 0);

    // 设置彩虹色渐变（紫色→红色→橙色→绿色→青色→蓝色→紫色）
    Conical.setColorAt(0, QColor(128, 0, 255));
    Conical.setColorAt(0.05, QColor(128, 0, 255));
    Conical.setColorAt(0.2, QColor(255, 0, 0));
    Conical.setColorAt(0.4, QColor(255, 165, 0));
    Conical.setColorAt(0.6, QColor(0, 128, 0));
    Conical.setColorAt(0.8, QColor(0, 255, 255));
    Conical.setColorAt(0.95, QColor(0, 0, 255));
    Conical.setColorAt(1.0, QColor(128, 0, 255));

    painter.setBrush(Conical);
    // 从-180度（顶部）开始，逆时针绘制currentColorProgress度
    painter.drawPie(rect, -180 * 16, -(currentColorProgress * 16));
}

/**
 * @brief paintEvent函数 - 重写绘制事件处理函数
 * 核心功能：将坐标原点移到窗口中心，依次绘制外圈大圆→彩色进度环→内圈小圆
 */
void Circularprogressbar::paintEvent(QPaintEvent *event) {
    Q_UNUSED(event);
    QPainter painter(this);
    int width = this->width();
    int height = this->height();
    
    // 将坐标原点移到窗口中心
    painter.translate(width / 2, height / 2); 
    painter.setRenderHint(QPainter::Antialiasing, true);
    painter.setPen(Qt::NoPen);

    int outerRadius = std::min(width, height) / 2;
    int innerRadius = outerRadius - 50;

    drawBiggestCircle(painter, outerRadius);
    drawColor(painter, outerRadius);
    drawLittleCircle(painter, innerRadius);
}

/**
 * @brief keyPressEvent函数 - 重写键盘按下事件处理函数
 * 核心功能：按下空格键时启动定时器，设置direction=true（进度增加）
 */
void Circularprogressbar::keyPressEvent(QKeyEvent *event)
{
    if(event->key() == Qt::Key_Space)
    {
        myTimer->start();
        direction = true;
        update();
    }
}

/**
 * @brief keyReleaseEvent函数 - 重写键盘释放事件处理函数
 * 核心功能：释放空格键时设置direction=false（进度减少）
 */
void Circularprogressbar::keyReleaseEvent(QKeyEvent *event)
{
    if(event->key() == Qt::Key_Space)
    {
        direction = false;
        update();
    }
}

/**
 * @brief decreaseColorProgress槽函数 - 更新进度条进度
 * 核心功能：direction=true时进度每次增加3.6度（1%），direction=false时每次减少1度，范围0-360度
 */
void Circularprogressbar::decreaseColorProgress()
{
    if(direction){
        // 按下空格键：进度增加
        currentColorProgress += 3.6;
        if(currentColorProgress > 360){
            currentColorProgress = 360;
        }
    }else{
        // 释放空格键：进度减少
        currentColorProgress -= 1;
        if(currentColorProgress < 0){
            currentColorProgress = 0;
            myTimer->stop();
        }
    }
    update();
}

Circularprogressbar::~Circularprogressbar()
{
}
