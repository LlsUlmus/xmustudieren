package ui.windows;

import ui.TextField;

public class WindowsTextField implements TextField {
    private String text = "";

    @Override
    public void render() {
        System.out.println("[Windows] 渲染带边框文本框，当前内容: \"" + text + "\"");
    }

    @Override
    public void setText(String text) {
        this.text = text;
        System.out.println("[Windows] 文本框已更新");
    }
}
