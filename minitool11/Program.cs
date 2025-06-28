namespace minitool11
{
    internal class Program
    {
        enum KeyByte
        {
            Original = 0x75,
            Patch = 0xEB
        }

        const string patternString = "FF 15 ?? ?? ?? ?? 83 C4 ?? 8D 8D ?? ?? ?? ?? 85 C0";

        static void Main(string[] args)
        {
            if (args.Length < 1 || !File.Exists(args[0]))
            {
                PauseAndExit("Please drag and drop the target program onto this patch tool to run it.");
            }

            string filePath = args[0];
            string backupPath = filePath + ".bak";

            // Parse Pattern
            ParsePattern(patternString, out byte[] pattern, out string mask);

            // Search for the pattern in the file
            byte[] fileData = File.ReadAllBytes(filePath);

            int offset = FindPattern(fileData, pattern, mask);
            if (offset == -1)
            {
                PauseAndExit("Pattern not found.");
            }
            Console.WriteLine($"Pattern found: at offset 0x{offset:X}");

            // Check if the patch has already been applied
            int pos = offset + pattern.Length;
            if (pos >= fileData.Length)
            {
                PauseAndExit("Patch offset out of file bounds.");
            }
            byte b = fileData[pos];

            if (b == (byte)KeyByte.Patch)
            {
                PauseAndExit("The patch has already been applied. No changes made.");
            }
            else if (b != (byte)KeyByte.Original)
            {
                PauseAndExit($"Unexpected byte at offset 0x{pos:X}: expected {KeyByte.Patch}, found {b}. Exiting.");
            }

            // Create backup
            if (!CreateBackup(filePath, backupPath))
            {
                PauseAndExit("Failed to create backup file. Please check file permissions or disk space.");
            }

            // Apply the patch
            ApplyPatch(filePath, pos, [(byte)KeyByte.Patch]);
            Console.WriteLine($"Patch successfully applied to {filePath}");
            Console.WriteLine($"Modified 1 bytes at offset 0x{pos:X}");
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

        public static void PauseAndExit(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}