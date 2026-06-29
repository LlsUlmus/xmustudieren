namespace System.Text.Json.Nodes
{
    public static class JsonNodeExtensions
    {
        public static T GetValue<T>(this JsonNode node, string path, T defaultValue)
        {
            TryGetValue(node, path, default, out T? value);
            return value ?? defaultValue;
        }

        public static T GetValue<T>(this JsonNode node, string path)
            where T : new()
        {
            TryGetValue(node, path, default, out T? value);
            return value ?? new T();
        }

        public static bool TryGetValue<T>(this JsonNode node, string path, T defaultValue, out T? value)
        {
            string[] arry = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (arry.Length == 0)
            {
                value = defaultValue;
                return false;
            }

            int index = 0;
            int length = arry.Length - 1;
            JsonNode? currentNode = node;
            foreach (var item in arry)
            {
                if (index == 0 && item == "$")
                {
                    index++;
                    continue;
                }
                if (index != 0 && item == "$") throw new InvalidOperationException($"开始标记指示符$必须出现在最开始");

                if (currentNode is JsonArray ary)
                {
                    if (!int.TryParse(item, out int itemIndex))
                    {
                        throw new InvalidOperationException($"当前Json属性是一个数组，所以，输入值必须是索引");
                    }

                    currentNode = ary.Count < itemIndex + 1 ? null : ary[itemIndex];
                }
                else
                {
                    currentNode = currentNode[item];
                }

                if (currentNode == null)
                {
                    value = defaultValue;
                    return false;
                }

                if (index == length)
                {
                    try
                    {
                        value = currentNode.GetValue<T>();
                        return true;
                    }
                    catch
                    {
                        try
                        {
                            value = currentNode.Deserialize<T>();
                            return true;
                        }
                        catch
                        {
                            value = defaultValue;
                            return false;
                        }
                    }
                }

                index++;
            }

            value = defaultValue;
            return false;
        }
    }
}
