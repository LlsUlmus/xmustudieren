package ui.linux;

import ui.Button;
import ui.GUIFactory;
import ui.TextField;

/**
 * 具体工厂 3：Linux 风格产品族（对应课件手写 Linux Button/Text 示例）
 */
public class LinuxFactory implements GUIFactory {
    @Override
    public Button createButton() {
        return new LinuxButton();
    }

    @Override
    public TextField createTextField() {
        return new LinuxTextField();
    }
}
