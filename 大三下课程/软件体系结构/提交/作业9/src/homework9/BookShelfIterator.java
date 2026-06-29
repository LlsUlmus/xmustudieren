package homework9;

/**
 * 迭代器角色（GoF）：与具体集合解耦的遍历抽象。
 * 使用泛型 Book，避免返回 Object。
 */
public interface BookShelfIterator {
    boolean hasNext();

    Book next();
}
