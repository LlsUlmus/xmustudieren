namespace System.IO
{
    public static class StreamExtensions
    {
        /// <summary>
        /// 将字符串写入文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="str"></param>
        public static void WriteString(this Stream stream, string str)
        {
            byte[] bytes = str.GetBytes();
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 从文件流中读取内容
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReadToEnd(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static StreamReader GetReader(this Stream stream)
        {
            return stream.GetReader(Encoding.UTF8);
        }

        public static StreamReader GetReader(this Stream stream, Encoding encoding)
        {
            if (!stream.CanRead)
            {
                throw new InvalidOperationException("Stream does not support reading.");
            }

            encoding ??= Encoding.Default;
            return new StreamReader(stream, encoding);
        }

        public static StreamWriter GetWriter(this Stream stream)
        {
            return stream.GetWriter(Encoding.UTF8);
        }

        public static StreamWriter GetWriter(this Stream stream, Encoding encoding)
        {
            if (!stream.CanWrite)
            {
                throw new InvalidOperationException("Stream does not support writing.");
            }

            encoding ??= Encoding.Default;
            return new StreamWriter(stream, encoding);
        }

        public static Stream SeekToBegin(this Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream does not support seeking.");
            }

            stream.Seek(0L, SeekOrigin.Begin);
            return stream;
        }

        public static Stream SeekToEnd(this Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream does not support seeking.");
            }

            stream.Seek(0L, SeekOrigin.End);
            return stream;
        }

        public static Stream CopyTo(this Stream stream, Stream targetStream)
        {
            return CopyTo(stream, targetStream, 4096);
        }

        public static Stream CopyTo(this Stream stream, Stream targetStream, int bufferSize)
        {
            if (!stream.CanRead)
            {
                throw new InvalidOperationException("来源流不支持读取。");
            }

            if (!targetStream.CanWrite)
            {
                throw new InvalidOperationException("目标流不支持写入。");
            }

            byte[] buffer = new byte[bufferSize];
            int count;
            while ((count = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                targetStream.Write(buffer, 0, count);
            }

            return stream;
        }

        public static MemoryStream CopyToMemory(this Stream stream)
        {
            MemoryStream memoryStream = new MemoryStream((int)stream.Length);
            stream.SeekToBegin();
            CopyTo(stream, memoryStream);
            memoryStream.SeekToBegin();
            return memoryStream;
        }

        public static byte[] ReadAllBytes(this Stream stream)
        {
            using MemoryStream memoryStream = stream.CopyToMemory();
            return memoryStream.ToArray();
        }

        public static byte[] ReadFixedBuffersize(this Stream stream, int bufsize)
        {
            byte[] array = new byte[bufsize];
            int num = 0;
            do
            {
                int num2 = stream.Read(array, num, bufsize - num);
                if (num2 == 0)
                {
                    return [];
                }

                num += num2;
            }
            while (num < bufsize);
            return array;
        }

        public static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
