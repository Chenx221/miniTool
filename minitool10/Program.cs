using System.ComponentModel;

namespace minitool10
{
    internal class Program
    {
        public class GameFileInfo
        {
            public string path { get; set; } = string.Empty;
            public int fileSize { get; set; } = 0;
            public long fileOffset { get; set; } = 0;
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
                using (FileStream fileStream = new(args[0], FileMode.Open, FileAccess.Read))
                {
                    // 读取4字节魔法头 EXFS (45 58 46 53)
                    byte[] magicBytes = new byte[4];
                    fileStream.Read(magicBytes, 0, 4);
                    if (magicBytes[0] != 0x45 || magicBytes[1] != 0x58 || magicBytes[2] != 0x46 || magicBytes[3] != 0x53)
                    {
                        Console.WriteLine("无效的文件格式。应为EXFS魔法头");
                        return;
                    }

                    // 读取compatibleReaderVersion (4字节)
                    byte[] versionBytes = new byte[4];
                    fileStream.Read(versionBytes, 0, 4);
                    uint compatibleReaderVersion = BitConverter.ToUInt32(versionBytes, 0);

                    byte[] writerVersionBytes = new byte[4];
                    fileStream.Read(writerVersionBytes, 0, 4);
                    uint writerVersion = BitConverter.ToUInt32(writerVersionBytes, 0);

                    byte[] fileCountBytes = new byte[4];
                    fileStream.Read(fileCountBytes, 0, 4);
                    uint fileCount = BitConverter.ToUInt32(fileCountBytes, 0);

                    byte[] headerSizeBytes = new byte[8];
                    fileStream.Read(headerSizeBytes, 0, 8);
                    long headerSize = BitConverter.ToInt64(headerSizeBytes, 0);

                    byte[] entryListSizeBytes = new byte[8];
                    fileStream.Read(entryListSizeBytes, 0, 8);
                    long entryListSize = BitConverter.ToInt64(entryListSizeBytes, 0);

                    byte[] pathListSizeBytes = new byte[8];
                    fileStream.Read(pathListSizeBytes, 0, 8);
                    long pathListSize = BitConverter.ToInt64(pathListSizeBytes, 0);

                    byte[] bodyListOffsetBytes = new byte[8];
                    fileStream.Read(bodyListOffsetBytes, 0, 8);
                    long bodyListOffset = BitConverter.ToInt64(bodyListOffsetBytes, 0);

                    // adjust point to header end
                    fileStream.Seek(headerSize, SeekOrigin.Begin);

                    // get entryList data after header
                    byte[] entryListData = new byte[entryListSize];
                    fileStream.Read(entryListData, 0, (int)entryListSize); // 没逝的 游戏封包没那么大

                    // get pathList data after entryList
                    byte[] pathListData = new byte[pathListSize];
                    fileStream.Read(pathListData, 0, (int)pathListSize);
                    string paths = System.Text.Encoding.UTF8.GetString(pathListData);

                    fileStream.Seek(headerSize, SeekOrigin.Begin);
                    List<GameFileInfo> fileInfos = [];
                    for (int i = 0; i < fileCount; i++)
                    {
                        byte[] filepathOffsetData = new byte[8];
                        fileStream.Read(filepathOffsetData, 0, 8);
                        byte[] filepathSizeData = new byte[8];
                        fileStream.Read(filepathSizeData, 0, 8);
                        byte[] filebodyOffsetData = new byte[8];
                        fileStream.Read(filebodyOffsetData, 0, 8);
                        byte[] filebodySizeData = new byte[8];
                        fileStream.Read(filebodySizeData, 0, 8);
                        GameFileInfo gfi = new()
                        {
                            path = paths.Substring((int)BitConverter.ToInt64(filepathOffsetData, 0), (int)BitConverter.ToInt64(filepathSizeData, 0)),
                            fileOffset = BitConverter.ToInt64(filebodyOffsetData, 0) + bodyListOffset,
                            fileSize = (int)BitConverter.ToInt64(filebodySizeData, 0)
                        };
                        fileInfos.Add(gfi);
                    }
                    Console.WriteLine($"文件总数: {fileCount}");

                    // 创建输出目录
                    string outputDir = Path.Combine(Path.GetDirectoryName(args[0]) ?? "", "extracted");
                    Directory.CreateDirectory(outputDir);

                    int processedCount = 0;
                    foreach (var fileInfo in fileInfos)
                    {
                        try
                        {
                            // 创建目标文件的目录
                            string outputFilePath = Path.Combine(outputDir, fileInfo.path);
                            string? outputFileDir = Path.GetDirectoryName(outputFilePath);
                            if (!string.IsNullOrEmpty(outputFileDir))
                            {
                                Directory.CreateDirectory(outputFileDir);
                            }

                            // 读取文件数据
                            fileStream.Seek(fileInfo.fileOffset, SeekOrigin.Begin);
                            byte[] fileData = new byte[fileInfo.fileSize];
                            fileStream.Read(fileData, 0, fileInfo.fileSize);

                            // 写入目标文件
                            using FileStream outputFileStream = new(outputFilePath, FileMode.Create, FileAccess.Write);
                            outputFileStream.Write(fileData, 0, fileInfo.fileSize);

                            processedCount++;
                            if (processedCount % 100 == 0)
                            {
                                Console.WriteLine($"已处理: {processedCount}/{fileCount} 文件");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"提取文件 {fileInfo.path} 时出错: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"完成! 共提取 {processedCount}/{fileCount} 个文件到 {outputDir} 目录");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"读取文件错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"意外错误: {ex.Message}");
            }
        }
    }
}
