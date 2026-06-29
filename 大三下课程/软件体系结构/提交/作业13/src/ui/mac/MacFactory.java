package ui.mac;

import ui.Button;
import ui.GUIFactory;
import ui.TextField;

/**
 * 具体工厂 2：macOS 风格产品族
 */
public class MacFactory implements GUIFactory {
    @Override
    public Button createButton() {
        return new MacButton();
    }

    @Override
    public TextField createTextField() {
        return new MacTextField();
    }
}
