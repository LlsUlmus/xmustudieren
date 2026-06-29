package homework9;

/**
 * 具体迭代器：从前往后依次访问。
 */
public final class ForwardBookShelfIterator implements BookShelfIterator {
    private final BookShelf shelf;
    private int index;

    public ForwardBookShelfIterator(BookShelf shelf) {
        this.shelf = shelf;
        this.index = 0;
    }

    @Override
    public boolean hasNext() {
        return index < shelf.getLength();
    }

    @Override
    public Book next() {
        if (!hasNext()) {
            throw new IllegalStateException("没有下一个元素");
        }
        Book book = shelf.getBookAt(index);
        index++;
        return book;
    }
}
