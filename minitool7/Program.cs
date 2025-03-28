namespace minitool7
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string inputPath = args[0];

            PRead pRead = new PRead(inputPath);

            string directory = Path.GetDirectoryName(inputPath);
            string outputPath = Path.Combine(directory, "output");
            Directory.CreateDirectory(outputPath);

            foreach (var item in pRead.ti)
            {
                string filename = item.Key;
                byte[] data = pRead.Data(filename);

                if (data != null)
                {
                    string fullPath = Path.Combine(outputPath, filename);
                    string fileDir = Path.GetDirectoryName(fullPath);
                    Directory.CreateDirectory(fileDir);
                    File.WriteAllBytes(fullPath, data);

                    Console.WriteLine($"已解密: {filename}");
                }

            }
            Console.WriteLine($"导出完毕");
            Console.ReadKey();
        }
    }
}
