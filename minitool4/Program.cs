using System.Security.Cryptography;
using ZstdSharp;

namespace minitool4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: DecryptApp <file1> <file2> ...");
                Console.WriteLine("Provide one or more file paths to decrypt.");
                return;
            }

            foreach (string filePath in args)
            {
                Console.WriteLine($"Processing file: {filePath}");

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: File not found - {filePath}");
                    continue;
                }

                try
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    if (fileBytes.Length < 48)
                    {
                        Console.WriteLine($"Error: File is too small to contain valid data - {filePath}");
                        continue;
                    }

                    byte[] iv = fileBytes[..16]; // IV
                    byte[] key = fileBytes[16..48]; // Key
                    byte[] encryptedData = fileBytes[48..]; //ENC Data

                    byte[] decryptedData = DecryptAesCbc(encryptedData, key, iv);
                    using var decompressor = new Decompressor();
                    byte[] decompressedData = decompressor.Unwrap(decryptedData.AsSpan(4)).ToArray();

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    string fileExtension = Path.GetExtension(filePath);
                    string outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileNameWithoutExtension}_dec{fileExtension}");

                    File.WriteAllBytes(outputFilePath, decompressedData);
                    Console.WriteLine($"Decryption successful! Output saved to: {outputFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error decrypting file {filePath}: {ex.Message}");
                }
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static byte[] DecryptAesCbc(byte[] data, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
}
