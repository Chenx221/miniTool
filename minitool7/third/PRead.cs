using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace minitool7
{
    // Token: 0x0200000C RID: 12
    public class PRead
    {

        // Token: 0x06000049 RID: 73 RVA: 0x00003850 File Offset: 0x00001A50
        public PRead(string fn)
        {
            this.fs = new FileStream(fn, FileMode.Open, FileAccess.Read);
            this.Init();
            if (fn.ToLower().EndsWith("adult.dat"))
            {
                this.ti.Remove("def/version.txt");
            }
        }

        // Token: 0x0600004A RID: 74 RVA: 0x0000388F File Offset: 0x00001A8F
        public void Release()
        {
            if (this.fs != null)
            {
                this.fs.Close();
                this.fs = null;
            }
        }

        // Token: 0x0600004B RID: 75 RVA: 0x000038AC File Offset: 0x00001AAC
        ~PRead()
        {
            this.Release();
        }

        // Token: 0x0600004C RID: 76 RVA: 0x000038D8 File Offset: 0x00001AD8
        private void Init()
        {
            this.ti = new Dictionary<string, PRead.fe>();
            this.fs.Position = 0L;
            byte[] array = new byte[1024];
            this.fs.Read(array, 0, 1024);
            int num = 0;
            for (int i = 3; i < 255; i++)
            {
                num += BitConverter.ToInt32(array, i * 4);
            }
            byte[] array2 = new byte[16 * num];
            this.fs.Read(array2, 0, array2.Length);
            this.dd(array2, 16 * num, BitConverter.ToUInt32(array, 212));
            int num2 = BitConverter.ToInt32(array2, 12) - (1024 + 16 * num);
            byte[] array3 = new byte[num2];
            this.fs.Read(array3, 0, array3.Length);
            this.dd(array3, num2, BitConverter.ToUInt32(array, 92));
            this.Init2(array2, array3, num);
        }

        // Token: 0x0600004D RID: 77 RVA: 0x000039B8 File Offset: 0x00001BB8
        protected void Init2(byte[] rtoc, byte[] rpaths, int numfiles)
        {
            int num = 0;
            for (int i = 0; i < numfiles; i++)
            {
                int num2 = 16 * i;
                uint num3 = BitConverter.ToUInt32(rtoc, num2);
                int num4 = BitConverter.ToInt32(rtoc, num2 + 4);
                uint num5 = BitConverter.ToUInt32(rtoc, num2 + 8);
                uint num6 = BitConverter.ToUInt32(rtoc, num2 + 12);
                int num7 = num4;
                while (num7 < rpaths.Length && rpaths[num7] != 0)
                {
                    num7++;
                }
                string text = Encoding.ASCII.GetString(rpaths, num, num7 - num).ToLower();
                PRead.fe fe = default(PRead.fe);
                fe.p = num6;
                fe.L = num3;
                fe.k = num5;
                this.ti.Add(text, fe);
                num = num7 + 1;
            }
        }

        // Token: 0x0600004E RID: 78 RVA: 0x00003A6C File Offset: 0x00001C6C
        private void gk(byte[] b, uint k0)
        {
            uint num = k0 * 5892U + 41280U;
            uint num2 = (num << 7) ^ num;
            for (int i = 0; i < 256; i++)
            {
                num -= k0;
                num += num2;
                num2 = num + 341U;
                num *= num2 & 220U;
                b[i] = (byte)num;
                num >>= 2;
            }
        }

        // Token: 0x0600004F RID: 79 RVA: 0x00003AC0 File Offset: 0x00001CC0
        protected void dd(byte[] b, int L, uint k)
        {
            byte[] array = new byte[256];
            this.gk(array, k);
            for (int i = 0; i < L; i++)
            {
                byte b2 = b[i];
                b2 ^= array[i % 235];
                b2 += 31;
                b2 += array[i % 87];
                b2 ^= 165;
                b[i] = b2;
            }
        }

        // Token: 0x06000050 RID: 80 RVA: 0x00003B1C File Offset: 0x00001D1C
        public virtual byte[] Data(string fn)
        {
            PRead.fe fe;
            if (!this.ti.TryGetValue(fn, out fe))
            {
                return null;
            }
            this.fs.Position = (long)((ulong)fe.p);
            byte[] array = new byte[fe.L];
            this.fs.Read(array, 0, array.Length);
            this.dd(array, array.Length, fe.k);
            return array;
        }

        // Token: 0x0400003E RID: 62
        private FileStream fs;

        // Token: 0x0400003F RID: 63
        public Dictionary<string, PRead.fe> ti;

        // Token: 0x0200000D RID: 13
        public struct fe
        {
            // Token: 0x04000040 RID: 64
            public uint p;

            // Token: 0x04000041 RID: 65
            public uint L;

            // Token: 0x04000042 RID: 66
            public uint k;
        }
    }
}