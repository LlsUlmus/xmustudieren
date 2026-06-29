import java.util.concurrent.CountDownLatch;

/**
 * 并发拉取单例：若 DCL 正确，所有线程打印的 identity hash 应一致。
 */
public class SingletonTest {

    private static final int THREAD_COUNT = 8;

    public static void main(String[] args) throws InterruptedException {
        CountDownLatch ready = new CountDownLatch(THREAD_COUNT);
        CountDownLatch startGate = new CountDownLatch(1);
        Thread[] workers = new Thread[THREAD_COUNT];

        for (int i = 0; i < THREAD_COUNT; i++) {
            final int index = i + 1;
            workers[i] = new Thread(() -> {
                ready.countDown();
                try {
                    startGate.await();
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                    return;
                }
                Singleton one = Singleton.getInstance();
                System.out.printf("[%s] hash=%s%n",
                        Thread.currentThread().getName(),
                        Integer.toHexString(System.identityHashCode(one)));
            }, "T-" + index);
        }

        for (Thread t : workers) {
            t.start();
        }
        ready.await();
        startGate.countDown();

        for (Thread t : workers) {
            t.join();
        }
    }
}
