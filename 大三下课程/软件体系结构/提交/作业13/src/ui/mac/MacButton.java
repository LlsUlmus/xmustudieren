package ui.mac;

import ui.Button;

public class MacButton implements Button {
    @Override
    public void render() {
        System.out.println("[macOS] 渲染圆角渐变按钮");
    }

    @Override
    public void onClick() {
        System.out.println("[macOS] 按钮点击：轻微弹性动画反馈");
    }
}
