#include "widget.h"
#include "ui_widget.h"

/**
 * @brief Widget构造函数 - TCP客户端初始化
 * 核心功能：创建QTcpSocket，连接readyRead信号到readMessage槽（接收数据），连接error信号到displayError槽（错误处理）
 */
Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
{
    ui->setupUi(this);
    tcpSocket = new QTcpSocket(this);
    
    // 数据到达时自动调用readMessage()
    connect(tcpSocket,SIGNAL(readyRead()),this,SLOT(readMessage()));
    
    // 网络错误时调用displayError()
    connect(tcpSocket,SIGNAL(error(QAbstractSocket::SocketError)),
             this,SLOT(displayError(QAbstractSocket::SocketError)));
    
    connect(ui->pushButton,SIGNAL(clicked()),this,SLOT(pushButton_clicked()));
}

Widget::~Widget()
{
    delete ui;
}

/**
 * @brief newConnect函数 - 建立与服务器的连接
 * 核心功能：从界面获取IP和端口，连接到服务器
 */
void Widget::newConnect()
{
    blockSize = 0; 
    tcpSocket->abort(); 
    tcpSocket->connectToHost(ui->hostLineEdit->text(),
                             ui->portLineEdit->text().toInt());
}

/**
 * @brief readMessage槽函数 - 接收并处理服务器发送的数据
 * 核心功能：先读取2字节长度信息，等待完整数据包到达后读取实际数据并显示
 * 数据包格式：[2字节长度信息][实际文本数据]
 */
void Widget::readMessage()
{
    QDataStream in(tcpSocket);
    in.setVersion(QDataStream::Qt_4_6);
    
    // 首次接收：读取2字节长度信息
    if(blockSize==0) 
    {
       if(tcpSocket->bytesAvailable() < (int)sizeof(quint16)) return;
       in >> blockSize;
    }
    
    // 等待完整数据包到达
    if(tcpSocket->bytesAvailable() < blockSize) return;
    
    // 读取实际数据并显示
    in >> message;
    ui->messageLabel->setText(message);
}

/**
 * @brief displayError槽函数 - 显示网络错误信息
 */
void Widget::displayError(QAbstractSocket::SocketError)
{
    qDebug() << "error"<<tcpSocket->errorString(); 
}

void Widget::pushButton_clicked()
{
    newConnect(); 
}
