package ui.linux;

import ui.Button;

public class LinuxButton implements Button {
    @Override
    public void render() {
        System.out.println("[Linux] 渲染 GTK 风格按钮");
    }

    @Override
    public void onClick() {
        System.out.println("[Linux] 按钮点击：终端输出事件日志");
    }
}
