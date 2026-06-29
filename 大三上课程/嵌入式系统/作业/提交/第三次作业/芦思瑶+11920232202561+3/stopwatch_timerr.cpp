#include "timerr.h"
#include "ui_timerr.h"

/**
 * @brief Timerr构造函数 - 秒表窗口初始化
 * 核心功能：初始化UI界面
 */
Timerr::Timerr(QWidget *parent) :
    QDialog(parent),
    ui(new Ui::Timerr)
{
    ui->setupUi(this);
}

Timerr::~Timerr()
{
    delete ui;
}

/**
 * @brief start函数 - 启动秒表计时
 * 核心功能：创建QTimer定时器（间隔10ms），连接timeout信号到timeUpdateSlot槽，启动定时器
 */
void Timerr::start()
{
    timer = new QTimer(this);
    timer->setSingleShot(false);
    timer->start(10);
    connect(timer,SIGNAL(timeout()),this,SLOT(timeUpdateSlot()));
    timeUpdateSlot();
    ui->lcdNumber->show();
}

/**
 * @brief stop函数 - 停止秒表计时
 * 核心功能：停止定时器
 */
void Timerr::stop()
{
    timer->stop();
}

/**
 * @brief timeUpdateSlot槽函数 - 更新时间显示
 * 核心功能：每10ms更新一次，timerBegin加1（十分之一秒），当timerBegin=10时ms加1，当ms=10时s加1
 * 时间格式：秒:毫秒.十分之一秒（例如：5:3.7表示5.37秒）
 */
void Timerr::timeUpdateSlot()
{
    // 格式化时间字符串：秒:毫秒.十分之一秒
    time =  QString::number(s).append(":").append(QString::number(ms))
            .append(QString::number(timerBegin++));
    
    // 十分之一秒计数器达到10时，毫秒计数器加1
    if(timerBegin == 10)
    {
       ms++;
      timerBegin=0;
    }
    
    // 毫秒计数器达到10时，秒计数器加1
    if(ms ==10)
    {
        s++;
        ms =0;
        timerBegin=0;
    }
    
    // 秒计数器达到60时归零
    if(s==60)
    {
        s=0;
        timerBegin =0;
    }
    
    ui->lcdNumber->display(time);
}
