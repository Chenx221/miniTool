using System.Diagnostics;
using System.Text;

namespace minitool8.third
{
    public class APF<T>
    {
        // Token: 0x06000993 RID: 2451 RVA: 0x0005A9D4 File Offset: 0x00058BD4
        public APF(string _filePath, string _indexPath)
        {
            this.filePath = _filePath;
            this.GetIndex(_indexPath);
        }

        // Token: 0x06000994 RID: 2452 RVA: 0x0005A9EA File Offset: 0x00058BEA
        public APF(string _filePath, bool _compress = false)
        {
            this.filePath = _filePath;
            this.compress = _compress;
            this.HeaderDict = [];
            this.ParsingPack(_filePath);
        }

        // Token: 0x06000995 RID: 2453 RVA: 0x0005AA14 File Offset: 0x00058C14
        private void GetIndex(string _indexPath)
        {
            this.dataIndex = [];
            FileStream fileStream = new(_indexPath, FileMode.Open, FileAccess.Read);
            for (long num = 0L; num < fileStream.Length; num = fileStream.Position)
            {
                byte[] array = new byte[10];
                for (int i = 0; i < 10; i++)
                {
                    array[i] = (byte)fileStream.ReadByte();
                }
                long num2 = BitConverter.ToInt64(array);
                byte[] array2 = new byte[10];
                for (int j = 0; j < 10; j++)
                {
                    array2[j] = (byte)fileStream.ReadByte();
                }
                long num3 = BitConverter.ToInt64(array2);
                Index index;
                index.head = num2;
                index.length = num3;
                this.dataIndex.Add(index);
            }
        }

        // Token: 0x06000996 RID: 2454 RVA: 0x0005AACC File Offset: 0x00058CCC
        private void ParsingPack(string _filePath)
        {
            if (!File.Exists(this.filePath))
            {
                this.isInitialized = false;
                return;
            }
            FileStream fileStream = new(this.filePath, FileMode.Open, FileAccess.Read);
            fileStream.Seek(16L, SeekOrigin.Begin);
            while (fileStream.Position < fileStream.Length)
            {
                byte[] array = new byte[32];
                for (int i = 0; i < 32; i++)
                {
                    array[i] = (byte)fileStream.ReadByte();
                }
                int num = 0;
                int num2 = 0;
                while (num2 < 32 && array[num2] != 0)
                {
                    num++;
                    num2++;
                }
                string text = Encoding.UTF8.GetString(array, 0, num);
                byte[] array2 = new byte[10];
                for (int j = 0; j < 10; j++)
                {
                    array2[j] = (byte)fileStream.ReadByte();
                }
                long num3 = BitConverter.ToInt64(array2);
                byte[] array3 = new byte[10];
                for (int k = 0; k < 10; k++)
                {
                    array3[k] = (byte)fileStream.ReadByte();
                }
                long num4 = BitConverter.ToInt64(array3);
                text = text.ToUpper();
                APF<T>.PFHeader pfheader = new(text, num3 + 20L, num4);
                this.HeaderDict.Add(text, pfheader);
                fileStream.Seek(num4, SeekOrigin.Current);
            }
            fileStream.Close();
            fileStream.Dispose();
            this.isInitialized = true;
        }

        // Token: 0x04001521 RID: 5409
        private List<Index> dataIndex;

        // Token: 0x04001522 RID: 5410
        private string filePath;

        // Token: 0x04001523 RID: 5411
        public Dictionary<string, APF<T>.PFHeader> HeaderDict;

        //// Token: 0x04001524 RID: 5412
        //private AssetBundle currentAB;

        // Token: 0x04001525 RID: 5413
        public bool isInitialized;

        // Token: 0x04001526 RID: 5414
        public bool compress;

        // Token: 0x020002E6 RID: 742
        [Serializable]
        public class PFHeader(string _fileName, long _readStartBytePos, long _byteLength)
        {
            // Token: 0x040021E7 RID: 8679
            public string FileName = _fileName;

            // Token: 0x040021E8 RID: 8680
            public long readStartBytePos = _readStartBytePos;

            // Token: 0x040021E9 RID: 8681
            public long ByteLength = _byteLength;
        }
    }

    public struct Index
    {
        // Token: 0x0400151F RID: 5407
        public long head;

        // Token: 0x04001520 RID: 5408
        public long length;
    }

}
