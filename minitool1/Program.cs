using K4os.Compression.LZ4;
using System.Text;

namespace minitool1
{
    public class PkgChunk
    {
        public string pkg_path;
        public long off;
        public int len_comp;
        public int len_uncomp;
    }

    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
                return;
            if (!Path.Exists(args[0]))
                return;
            string streamingAssetsPath = args[0];
            Dictionary<string, PkgChunk> file_map = [];
            LoadFileHeader(file_map, streamingAssetsPath + "/data_bg_i.arc");
            LoadFileHeader(file_map, streamingAssetsPath + "/data_st.arc");
            LoadFileHeader(file_map, streamingAssetsPath + "/data_tb.arc");
            LoadFileHeader(file_map, streamingAssetsPath + "/data_tr.arc");
            LoadFileHeader(file_map, streamingAssetsPath + "/data_cgs_real.arc");
            LoadFileHeader(file_map, streamingAssetsPath + "/data_game_sc_stand.arc");
            LoadAbFile(file_map, args[0]);
            Console.ReadKey();
        }

        private static void LoadAbFile(Dictionary<string, PkgChunk> file_map, string outputPath)
        {
            outputPath = Path.Combine(outputPath, "output");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            foreach (var kvp in file_map)
            {
                string key = kvp.Key;
                byte[] data;
                if (key.EndsWith(".co.bytes"))
                {
                    byte[] array = LoadNormalFile(file_map, key);
                    int num = 0;
                    while (num < array.Length && array[num] != 0)
                    {
                        num++;
                    }
                    string @string = Encoding.UTF8.GetString(array, 0, num);
                    byte[] array2 = LoadNormalFile(file_map, @string);
                    data = Proc_dec(array2, array);
                }
                else
                {
                    data = LoadNormalFile(file_map, key);
                }
                string newKey = key[..^".un.bytes".Length]; //.co.bytes顺带处理
                WriteFile(data, Path.Combine(outputPath, newKey + ".asset"));
            }
        }

        private static void WriteFile(byte[] data, string path)
        {
            try
            {
                File.WriteAllBytes(path, data);
                Console.WriteLine($"{Path.GetFileName(path)} extraction successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write file to {path}. Error: {ex.Message}");
            }
        }
        private static byte[] LoadNormalFile(Dictionary<string, PkgChunk> file_map, string file_name)
        {
            if (!file_map.TryGetValue(file_name, out PkgChunk? pkgChunk))
                throw new Exception($"{file_name} not found");
            int num = pkgChunk.len_uncomp;
            if (pkgChunk.len_comp != 0)
                num = pkgChunk.len_comp;
            byte[] array2 = new byte[num];
            FileStream fileStream = new(pkgChunk.pkg_path, FileMode.Open);
            fileStream.Seek(pkgChunk.off, SeekOrigin.Begin);
            fileStream.Read(array2, 0, num);
            fileStream.Close();
            if (pkgChunk.len_comp != 0)
                return DecompressLz4(array2, pkgChunk.len_uncomp);
            return array2;
        }

        private static byte[] DecompressLz4(byte[] src, int len_uncomp)
        {
            byte[] array = new byte[len_uncomp];
            LZ4Codec.Decode(src, 0, src.Length, array, 0, array.Length);
            return array;
        }

        private static int GetInt32(byte[] buf, int off)
        {
            return ((((((int)(0 | buf[off + 3]) << 8) | (int)buf[off + 2]) << 8) | (int)buf[off + 1]) << 8) | (int)buf[off];
        }

        private static long GetInt64(byte[] buf, int off)
        {
            return (long)(((((((((((((((0UL | (ulong)buf[off + 7]) << 8) | (ulong)buf[off + 6]) << 8) | (ulong)buf[off + 5]) << 8) | (ulong)buf[off + 4]) << 8) | (ulong)buf[off + 3]) << 8) | (ulong)buf[off + 2]) << 8) | (ulong)buf[off + 1]) << 8) | (ulong)buf[off]);
        }

        private static string GetString(byte[] buf, int off, ref int proc_len)
        {
            int num = 0;
            while (num < buf.Length - off && buf[off + num] != 0)
            {
                num++;
            }
            string @string = Encoding.UTF8.GetString(buf, off, num);
            proc_len = num + 1;
            return @string;
        }

        private static void LoadFileHeader(Dictionary<string, PkgChunk> file_map, string file_path)
        {
            if (!File.Exists(file_path))
            {
                return;
            }
            FileStream fileStream = new(file_path, FileMode.Open);
            byte[] array = new byte[16];
            fileStream.Seek(0L, SeekOrigin.Begin);
            fileStream.Read(array, 0, 16);
            long @int = GetInt64(array, 0);
            int int2 = GetInt32(array, 8);
            int int3 = GetInt32(array, 12);
            byte[] array2 = new byte[int2];
            fileStream.Seek(@int, SeekOrigin.Begin);
            fileStream.Read(array2, 0, int2);
            fileStream.Close();
            int num = 0;
            for (int i = 0; i < int3; i++)
            {
                int num2 = 0;
                string @string = GetString(array2, num, ref num2);
                num += num2;
                PkgChunk pkgChunk = new()
                {
                    pkg_path = file_path,
                    off = GetInt64(array2, num)
                };
                num += 8;
                pkgChunk.len_uncomp = GetInt32(array2, num);
                num += 4;
                pkgChunk.len_comp = GetInt32(array2, num);
                num += 4;
                file_map.Add(@string, pkgChunk);
            }
        }

        private static byte[] Proc_dec(byte[] buf_base, byte[] buf_now)
        {
            int i = 0;
            while (i < buf_now.Length && buf_now[i] != 0)
            {
                i++;
            }
            i++;
            int @int = GetInt32(buf_now, i);
            i += 4;
            byte[] array = new byte[@int];
            int num = 0;
            int num2 = 0;
            int num3 = buf_now.Length;
            while (i < num3)
            {
                byte b = buf_now[i++];
                if ((b & 128) != 0)
                {
                    int num4 = (int)(b & 127);
                    Array.Copy(buf_now, i, array, num, num4);
                    i += num4;
                    num += num4;
                    num2 += num4;
                }
                else if (b == 0)
                {
                    int num5 = (int)buf_now[i++];
                    num5 &= 255;
                    if ((num5 & 128) != 0)
                    {
                        num5 &= 127;
                        num5 -= 128;
                    }
                    num2 += num5;
                }
                else if (b == 1)
                {
                    int num6 = (int)buf_now[i++];
                    Array.Copy(buf_base, num2, array, num, num6);
                    num2 += num6;
                    num += num6;
                }
                else if (b == 2)
                {
                    int num7 = (int)buf_now[i++];
                    num7 <<= 8;
                    num7 |= (int)buf_now[i++];
                    Array.Copy(buf_base, num2, array, num, num7);
                    num2 += num7;
                    num += num7;
                }
                else if (b == 3)
                {
                    int num8 = (int)buf_now[i++];
                    num8 <<= 8;
                    num8 |= (int)buf_now[i++];
                    num8 <<= 8;
                    num8 |= (int)buf_now[i++];
                    num8 <<= 8;
                    num8 |= (int)buf_now[i++];
                    Array.Copy(buf_base, num2, array, num, num8);
                    num2 += num8;
                    num += num8;
                }
                else
                {
                    throw new Exception("Decompress Fail");
                }
            }
            return array;
        }
    }
}
