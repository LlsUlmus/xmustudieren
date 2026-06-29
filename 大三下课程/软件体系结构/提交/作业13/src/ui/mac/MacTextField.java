package ui.mac;

import ui.TextField;

public class MacTextField implements TextField {
    private String text = "";

    @Override
    public void render() {
        System.out.println("[macOS] 渲染圆角无边框文本框，当前内容: \"" + text + "\"");
    }

    @Override
    public void setText(String text) {
        this.text = text;
        System.out.println("[macOS] 文本框已更新");
    }
}
