import factory.GUIFactoryProvider;
import ui.Application;
import ui.GUIFactory;


public class Client {
    public static void main(String[] args) {
        System.out.println("========== 抽象工厂模式：跨平台 GUI ==========\n");

        String[] platforms = {"windows", "mac", "linux"};
        for (String platform : platforms) {
            System.out.println(">>> 当前平台配置: " + platform);
            GUIFactory factory = GUIFactoryProvider.getFactory(platform);
            Application app = new Application(factory);
            app.paintLoginForm();
        }

        System.out.println("========== 演示结束 ==========");
    }
}
