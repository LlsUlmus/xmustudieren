package gui;

import fs.Directory;
import fs.Entry;
import fs.FsFile;

import javax.swing.event.TreeModelListener;
import javax.swing.tree.TreeModel;
import javax.swing.tree.TreePath;

/**
 * 将组合模式 Entry 树绑定到 JTree。
 */
public class EntryTreeModel implements TreeModel {

    private final Entry root;

    public EntryTreeModel(Entry root) {
        this.root = root;
    }

    @Override
    public Object getRoot() {
        return root;
    }

    @Override
    public Object getChild(Object parent, int index) {
        if (parent instanceof Directory dir) {
            return dir.getChildren().get(index);
        }
        return null;
    }

    @Override
    public int getChildCount(Object parent) {
        if (parent instanceof Directory dir) {
            return dir.getChildren().size();
        }
        return 0;
    }

    @Override
    public boolean isLeaf(Object node) {
        return node instanceof FsFile;
    }

    @Override
    public void valueForPathChanged(TreePath path, Object newValue) {
        // 只读浏览
    }

    @Override
    public int getIndexOfChild(Object parent, Object child) {
        if (parent instanceof Directory dir) {
            return dir.getChildren().indexOf(child);
        }
        return -1;
    }

    @Override
    public void addTreeModelListener(TreeModelListener l) {
    }

    @Override
    public void removeTreeModelListener(TreeModelListener l) {
    }
}
