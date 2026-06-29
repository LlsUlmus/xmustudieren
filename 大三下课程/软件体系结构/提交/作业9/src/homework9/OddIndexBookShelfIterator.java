package homework9;

/**
 * 具体迭代器（定制）：仅访问下标为 1,3,5,... 的书架槽位（0 起始下标中的奇数下标）。
 * 若书架长度为 n，则最多访问 floor(n/2) 本书。
 */
public final class OddIndexBookShelfIterator implements BookShelfIterator {
    private final BookShelf shelf;
    private int index;

    public OddIndexBookShelfIterator(BookShelf shelf) {
        this.shelf = shelf;
        int len = shelf.getLength();
        // 不足两本时，将起始下标设为 length，使 hasNext 为 false
        this.index = len >= 2 ? 1 : len;
    }

    @Override
    public boolean hasNext() {
        return index < shelf.getLength() && index >= 0;
    }

    @Override
    public Book next() {
        if (!hasNext()) {
            throw new IllegalStateException("没有下一个元素");
        }
        Book book = shelf.getBookAt(index);
        index += 2;
        return book;
    }
}
