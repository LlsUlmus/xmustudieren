package homework9;

import java.util.ArrayList;
import java.util.List;

/**
 * 聚合角色：内部用 ArrayList 存放 Book（任务一：不用数组）。
 */
public class BookShelf {
    private final List<Book> books = new ArrayList<>();

    public void addBook(Book book) {
        books.add(book);
    }

    public Book getBookAt(int index) {
        return books.get(index);
    }

    public int getLength() {
        return books.size();
    }

    /**
     * 工厂方法：默认正向迭代器。
     */
    public BookShelfIterator iterator() {
        return new ForwardBookShelfIterator(this);
    }

    /**
     * 任务二：定制迭代器——只遍历下标为奇数的位置（1,3,5,...），
     * 与常见“从头到尾”或“倒序”不同，体现同一聚合多种遍历策略。
     */
    public BookShelfIterator oddIndexIterator() {
        return new OddIndexBookShelfIterator(this);
    }
}
