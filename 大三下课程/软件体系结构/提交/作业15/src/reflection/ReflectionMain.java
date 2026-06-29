package reflection;

public class ReflectionMain {

    public static void main(String[] args) throws Exception {
        Person original = new Person("张三", 20);
        System.out.println("原始对象: " + original);

        ReflectionUtil.setProperty(original, "name", "李四");
        ReflectionUtil.setProperty(original, "age", "25");
        System.out.println("setProperty 后: " + original);

        Person copy = ReflectionUtil.shallowCopy(original);
        ReflectionUtil.setProperty(copy, "name", "王五");
        System.out.println("浅拷贝副本修改 name 后:");
        System.out.println("  original = " + original);
        System.out.println("  copy     = " + copy);
    }
}
