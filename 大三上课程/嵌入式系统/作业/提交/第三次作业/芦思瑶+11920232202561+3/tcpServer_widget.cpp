#include "widget.h"
#include "ui_widget.h"

/**
 * @brief Widget构造函数 - TCP服务器端初始化
 * 核心功能：创建QTcpServer，监听6666端口，连接newConnection信号到sendMessage槽
 */
Widget::Widget(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::Widget)
{
    ui->setupUi(this);
    tcpServer = new QTcpServer(this);
    
    // 监听本地主机6666端口
    if(!tcpServer->listen(QHostAddress::LocalHost,6666))
    {  
        qDebug() << "error" <<tcpServer->errorString();
        close();
    }
    
    // 新客户端连接时自动调用sendMessage()
    connect(tcpServer,SIGNAL(newConnection()),this,SLOT(sendMessage()));
}

Widget::~Widget()
{
    delete ui;
}

/**
 * @brief sendMessage槽函数 - 发送消息给客户端
 * 核心功能：将界面文本打包成数据包[2字节长度+实际数据]，发送给客户端后断开连接
 */
void Widget::sendMessage()
{
    QByteArray block;
    QDataStream out(&block,QIODevice::WriteOnly);
    out.setVersion(QDataStream::Qt_4_6);
    
    // 先写入占位符（2字节），稍后替换为实际长度
    out<<(quint16) 0;
    qDebug()<<ui->textEdit_send->toPlainText();
    
    // 写入实际文本数据
    out<<ui->textEdit_send->toPlainText();
    
    // 回到开头，写入实际数据长度
    out.device()->seek(0);
    out<<(quint16) (block.size() - sizeof(quint16));

    // 获取客户端连接并发送数据
    QTcpSocket *clientConnection = tcpServer->nextPendingConnection();
    connect(clientConnection,SIGNAL(disconnected()),clientConnection,
           SLOT(deleteLater()));
    clientConnection->write(block);
    clientConnection->disconnectFromHost();

    ui->statusLabel->setText("send message successful!!!");
    qDebug() <<"send message successful!!!";
}
