using minitool8.third;

namespace minitool8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the file path as an argument.");
                return;
            }

            foreach (string pacPath in args)
            {
                ProcessFile(pacPath);
            }
        }

        private static void ProcessFile(string pacPath)
        {
            APF<string> apf = new(pacPath);

            if (apf.isInitialized)
            {
                string directory = Path.GetDirectoryName(pacPath);
                string fileName = Path.GetFileNameWithoutExtension(pacPath);
                string outputDirectory = Path.Combine(directory, "output", fileName);
                Directory.CreateDirectory(outputDirectory);

                foreach (var entry in apf.HeaderDict)
                {
                    var file = entry.Key;
                    var header = entry.Value;
                    long startByte = header.readStartBytePos;
                    long byteLength = header.ByteLength;

                    Console.WriteLine($"Processing file: {file}, Start Byte: 0x{startByte:X}, Length: 0x{byteLength:X}");

                    string outputPath = Path.Combine(outputDirectory, file);
                    try
                    {
                        ExtractData(pacPath, startByte, byteLength, outputPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error extracting data for {fileName}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("APF initialization failed.");
            }
        }

        static void ExtractData(string filePath, long startByte, long byteLength, string outputPath)
        {
            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(startByte, SeekOrigin.Begin);
                byte[] data = new byte[byteLength];
                fs.Read(data, 0, (int)byteLength);
                File.WriteAllBytes(outputPath, data);
            }
        }
    }
}
