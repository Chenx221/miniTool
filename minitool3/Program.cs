using System.Security.Cryptography;
using System.Text;

namespace minitool3
{
    internal class Program
    {
        private const string DefaultSecretKey = "dB3aqcLtAmBd";
        private const string DefaultKeyBase = "RWd3NusabzRc";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please drag and drop one or more files onto this program.");
                Console.ReadKey();
                return;
            }

            Byte[] SHA256Byte = GenerateSHA256Byte(DefaultKeyBase, Encoding.UTF8.GetBytes(DefaultSecretKey));

            foreach (var filePath in args)
            {
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"Processing file: {filePath}");
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(filePath);

                        if (fileData.Length < 16)
                        {
                            Console.WriteLine("The file does not contain enough data.");
                            continue;
                        }

                        // Read the encrypted data & iv size (4 bytes)*
                        int encryptedSize = BitConverter.ToInt32(fileData, 0);
                        int sp = 4;
                        if (fileData.Length - 4 != encryptedSize) //*某些情况下头部不带长度
                        {
                            sp = 0;
                            encryptedSize = fileData.Length;
                        }
                        encryptedSize -= 16;

                        // Read the IV (16 bytes)
                        byte[] iv = new byte[16];
                        Array.Copy(fileData, sp, iv, 0, 16);

                        // Read the ciphertext based on the size
                        byte[] cipherText = new byte[encryptedSize];
                        Array.Copy(fileData, 16 + sp, cipherText, 0, encryptedSize);

                        byte[] decryptedData = Decrypt(cipherText, SHA256Byte, iv);

                        string outputDir = Path.Combine(Path.GetDirectoryName(filePath)!, "dec");
                        Directory.CreateDirectory(outputDir); // Ensure the directory exists
                        string outputFilePath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath) + ".assets");
                        File.WriteAllBytes(outputFilePath, decryptedData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"File not found: {filePath}");
                }
            }

            Console.WriteLine("Processing complete. Press any key to exit.");
            Console.ReadKey();
        }

        public static byte[] GenerateSHA256Byte(string data, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        public static byte[] Decrypt(byte[] cipherText, byte[] sha256Key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = sha256Key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            //PaddingMode.None;
            aes.Padding = PaddingMode.Zeros;

            using var decryptor = aes.CreateDecryptor();
            using var memoryStream = new MemoryStream(cipherText);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            byte[] decryptedData = new byte[cipherText.Length];
            int bytesRead = cryptoStream.Read(decryptedData, 0, decryptedData.Length);

            return [.. decryptedData];
        }
    }
}
