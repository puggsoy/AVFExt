using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace AVFExt
{
    class AVF
    {
        static public AVF load(FileStream f, string fName)
        {
            BinaryReader br = new BinaryReader(f);
            string magic = new string(br.ReadChars(0x0F));

            if(magic != "AVF WayneSikes\0")
            {
                throw new Exception("Invalid AVF ID: " + magic);
            }

            br.BaseStream.Seek(0x15, SeekOrigin.Begin);

            ushort numEntries = br.ReadUInt16();
            ushort width = br.ReadUInt16();
            ushort height = br.ReadUInt16();

            br.BaseStream.Seek(0x21, SeekOrigin.Begin);

            AVFEntry[] entries = new AVFEntry[numEntries];

            for(int i = 0; i < numEntries; i++)
            {
                AVFEntry e = new AVFEntry();
                e.frameNo = br.ReadUInt16();
                e.offset = br.ReadUInt32();
                e.length = br.ReadUInt32();
                e.unk = br.ReadUInt32();
                br.BaseStream.Seek(5, SeekOrigin.Current);

                entries[i] = e;
            }

            AVF avf = new AVF();
            avf.basename = Path.GetFileNameWithoutExtension(fName);
            avf.frames = new Bitmap[numEntries];
            avf.rawFrames = new byte[numEntries][];

            for (int i = 0; i < numEntries; i++)
            {
                br.BaseStream.Seek(entries[i].offset, SeekOrigin.Begin);
                byte[] outData = br.ReadBytes((int)entries[i].length);
                outData = decrypt(outData);

                try
                {
                    outData = LZSS.dec(outData);
                }
                catch(Exception ex)
                {
                    throw new Exception("Decompression issue: " + ex.ToString());
                }

                avf.rawFrames[entries[i].frameNo] = new byte[outData.Length];
                outData.CopyTo(avf.rawFrames[entries[i].frameNo], 0);

                Bitmap bmp = toBitmap(outData, width, height);
                avf.frames[entries[i].frameNo] = bmp;
            }

            return avf;
        }

        static private Bitmap toBitmap(byte[] data, uint width, uint height)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format16bppRgb555);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                              ImageLockMode.WriteOnly, bmp.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        static private byte[] decrypt(byte[] enc)
        {
            byte[] dec = new byte[enc.Length];

            for(int i = 0; i < enc.Length; i++)
            {
                dec[i] = (byte)(enc[i] - i);
            }

            return dec;
        }
        //End of static functions

        public string basename;
        public byte[][] rawFrames;
        public Bitmap[] frames;
        
        public AVF() {}

        public void saveFrames(string dir)
        {
            for(int i = 0; i < frames.Length; i++)
            {
                string name = basename + "_" + i + ".png";
                frames[i].Save(Path.Combine(dir, name), ImageFormat.Png);
            }
        }

        public void dumpFrame(string dir, int index)
        {
            string name = basename + "_" + index + ".dmp";
            File.WriteAllBytes(Path.Combine(dir, name), rawFrames[index]);
        }

        private struct AVFEntry
        {
            public ushort frameNo;
            public uint offset;
            public uint length;
            public uint unk;
        }
    }
}
