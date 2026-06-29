/**
 * 线程安全单例（双重检查锁定）。
 * volatile：新建完成后对其他线程可见，并约束与构造相关的重排序。
 * synchronized：同一时刻只有一个线程执行初始化。
 */
public class Singleton {

    /** 惰性初始化时的共享引用，必须由 volatile 修饰。 */
    private static volatile Singleton instance;

    /** 仅用于初始化互斥，避免占用类字面量锁名空间。 */
    private static final Object INIT_LOCK = new Object();

    private Singleton() {}

    public static Singleton getInstance() {
        if (instance == null) {
            synchronized (INIT_LOCK) {
                if (instance == null) {
                    instance = new Singleton();
                }
            }
        }
        return instance;
    }
}
