package ui.linux;

import ui.TextField;

public class LinuxTextField implements TextField {
    private String text = "";

    @Override
    public void render() {
        System.out.println("[Linux] 渲染 GTK 文本输入框，当前内容: \"" + text + "\"");
    }

    @Override
    public void setText(String text) {
        this.text = text;
        System.out.println("[Linux] 文本框已更新");
    }
}
