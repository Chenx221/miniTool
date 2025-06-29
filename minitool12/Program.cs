using System.Reflection;

namespace minitool12
{
    internal class Program
    {
        /**
         * 
         * 2B D5 8D ?? ?? 0F B6 ?? ?? 3B CB 75 ?? 0F B6 ?? ?? ?? ?? ?? ?? 0F B6 ?? ?? ?? 3B ?? 75 ??
         *                                  90 90                                              90 90
         * 0F B6 ?? ?? ?? 3B F8 B8 00 00 00 00 0F 94 C0
         *                39 C0
         */
        const string patternString1 = "2B D5 8D ?? ?? 0F B6 ?? ?? 3B CB 75 ?? 0F B6 ?? ?? ?? ?? ?? ?? 0F B6 ?? ?? ?? 3B ?? 75 ??";
        const string patternString2 = "0F B6 ?? ?? ?? 3B F8 B8 00 00 00 00 0F 94 C0";

        static void Main(string[] args)
        {
            if (args.Length < 1 || !File.Exists(args[0]))
            {
                PauseAndExit("Please drag and drop the target program onto this patch tool to run it.");
            }

            string filePath = args[0];
            string backupPath = filePath + ".bak";

            // Parse Pattern
            ParsePattern(patternString1, out byte[] pattern1, out string mask1);
            ParsePattern(patternString2, out byte[] pattern2, out string mask2);

            // Search for the pattern in the file
            byte[] fileData = File.ReadAllBytes(filePath);

            int offset1 = FindPattern(fileData, pattern1, mask1);
            int offset2 = FindPattern(fileData, pattern2, mask2);
            if (offset1 == -1 || offset2 == -1)
            {
                PauseAndExit("Pattern not found.");
            }
            Console.WriteLine($"Pattern found: at offset 0x{offset1:X}, 0x{offset2:X}");

            // Create backup
            if (!CreateBackup(filePath, backupPath))
            {
                PauseAndExit("Failed to create backup file. Please check file permissions or disk space.");
            }

            // Apply the patch
            SaveFile(ApplyPatch(ApplyPatch(ApplyPatch(fileData, offset1 + 0xB, [0x90, 0x90]), offset1 + 0x1C, [0x90, 0x90]), offset2 + 0x5, [0x39, 0xC0]), args[0]);

            // Copy extra file
            ExtractResourceFile("minitool12.yshpd.dat", Path.Combine(Path.GetDirectoryName(args[0]), "yshpd.dat"));

            Console.WriteLine($"Patch successfully applied to {filePath}");
            Console.WriteLine($"You may need to enter the Serial: BBBBBBBBBBBBBBB when launching for the first time.");
            PauseAndExit($"Backup file created: {backupPath}");
        }

        public static void ParsePattern(string patternStr, out byte[] pattern, out string mask)
        {
            var bytes = new List<byte>();
            var maskBuilder = new System.Text.StringBuilder();

            string[] tokens = patternStr.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                if (token == "?" || token == "??")
                {
                    bytes.Add(0x00);
                    maskBuilder.Append('?');
                }
                else
                {
                    if (byte.TryParse(token, System.Globalization.NumberStyles.HexNumber, null, out byte b))
                    {
                        bytes.Add(b);
                        maskBuilder.Append('x');
                    }
                    else
                    {
                        throw new FormatException($"Invalid token: {token}");
                    }
                }
            }

            pattern = [.. bytes];
            mask = maskBuilder.ToString();
        }

        public static int FindPattern(byte[] data, byte[] pattern, string mask)
        {
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                if (MatchPattern(data, i, pattern, mask))
                    return i;
            }
            return -1;
        }

        public static bool MatchPattern(byte[] data, int offset, byte[] pattern, string mask)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (offset + i >= data.Length)
                    return false;
                if (mask[i] == 'x' && data[offset + i] != pattern[i])
                    return false;
            }
            return true;
        }

        public static bool CreateBackup(string sourceFilePath, string backupFilePath)
        {
            try
            {
                if (File.Exists(backupFilePath))
                    File.Delete(backupFilePath);

                File.Copy(sourceFilePath, backupFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create backup file: {ex.Message}");
                return false;
            }
        }

        public static void ApplyPatch(string filePath, int offset, byte[] patchBytes)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.ReadWrite);
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(patchBytes, 0, patchBytes.Length);
                fs.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to apply patch: {ex.Message}");
                throw;
            }
        }

        public static byte[] ApplyPatch(byte[] data, int offset, byte[] patchBytes)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(data);

                ArgumentNullException.ThrowIfNull(patchBytes);

                if (offset < 0 || offset + patchBytes.Length > data.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset), "Patch would extend beyond the bounds of the data array.");

                byte[] result = (byte[])data.Clone();

                Buffer.BlockCopy(patchBytes, 0, result, offset, patchBytes.Length);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to apply patch to byte array: {ex.Message}");
                throw;
            }
        }

        public static bool SaveFile(byte[] data, string filePath)
        {
            try
            {
                File.WriteAllBytes(filePath, data);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save file: {ex.Message}");
                return false;
            }
        }

        public static void ExtractResourceFile(string resourceName, string outputPath)
        {
            try
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Console.WriteLine($"Resource not found: {resourceName}");
                    return;
                }

                using var fileStream = File.Create(outputPath);
                stream.CopyTo(fileStream);
                Console.WriteLine($"Resource file extracted to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract resource file: {ex.Message}");
            }
        }

        public static void PauseAndExit(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}