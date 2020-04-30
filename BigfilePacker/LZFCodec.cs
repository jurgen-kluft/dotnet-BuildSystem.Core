/*
 * Copyright (c) 2005 Oren J. Maurice <oymaurice@hazorea.org.il>
 * 
 * Redistribution and use in source and binary forms, with or without modifica-
 * tion, are permitted provided that the following conditions are met:
 * 
 *   1.  Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 * 
 *   2.  Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 * 
 *   3.  The name of the author may not be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MER-
 * CHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO
 * EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPE-
 * CIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTH-
 * ERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Alternatively, the contents of this file may be used under the terms of
 * the GNU General Public License version 2 (the "GPL"), in which case the
 * provisions of the GPL are applicable instead of the above. If you wish to
 * allow the use of your version of this file only under the terms of the
 * GPL and not to allow others to use your version of this file under the
 * BSD license, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the GPL. If
 * you do not delete the provisions above, a recipient may use your version
 * of this file under either the BSD or the GPL.
 */

using System;
using System.Collections.Generic;

namespace Codec
{
    /// <summary>
    /// Summary description for LZF.
    /// </summary>
    public class LZF
    {
        /// <summary>
        /// LZF Compressor
        /// </summary>

        UInt32 HLOG = 14;
        UInt32 HSIZE = (1 << 14);

        /*
        * don't play with this unless you benchmark!
        * decompression is not dependent on the hash function
        * the hashing function might seem strange, just believe me
        * it works ;)
        */
        UInt32 MAX_LIT = (1 << 5);
        UInt32 MAX_OFF = (1 << 13);
        UInt32 MAX_REF = ((1 << 8) + (1 << 3));

        UInt32 FRST(byte[] array, UInt32 ptr)
        {
            return (UInt32)(((array[ptr]) << 8) | array[ptr + 1]);
        }

        UInt32 NEXT(UInt32 v, byte[] array, UInt32 ptr)
        {
            return ((v) << 8) | array[ptr + 2];
        }

        UInt32 IDX(UInt32 h)
        {
            return ((h ^ (h << 5)) >> (int)(((3 * 8 - HLOG)) - h * 5) & (HSIZE - 1));
        }

        /*
        * compressed format
        *
        * 000LLLLL <L+1>    ; literal
        * LLLOOOOO oooooooo ; backref L
        * 111OOOOO LLLLLLLL oooooooo ; backref L+7
        *
        */

        public byte[] Compress(byte[] in_data)
        {
            int in_len = in_data.Length;

            int c;

            long[] htab = new long[1 << 14];
            for (c = 0; c < 1 << 14; c++)
            {
                htab[c] = 0;
            }

            long hslot;
            long reference;

            UInt32 iidx = 4;
            UInt32 oidx = 0;

            UInt32 hval = FRST(in_data, iidx);
            long off;
            int lit = 0;

            int out_len = in_len + (in_len / 4);
            List<byte> out_data = new List<byte>(out_len);

            for (; ; )
            {
                if (iidx < in_len - 2)
                {
                    hval = NEXT(hval, in_data, iidx);
                    hslot = IDX(hval);
                    reference = htab[hslot];
                    htab[hslot] = (long)iidx;

                    if ((off = iidx - reference - 1) < MAX_OFF
                        && iidx + 4 < in_len
                        && reference > 0
                        && in_data[reference + 0] == in_data[iidx + 0]
                        && in_data[reference + 1] == in_data[iidx + 1]
                        && in_data[reference + 2] == in_data[iidx + 2]
                        )
                    {
                        /* match found at *reference++ */
                        UInt32 len = 2;
                        UInt32 maxlen = (UInt32)in_len - iidx - len;
                        maxlen = maxlen > MAX_REF ? MAX_REF : maxlen;

                        if (oidx + lit + 1 + 3 >= out_len)
                            return null;

                        do
                            len++;
                        while (len < maxlen && in_data[reference + len] == in_data[iidx + len]);

                        if (lit != 0)
                        {
                            out_data.Add((byte)(lit - 1));
                            oidx++;
                            lit = -lit;
                            do
                            {
                                out_data.Add(in_data[iidx + lit]);
                                oidx++;
                            } while ((++lit) != 0);
                        }

                        len -= 2;
                        iidx++;

                        if (len < 7)
                        {
                            out_data.Add((byte)((off >> 8) + (len << 5)));
                            oidx++;
                        }
                        else
                        {
                            out_data.Add((byte)((off >> 8) + (7 << 5)));
                            oidx++;
                            out_data.Add((byte)(len - 7));
                            oidx++;
                        }

                        out_data.Add((byte)off);
                        oidx++;

                        iidx += len - 1;
                        hval = FRST(in_data, iidx);

                        hval = NEXT(hval, in_data, iidx);
                        htab[IDX(hval)] = iidx;
                        iidx++;

                        hval = NEXT(hval, in_data, iidx);
                        htab[IDX(hval)] = iidx;
                        iidx++;
                        continue;
                    }
                }
                else if (iidx == in_len)
                    break;

                /* one more literal byte we must copy */
                lit++;
                iidx++;

                if (lit == MAX_LIT)
                {
                    if (oidx + 1 + MAX_LIT >= out_len)
                        return null;

                    out_data.Add((byte)(MAX_LIT - 1));
                    oidx++;
                    lit = -lit;
                    do
                    {
                        out_data.Add(in_data[iidx + lit]);
                        oidx++;
                    } while ((++lit) != 0);
                }
            }

            if (lit != 0)
            {
                if (oidx + lit + 1 >= out_len)
                    return null;

                out_data.Add((byte)(lit - 1));
                oidx++;
                lit = -lit;
                do
                {
                    out_data.Add(in_data[iidx + lit]);
                    oidx++;
                } while ((++lit) != 0);
            }

            byte[] compressed_data = new byte[4 + oidx];
            compressed_data[0] = (byte)(oidx >> 24);
            compressed_data[1] = (byte)(oidx >> 16);
            compressed_data[2] = (byte)(oidx >> 8);
            compressed_data[3] = (byte)(oidx);
            out_data.CopyTo(0, compressed_data, 0, (int)oidx);
            return compressed_data;
        }

        /// <summary>
        /// LZF Decompressor
        /// </summary>
        public int Decompress(byte[] in_data, int in_len, byte[] out_data, int out_len)
        {
            UInt32 oidx = 0;

            out_len = (in_data[0] << 24) | (in_data[1] << 16) | (in_data[2] << 8) | (in_data[3]);
            UInt32 iidx = 4;

            do
            {
                UInt32 ctrl = in_data[iidx++];

                if (ctrl < (1 << 5)) /* literal run */
                {
                    ctrl++;

                    if (oidx + ctrl > out_len)
                    {
                        //SET_ERRNO (E2BIG);
                        return 0;
                    }

                    do
                        out_data[oidx++] = in_data[iidx++];
                    while ((--ctrl) != 0);
                }
                else /* back reference */
                {
                    UInt32 len = ctrl >> 5;

                    int reference = (int)(oidx - ((ctrl & 0x1f) << 8) - 1);

                    if (len == 7)
                        len += in_data[iidx++];

                    reference -= in_data[iidx++];

                    if (oidx + len + 2 > out_len)
                    {
                        //SET_ERRNO (E2BIG);
                        return 0;
                    }

                    if (reference < 0)
                    {
                        //SET_ERRNO (EINVAL);
                        return 0;
                    }

                    out_data[oidx++] = out_data[reference++];
                    out_data[oidx++] = out_data[reference++];

                    do
                        out_data[oidx++] = out_data[reference++];
                    while ((--len) != 0);
                }
            }
            while (iidx < in_len);

            return (int)oidx;
        }

        public LZF()
        {
            //
            // Add ructor logic here
            //
        }
    }
}