namespace minitool9
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || !File.Exists(args[0]))
            {
                PauseAndExit("Please drag and drop the target program onto this patch tool to run it.");
                return;
            }

            string filePath = args[0];
            string backupPath = filePath + ".bak";

            string patternString = "CC 55 8B EC 6A FF 68 ?? ?? ?? ?? 64 A1 ?? ?? ?? ?? 50 81 EC ?? ?? ?? ?? A1 ?? ?? ?? ?? 33 C5 89 45 F0 56 57 50 8D 45 F4 64 A3 00 00 00 00 C7 45 FC 00 00 00 00";
            string patternStringOld = "CC 55 8B EC 6A FF 68 ?? ?? ?? ?? 64 A1 ?? ?? ?? ?? 50 81 EC ?? ?? ?? ?? A1 ?? ?? ?? ?? 33 C5 89 45 F0 56 57 50 8D 45 F4 64 A3 00 00 00 00 33 C0";

            byte[] patchBytes = [0xCC, 0xB0, 0x01, 0xC2, 0x18, 0x00];

            ParsePattern(patternString, out byte[] pattern, out string mask);
            ParsePattern(patternStringOld, out byte[] pattern2, out string mask2);

            byte[] fileData = File.ReadAllBytes(filePath);

            int offset = FindPattern(fileData, pattern, mask);
            if (offset == -1)
            {
                offset = FindPattern(fileData, pattern2, mask2);
                if (offset == -1)
                {
                    PauseAndExit("Pattern not found.");
                    return;
                }
            }
            Console.WriteLine($"Pattern found: at offset 0x{offset:X}");

            // Try to create a backup before patching
            if (!CreateBackup(filePath, backupPath))
            {
                PauseAndExit("Failed to create backup file. Please check file permissions or disk space.");
                return;
            }

            if (offset + patchBytes.Length > fileData.Length)
            {
                PauseAndExit("Patch offset out of file bounds.");
            }

            ApplyPatch(filePath, offset, patchBytes);
            Console.WriteLine($"Patch successfully applied to {filePath}");
            Console.WriteLine($"Modified {patchBytes.Length} bytes at offset 0x{offset:X}");
            Console.WriteLine($"Backup file created: {backupPath}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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

        public static void PauseAndExit(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}