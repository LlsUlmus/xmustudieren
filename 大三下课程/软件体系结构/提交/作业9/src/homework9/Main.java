package homework9;

/**
 * 演示：任务一（ArrayList 聚合）+ 任务二（奇数下标定制迭代器）。
 */
public final class Main {
    private Main() {
    }

    public static void main(String[] args) {
        BookShelf shelf = new BookShelf();
        shelf.addBook(new Book("深入理解计算机系统"));
        shelf.addBook(new Book("设计模式：可复用面向对象软件的基础"));
        shelf.addBook(new Book("Java 并发编程实战"));
        shelf.addBook(new Book("重构：改善既有代码的设计"));
        shelf.addBook(new Book("Effective Java（第3版）"));

        System.out.println("=== 默认正向迭代（ArrayList 作为内部存储）===");
        BookShelfIterator forward = shelf.iterator();
        while (forward.hasNext()) {
            System.out.println(forward.next().getTitle());
        }

        System.out.println();
        System.out.println("=== 定制迭代：仅奇数下标（第 2、4、6… 本）===");
        BookShelfIterator odd = shelf.oddIndexIterator();
        while (odd.hasNext()) {
            System.out.println(odd.next().getTitle());
        }
    }
}
