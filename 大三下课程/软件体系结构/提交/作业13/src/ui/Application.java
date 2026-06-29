package ui;


public class Application {
    private final Button button;
    private final TextField textField;

    public Application(GUIFactory factory) {
        this.button = factory.createButton();
        this.textField = factory.createTextField();
    }

    public void paintLoginForm() {
        System.out.println("--- 登录界面 ---");
        textField.setText("请输入用户名");
        textField.render();
        button.render();
        button.onClick();
        System.out.println();
    }
}
