package factory;

import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;
import ui.GUIFactory;

public final class GUIFactoryProvider {
    private static final Properties CONFIG = new Properties();

    static {
        try (InputStream in = GUIFactoryProvider.class.getClassLoader()
                .getResourceAsStream("gui.properties")) {
            if (in == null) {
                throw new IllegalStateException("找不到 gui.properties");
            }
            CONFIG.load(in);
        } catch (IOException e) {
            throw new ExceptionInInitializerError(e);
        }
    }

    private GUIFactoryProvider() {
    }

    public static GUIFactory getFactory(String platformKey) {
        String className = CONFIG.getProperty(platformKey);
        if (className == null || className.isBlank()) {
            throw new IllegalArgumentException("未知平台: " + platformKey);
        }
        try {
            Class<?> clazz = Class.forName(className);
            Object instance = clazz.getDeclaredConstructor().newInstance();
            return (GUIFactory) instance;
        } catch (ReflectiveOperationException e) {
            throw new IllegalStateException("无法创建工厂: " + className, e);
        }
    }
}
