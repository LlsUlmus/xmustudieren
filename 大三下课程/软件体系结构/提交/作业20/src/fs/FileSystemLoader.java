package fs;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.stream.Stream;

/**
 * 将本机真实目录结构转换为组合模式对象树。
 */
public final class FileSystemLoader {

    private FileSystemLoader() {
    }

    public static Directory load(Path rootDir, int maxDepth) throws IOException {
        if (!Files.isDirectory(rootDir)) {
            throw new IllegalArgumentException("路径不是目录: " + rootDir);
        }
        Directory root = new Directory(rootDir.getFileName().toString());
        loadChildren(rootDir, root, 0, maxDepth);
        return root;
    }

    private static void loadChildren(Path dirPath, Directory parent, int depth, int maxDepth)
            throws IOException {
        if (depth >= maxDepth) {
            return;
        }
        try (Stream<Path> stream = Files.list(dirPath)) {
            stream.sorted(Comparator
                            .comparing((Path p) -> !Files.isDirectory(p))
                            .thenComparing(p -> p.getFileName().toString().toLowerCase()))
                    .forEach(child -> {
                        try {
                            if (Files.isDirectory(child)) {
                                Directory sub = new Directory(child.getFileName().toString());
                                parent.add(sub);
                                loadChildren(child, sub, depth + 1, maxDepth);
                            } else if (Files.isRegularFile(child)) {
                                long size = Files.size(child);
                                parent.add(new FsFile(child.getFileName().toString(), size));
                            }
                        } catch (IOException ignored) {
                            // 无权限或临时文件不可读时跳过
                        }
                    });
        }
    }
}
