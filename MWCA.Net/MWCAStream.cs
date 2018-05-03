using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MWCAdotNet
{
    public class MWCAStream
    {
        public byte[] S1 { get; set; }
        public byte[] S2 { get; set; }
        public byte[] S3 { get; set; }
        public byte[] BV { get; set; }
        public byte[] D1 { get; set; }
        public byte[] D2 { get; set; }
        public byte RedundantBitLength { get; set; }
        public byte EscapeLength { get; set; }

        public void WriteMultiStream(string FileName)
        {
            byte[] BVFile = new byte[BV.Length + 1];
            BVFile[0] = RedundantBitLength;
            Buffer.BlockCopy(BV, 0, BVFile, 1, BV.Length);

            byte[] D1File = new byte[D1.Length + 1];
            D1File[0] = EscapeLength;
            Buffer.BlockCopy(D1, 0, D1File, 1, D1.Length);

            File.WriteAllBytes(FileName + ".BV", BVFile);
            File.WriteAllBytes(FileName + ".S1", S1);
            File.WriteAllBytes(FileName + ".S2", S2);
            File.WriteAllBytes(FileName + ".S3", S3);
            File.WriteAllBytes(FileName + ".D1", D1File);
            File.WriteAllBytes(FileName + ".D2", D2);
        }
        public void WriteSingleStram(string FileName)
        {
            //Output Format
            //|RedundantBitLength(1b)|EscapeLength(1b)|S1Length(4b)|S2Length(4b)|S3Length(4b)|D1Length(4b)|D2Length(4b)|BVLength(4b)|S1|S2|S3|D1|D2|
            int FileSize = S1.Length + S2.Length + S3.Length + D1.Length + D2.Length + BV.Length;
            //FileSize + RedundantBitLength(1) + EscapeLength(1) + S1,S2,S3,D1,D2,BV Length (24) = FileSize + 26
            int TotalLength = 26;
            byte[] Output = new byte[FileSize + TotalLength];
            Output[0] = RedundantBitLength;
            Output[1] = EscapeLength;
            Buffer.BlockCopy(BitConverter.GetBytes(S1.Length), 0, Output, 2, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(S2.Length), 0, Output, 6, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(S3.Length), 0, Output, 10, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(D1.Length), 0, Output, 14, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(D2.Length), 0, Output, 18, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(BV.Length), 0, Output, 22, 4);

            Buffer.BlockCopy(S1, 0, Output, TotalLength, S1.Length);
            TotalLength += S1.Length;

            Buffer.BlockCopy(S2, 0, Output, TotalLength, S2.Length);
            TotalLength += S2.Length;

            Buffer.BlockCopy(S3, 0, Output, TotalLength, S3.Length);
            TotalLength += S3.Length;

            Buffer.BlockCopy(D1, 0, Output, TotalLength, D1.Length);
            TotalLength += D1.Length;

            Buffer.BlockCopy(D2, 0, Output, TotalLength, D2.Length);
            TotalLength += D2.Length;

            Buffer.BlockCopy(BV, 0, Output, TotalLength, BV.Length);
            TotalLength += BV.Length;

            File.WriteAllBytes(FileName + ".MWF", Output);
        }
        public void ReadMultiStream(string FileName)
        {
            byte[] D1File = File.ReadAllBytes(FileName + ".D1");
            S1 = File.ReadAllBytes(FileName + ".S1");
            S2 = File.ReadAllBytes(FileName + ".S2");
            S3 = File.ReadAllBytes(FileName + ".S3");
            D2 = File.ReadAllBytes(FileName + ".D2");
            byte[] BVFile = File.ReadAllBytes(FileName + ".BV");
            EscapeLength = D1File[0];
            RedundantBitLength = BVFile[0];
            BV = new byte[BVFile.Length - 1];
            D1 = new byte[D1File.Length - 1];
            Buffer.BlockCopy(BVFile, 1, BV, 0, BVFile.Length - 1);
            Buffer.BlockCopy(D1File, 1, D1, 0, D1File.Length - 1);
        }
        public void ReadSingleStream(string FileName)
        {
            //Output Format
            //|RedundantBitLength(1b)|EscapeLength(1b)|S1Length(4b)|S2Length(4b)|S3Length(4b)|D1Length(4b)|D2Length(4b)|BVLength(4b)|S1|S2|S3|D1|D2|
            byte[] Input = File.ReadAllBytes(FileName);
            RedundantBitLength = Input[0];
            EscapeLength = Input[1];
            S1 = new byte[BitConverter.ToInt32(Input, 2)];
            S2 = new byte[BitConverter.ToInt32(Input, 6)];
            S3 = new byte[BitConverter.ToInt32(Input, 14)];
            D1 = new byte[BitConverter.ToInt32(Input, 18)];
            D2 = new byte[BitConverter.ToInt32(Input, 22)];
            BV = new byte[BitConverter.ToInt32(Input, 10)];

            int TotalLength = 26;

            Buffer.BlockCopy(Input, TotalLength, S1, 0, S1.Length);
            TotalLength += S1.Length;

            Buffer.BlockCopy(Input, TotalLength, S2, 0, S2.Length);
            TotalLength += S2.Length;

            Buffer.BlockCopy(Input, TotalLength, S3, 0, S3.Length);
            TotalLength += S3.Length;

            Buffer.BlockCopy(Input, TotalLength, D1, 0, D1.Length);
            TotalLength += D1.Length;

            Buffer.BlockCopy(Input, TotalLength, D2, 0, D2.Length);
            TotalLength += D2.Length;

            Buffer.BlockCopy(Input, TotalLength, BV, 0, BV.Length);
            TotalLength += BV.Length;
        }
    }
}
