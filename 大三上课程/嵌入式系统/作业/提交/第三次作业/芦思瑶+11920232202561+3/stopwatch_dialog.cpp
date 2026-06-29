#include "dialog.h"
#include "ui_dialog.h"

/**
 * @brief Dialog构造函数 - 秒表主窗口初始化
 * 核心功能：连接开始/停止按钮的clicked信号到对应的槽函数
 */
Dialog::Dialog(QWidget *parent)
    : QDialog(parent)
    , ui(new Ui::Dialog)
{
    ui->setupUi(this);
    connect(ui->pushButton_start,SIGNAL(clicked()),this,SLOT(startSlot()));
    connect(ui->pushButton_stop,SIGNAL(clicked()),this,SLOT(stopSlot()));
}

Dialog::~Dialog()
{
    delete ui;
}

/**
 * @brief startSlot槽函数 - 启动秒表
 * 核心功能：创建Timerr对象（秒表窗口），显示并启动计时
 */
void Dialog::startSlot()
{
    timer = new Timerr(this);
    timer->show();
    timer->start();
}

/**
 * @brief stopSlot槽函数 - 停止秒表
 * 核心功能：调用Timerr的stop()方法停止定时器
 */
void Dialog::stopSlot()
{
    timer->stop();
}
