using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVFExt
{
    class LZSS
    {
        static public byte[] dec(byte[] fData)
        {
            byte[] BUFFER = new byte[0x1000];
            for(int i = 0; i < BUFFER.Length; i++)
            {
                BUFFER[i] = 0x20;
            }

            List<byte> output = new List<byte>();

            byte flags8;
            ushort writeidx = 0xFEE;
            ushort readidx;
            uint fidx = 0;

            while(fidx < fData.Length)
            {
                flags8 = fData[fidx];
                fidx++;

                for(int i = 0; i < 8; i++)
                {
                    if((flags8 & 1) != 0)
                    {
                        output.Add(fData[fidx]);
                        BUFFER[writeidx] = fData[fidx];
                        writeidx++; writeidx %= 0x1000;
                        fidx++;
                    }
                    else
                    {
                        readidx = fData[fidx];
                        fidx++;
                        readidx = (ushort)(readidx | ((fData[fidx] & 0xF0) << 4));
                        
                        for(int j = 0; j < (fData[fidx] & 0x0F) + 3; j++)
                        {
                            output.Add(BUFFER[readidx]);
                            BUFFER[writeidx] = BUFFER[readidx];
                            readidx++; readidx %= 0x1000;
                            writeidx++; writeidx %= 0x1000;
                        }

                        fidx++;
                    }

                    flags8 >>= 1;

                    if (fidx >= fData.Length) break;
                }
            }

            return output.ToArray();
        }
    }
}
