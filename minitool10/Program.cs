using System.ComponentModel;

namespace minitool10
{
    internal class Program
    {
        public class GameFileInfo
        {
            public string Path { get; set; } = string.Empty;
            public int FileSize { get; set; } = 0;
            public long FileOffset { get; set; } = 0;
        }

        static void Main(string[] args)
        {
            //4B magic(45 58 46 53)
            //4B uint compatibleReaderVersion
            //4B uint writerVersion
            //4B uint fileCount
            //8B long headerSize
            //8B long entryListSize 基于bodyListOffset的偏移量
            //8B long pathListSize
            //8B long bodyListOffset 
            //...

            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided. Please provide a command.");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"File not found: {args[0]}");
                return;
            }

            try
            {
                using FileStream fileStream = new(args[0], FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new(fileStream);

                // 读取并验证魔法头 EXFS (45 58 46 53)
                byte[] magicBytes = reader.ReadBytes(4);
                if (BitConverter.ToInt32(magicBytes, 0) != 0x53465845) // EXFS 以小端字节序表示
                {
                    Console.WriteLine("无效的文件格式。应为EXFS魔法头");
                    return;
                }

                // 读取文件头信息
                uint compatibleReaderVersion = reader.ReadUInt32();
                uint writerVersion = reader.ReadUInt32();
                uint fileCount = reader.ReadUInt32();
                long headerSize = reader.ReadInt64();
                long entryListSize = reader.ReadInt64();
                long pathListSize = reader.ReadInt64();
                long bodyListOffset = reader.ReadInt64();

                Console.WriteLine($"文件总数: {fileCount}");

                // 调整到头部结束位置
                fileStream.Position = headerSize;

                // 读取路径列表数据（跳过条目列表）
                fileStream.Position = headerSize + entryListSize;
                byte[] pathListData = reader.ReadBytes((int)pathListSize);
                string paths = System.Text.Encoding.UTF8.GetString(pathListData);

                // 回到头部结束位置读取文件条目
                fileStream.Position = headerSize;

                // 使用更好的容量初始化
                List<GameFileInfo> fileInfos = new((int)fileCount);

                // 读取文件条目信息
                for (int i = 0; i < fileCount; i++)
                {
                    long filepathOffset = reader.ReadInt64();
                    long filepathSize = reader.ReadInt64();
                    long filebodyOffset = reader.ReadInt64();
                    long filebodySize = reader.ReadInt64();

                    fileInfos.Add(new GameFileInfo
                    {
                        Path = paths.Substring((int)filepathOffset, (int)filepathSize),
                        FileOffset = filebodyOffset + bodyListOffset,
                        FileSize = (int)filebodySize
                    });
                }

                // 创建输出目录
                string outputDir = Path.Combine(Path.GetDirectoryName(args[0]) ?? "", "extracted");
                Directory.CreateDirectory(outputDir);

                // 创建进度条
                int processedCount = 0;
                int lastProgressPercentage = -1;

                foreach (var fileInfo in fileInfos)
                {
                    try
                    {
                        // 创建目标文件的目录
                        string outputFilePath = Path.Combine(outputDir, fileInfo.Path);
                        string? outputFileDir = Path.GetDirectoryName(outputFilePath);
                        if (!string.IsNullOrEmpty(outputFileDir))
                        {
                            Directory.CreateDirectory(outputFileDir);
                        }

                        // 读取文件数据
                        fileStream.Position = fileInfo.FileOffset;
                        byte[] fileData = reader.ReadBytes(fileInfo.FileSize);

                        // 写入目标文件
                        File.WriteAllBytes(outputFilePath, fileData);

                        // 更新进度并显示
                        processedCount++;
                        int currentProgress = (int)((processedCount * 100.0) / fileCount);

                        if (currentProgress != lastProgressPercentage)
                        {
                            lastProgressPercentage = currentProgress;
                            if (currentProgress % 10 == 0)
                            {
                                Console.WriteLine($"进度: {currentProgress}% - 已处理: {processedCount}/{fileCount} 文件");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"提取文件 {fileInfo.Path} 时出错: {ex.Message}");
                    }
                }

                Console.WriteLine($"完成! 共提取 {processedCount}/{fileCount} 个文件到 {outputDir} 目录");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"读取文件错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"意外错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}