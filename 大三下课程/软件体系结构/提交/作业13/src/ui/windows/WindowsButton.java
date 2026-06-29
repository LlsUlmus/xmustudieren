package ui.windows;

import ui.Button;

public class WindowsButton implements Button {
    @Override
    public void render() {
        System.out.println("[Windows] 渲染扁平矩形按钮");
    }

    @Override
    public void onClick() {
        System.out.println("[Windows] 按钮点击：播放系统提示音");
    }
}
