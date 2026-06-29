package ui.windows;

import ui.Button;
import ui.GUIFactory;
import ui.TextField;


public class WindowsFactory implements GUIFactory {
    @Override
    public Button createButton() {
        return new WindowsButton();
    }

    @Override
    public TextField createTextField() {
        return new WindowsTextField();
    }
}
