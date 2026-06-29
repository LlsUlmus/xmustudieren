package homework9;

/**
 * 聚合中的元素：图书。
 */
public final class Book {
    private final String title;

    public Book(String title) {
        if (title == null) {
            throw new IllegalArgumentException("title 不能为 null");
        }
        this.title = title;
    }

    public String getTitle() {
        return title;
    }

    @Override
    public String toString() {
        return title;
    }
}
