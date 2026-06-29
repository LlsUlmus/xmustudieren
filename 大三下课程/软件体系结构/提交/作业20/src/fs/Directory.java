package fs;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * 组合模式中的 Composite：目录可包含文件或其它目录。
 */
public class Directory extends Entry {

    private final String name;
    private final List<Entry> children = new ArrayList<>();

    public Directory(String name) {
        this.name = name;
    }

    @Override
    public String getName() {
        return name;
    }

    @Override
    public int getSize() {
        long total = 0;
        for (Entry child : children) {
            if (child instanceof FsFile file) {
                total += file.getSizeLong();
            } else if (child instanceof Directory dir) {
                total += dir.getSizeLong();
            } else {
                total += child.getSize();
            }
            if (total > Integer.MAX_VALUE) {
                return Integer.MAX_VALUE;
            }
        }
        return (int) total;
    }

    public long getSizeLong() {
        long total = 0;
        for (Entry child : children) {
            if (child instanceof FsFile file) {
                total += file.getSizeLong();
            } else if (child instanceof Directory dir) {
                total += dir.getSizeLong();
            } else {
                total += child.getSize();
            }
        }
        return total;
    }

    @Override
    public Directory add(Entry entry) {
        children.add(entry);
        return this;
    }

    public List<Entry> getChildren() {
        return Collections.unmodifiableList(children);
    }

    @Override
    protected void printList(String prefix) {
        System.out.println(prefix + "/" + this);
        String childPrefix = prefix.isEmpty() ? name : prefix + "/" + name;
        for (Entry child : children) {
            child.printList(childPrefix);
        }
    }

    public boolean isDirectory() {
        return true;
    }
}
