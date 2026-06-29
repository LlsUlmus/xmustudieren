package com.lab.jprofiler;

/**
 * 模拟线程阻塞：两个线程以相反顺序获取锁，形成经典死锁。
 * 在 JProfiler 的 Threads / Thread History 中可看到 BLOCKED 状态及锁依赖链。
 */
public class ThreadBlockDemo {

    private static final Object LOCK_A = new Object();
    private static final Object LOCK_B = new Object();

    public static void run() throws InterruptedException {
        System.out.println("启动两个线程，将在数秒后发生死锁...");

        Thread thread1 = new Thread(() -> {
            synchronized (LOCK_A) {
                sleepQuietly(500);
                synchronized (LOCK_B) {
                    System.out.println("线程1 获得 LOCK_A 和 LOCK_B");
                }
            }
        }, "Worker-Thread-1");

        Thread thread2 = new Thread(() -> {
            synchronized (LOCK_B) {
                sleepQuietly(500);
                synchronized (LOCK_A) {
                    System.out.println("线程2 获得 LOCK_B 和 LOCK_A");
                }
            }
        }, "Worker-Thread-2");

        thread1.start();
        thread2.start();

        Thread.sleep(3000);
        System.out.println("线程1 状态: " + thread1.getState());
        System.out.println("线程2 状态: " + thread2.getState());
        System.out.println("若两者均为 BLOCKED，说明发生死锁。可在 JProfiler 中查看 Monitor & Locks。");
    }

    private static void sleepQuietly(long millis) {
        try {
            Thread.sleep(millis);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}
