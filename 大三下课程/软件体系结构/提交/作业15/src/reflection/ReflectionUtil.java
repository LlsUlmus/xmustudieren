package reflection;

import java.lang.reflect.Field;

public final class ReflectionUtil {

    private ReflectionUtil() {
    }

    /**
     * 通过反射为任意对象的指定字段赋值（含 private 字段）。
     */
    public static void setProperty(Object target, String propertyName, Object value)
            throws ReflectiveOperationException {
        if (target == null) {
            throw new IllegalArgumentException("target 不能为 null");
        }
        Field field = findField(target.getClass(), propertyName);
        field.setAccessible(true);
        Object converted = convertValue(field.getType(), value);
        field.set(target, converted);
    }

    /**
     * 浅拷贝：创建同类型新实例，并复制所有声明字段的值。
     */
    @SuppressWarnings("unchecked")
    public static <T> T shallowCopy(T source) throws ReflectiveOperationException {
        if (source == null) {
            return null;
        }
        Class<?> clazz = source.getClass();
        T target = (T) clazz.getDeclaredConstructor().newInstance();
        copyDeclaredFields(source, target, clazz);
        return target;
    }

    private static void copyDeclaredFields(Object source, Object target, Class<?> clazz)
            throws IllegalAccessException {
        for (Field field : clazz.getDeclaredFields()) {
            field.setAccessible(true);
            field.set(target, field.get(source));
        }
        Class<?> superClass = clazz.getSuperclass();
        if (superClass != null && superClass != Object.class) {
            copyDeclaredFields(source, target, superClass);
        }
    }

    private static Field findField(Class<?> clazz, String name) throws NoSuchFieldException {
        Class<?> current = clazz;
        while (current != null && current != Object.class) {
            try {
                return current.getDeclaredField(name);
            } catch (NoSuchFieldException ignored) {
                current = current.getSuperclass();
            }
        }
        throw new NoSuchFieldException(name);
    }

    private static Object convertValue(Class<?> fieldType, Object value) {
        if (value == null || fieldType.isInstance(value)) {
            return value;
        }
        if (fieldType == int.class || fieldType == Integer.class) {
            return Integer.parseInt(value.toString());
        }
        if (fieldType == long.class || fieldType == Long.class) {
            return Long.parseLong(value.toString());
        }
        if (fieldType == double.class || fieldType == Double.class) {
            return Double.parseDouble(value.toString());
        }
        if (fieldType == boolean.class || fieldType == Boolean.class) {
            return Boolean.parseBoolean(value.toString());
        }
        return value;
    }
}
