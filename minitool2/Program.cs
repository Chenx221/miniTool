namespace minitool2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: minitool2 <file.arc>");
                return;
            }

            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine("文件不存在");
                Console.ReadKey();
                return;
            }

            if (Path.GetExtension(filePath).ToLower() != ".arc")
            {
                Console.WriteLine("文件后缀不正确");
                Console.ReadKey();
                return;
            }

            byte[] fileHeader = "@ARCH000"u8.ToArray();
            byte[] unityFsHeader = [0x55, 0x6E, 0x69, 0x74, 0x79, 0x46, 0x53, 0x00, 0x00, 0x00, 0x00];

            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fileHeader.Length];
            fs.Read(buffer, 0, buffer.Length);

            for (int i = 0; i < fileHeader.Length; i++)
            {
                if (buffer[i] != fileHeader[i])
                {
                    Console.WriteLine("文件头不符合规则");
                    Console.ReadKey();
                    return;
                }
            }

            List<long> positions = [];
            long position = 0;
            int bytesRead;
            buffer = new byte[unityFsHeader.Length];

            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) == buffer.Length)
            {
                bool match = true;
                for (int j = 0; j < unityFsHeader.Length; j++)
                {
                    if (buffer[j] != unityFsHeader[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    positions.Add(position);
                }
                position += 1;
                fs.Seek(position, SeekOrigin.Begin);
            }

            if (positions.Count == 0)
            {
                Console.WriteLine("未找到任何UnityFS块");
                Console.ReadKey();
                return;
            }

            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
            Directory.CreateDirectory(outputDir);

            for (int i = 0; i < positions.Count; i++)
            {
                long start = positions[i];
                long end = (i + 1 < positions.Count) ? positions[i + 1] : fs.Length;
                long length = end - start;

                fs.Seek(start, SeekOrigin.Begin);
                buffer = new byte[length];
                fs.Read(buffer, 0, buffer.Length);

                string outputFilePath = Path.Combine(outputDir, $"{(i + 1).ToString("D5")}.assets");
                File.WriteAllBytes(outputFilePath, buffer);

                Console.Write($"\r进度: {((i + 1) * 100 / positions.Count)}%");
            }

            Console.WriteLine("提取完成");
            Console.ReadKey();
        }
    }
}
