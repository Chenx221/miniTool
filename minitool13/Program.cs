using System.ComponentModel;

namespace minitool13
{
    internal class Program
    {
        static readonly byte[] JammingData256 = [
            0x56, 0x91, 0xBC, 0xDF, 0x11, 0xB3, 0x6F, 0xF6, 0x15, 0xD2, 0x40, 0x39, 0x18, 0x4F, 0x0E, 0x9C,
            0x05, 0xB2, 0x17, 0xD8, 0x93, 0x03, 0x81, 0xFE, 0x5F, 0xC4, 0x24, 0xCF, 0x4D, 0xC5, 0x45, 0xBE,
            0x52, 0x74, 0xBD, 0xAD, 0x0B, 0xA6, 0x60, 0x94, 0x57, 0x27, 0x2D, 0xE2, 0x6A, 0x55, 0x96, 0xEF,
            0x69, 0x41, 0xE1, 0x6C, 0x62, 0xA5, 0xF7, 0x80, 0xBF, 0x1B, 0x79, 0xFB, 0xCA, 0x5B, 0x21, 0xB1,
            0x8F, 0x04, 0xFC, 0x37, 0x3F, 0x0C, 0x86, 0x8A, 0x65, 0x77, 0xB7, 0xAA, 0xFD, 0x7A, 0x34, 0xF0,
            0x97, 0xF5, 0x6D, 0x2C, 0xD5, 0x48, 0x66, 0x3C, 0x9D, 0xB0, 0xD1, 0xA9, 0x58, 0x78, 0xD7, 0x6E,
            0xE3, 0x51, 0x87, 0xAC, 0x63, 0xD4, 0xE9, 0x47, 0x23, 0xE8, 0x36, 0x7D, 0xE5, 0xF2, 0xC3, 0x5C,
            0x84, 0x88, 0x76, 0x1A, 0xA4, 0xC0, 0xF3, 0x31, 0x99, 0x9B, 0x6B, 0x2E, 0x98, 0xEE, 0x4B, 0x5A,
            0x44, 0xC7, 0xF8, 0xC1, 0xB5, 0x53, 0xA2, 0x32, 0xC9, 0xBA, 0x7B, 0x1E, 0x10, 0xA3, 0x00, 0x01,
            0x14, 0x4A, 0x08, 0x0D, 0x46, 0x95, 0x8C, 0xB6, 0x8D, 0xB8, 0xFF, 0x75, 0x4C, 0xB4, 0x64, 0xDE,
            0x85, 0xA7, 0x30, 0x43, 0x1F, 0xDB, 0x7E, 0x50, 0x26, 0x35, 0x20, 0x92, 0xCE, 0x9A, 0x9E, 0xEB,
            0xF4, 0x1C, 0xF9, 0x33, 0x38, 0xDD, 0x29, 0xF1, 0x3A, 0x2F, 0x5E, 0x73, 0xD3, 0xE6, 0x54, 0xB9,
            0x68, 0xCD, 0xAF, 0x06, 0xD9, 0xD0, 0x61, 0x83, 0x8E, 0xC6, 0xE7, 0x7F, 0xDA, 0xAB, 0x12, 0x0F,
            0x89, 0xBB, 0xD6, 0x3B, 0xC2, 0xA1, 0x3D, 0x72, 0x70, 0x19, 0xCB, 0xA8, 0x22, 0x2A, 0x59, 0x4E,
            0x90, 0x28, 0x07, 0x71, 0x13, 0xA0, 0x5D, 0xCC, 0x7C, 0xED, 0xFA, 0x16, 0x9F, 0xEC, 0x09, 0x67,
            0x02, 0xEA, 0x25, 0x82, 0x3E, 0x49, 0xAE, 0x0A, 0x42, 0xE0, 0x8B, 0x1D, 0xDC, 0x2B, 0xE4, 0xC8
        ];

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("请拖拽文件夹到程序上");
                Console.ReadKey();
                return;
            }
            if(!Directory.Exists(args[0]))
            {
                Console.WriteLine("文件夹不存在");
                Console.ReadKey();
                return;
            }

            string[] files = Directory.GetFiles(args[0], "*.*", SearchOption.AllDirectories);

            string decryptedFolder = Path.Combine(args[0], "Decrypted");
            if (!Directory.Exists(decryptedFolder))
            {
                Directory.CreateDirectory(decryptedFolder);
            }

            foreach (string file in files)
            {
                string relativePath = Path.GetRelativePath(args[0], file);
                string decryptedFilePath = Path.Combine(decryptedFolder, relativePath);
                string decryptedFileDir = Path.GetDirectoryName(decryptedFilePath);
                if (!Directory.Exists(decryptedFileDir))
                {
                    Directory.CreateDirectory(decryptedFileDir);
                }
                byte[] fileData = File.ReadAllBytes(file);
                if (DecryptWithKnownHeader(fileData))
                {
                    File.WriteAllBytes(decryptedFilePath, fileData);
                    Console.WriteLine($"解密成功: {file} -> {decryptedFilePath}");
                }
                else
                {
                    Console.WriteLine($"解密失败: {file}");
                }
            }
        }

        static ulong GetKey(string filename)
        {
            byte[] nameBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (i < filename.Length)
                {
                    nameBytes[i] = (byte)filename[filename.Length - 1 - i];
                    nameBytes[i] += (byte)(i * 4);
                }
                else
                {
                    throw new ArgumentException("异常, 文件名长度不足4位");
                }
            }
            uint[] uints = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                uints[i] = BitConverter.ToUInt32(JammingData256, nameBytes[i]);
            }
            // 这里还有几个步骤
            uint xorResult = uints[0] ^ uints[1] ^ uints[2] ^ uints[3];
            ulong key = (0xFFFFFFFFUL << 32) | xorResult;

            return key;
        }

        // 解密
        static bool Decrypt(byte[] data, ulong key)
        {
            if (data == null || data.Length == 0)
                return false;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)((key >> (8 * (i & 7))) & 0xFF);
            }

            return true;
        }

        static bool DecryptWithKnownHeader(byte[] data)
        {
            if (data == null || data.Length < 8)
                return false;

            // 已知明文头 8 字节
            byte[] knownPlainHeader = new byte[] { 0x55, 0x6E, 0x69, 0x74, 0x79, 0x46, 0x53, 0x00 };

            // 计算 key 的 8 个字节
            ulong key = 0;
            for (int i = 0; i < 8; i++)
            {
                byte keyByte = (byte)(data[i] ^ knownPlainHeader[i]);
                key |= ((ulong)keyByte << (8 * i));
            }

            // 用计算出的 key 完全解密 data
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)((key >> (8 * (i & 7))) & 0xFF);
            }

            return true;
        }

    }
}
