using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MWCAdotNet
{
    class MWCADecompress
    {
        MWCAStream Stream { get; set; }
        public byte[] OutputStream { get; set; }
        bool[] IsChar;
        List<byte> Output;
        List<byte[]> D1;
        List<byte[]> D2;
        int S1Counter, S2Counter, S3Counter, CodeWord;
        bool Previous, Current;

        public MWCADecompress(MWCAStream Stream)
        {
            this.Stream = Stream;
            Output = new List<byte>();
            D1 = new List<byte[]>(255);
            D2 = new List<byte[]>(65536);
            S1Counter = 0;
            S2Counter = 0;
            S3Counter = 0;
            Previous = false;
            Current = false;
            IsChar = new bool[256];
            for (int i = 0; i < 256; i++)
                IsChar[i] = char.IsLetterOrDigit((char)i);

            CreateDictionaries();
            bool[] BV = Stream.BV.SelectMany(GetBits).ToArray();

            for (int i = 0; i < BV.Length - Stream.RedundantBitLength; i++)
            {
                if (!BV[i])
                {
                    CodeWord = Stream.S1[S1Counter++];
                    if (CodeWord == 0)
                    {
                        byte[] S3WordArray = new byte[Stream.EscapeLength];
                        for (int j = 0; j < Stream.EscapeLength; j++)
                            S3WordArray[j] = Stream.S3[S3Counter++];
                        Current = IsChar[Stream.S3[S3Counter]];
                        if (Previous && Current)
                            Output.Add(Convert.ToByte(' '));
                        int boyut = BitConverter.ToInt32(S3WordArray, 0);
                        for (int j = 0; j < boyut; j++)
                            Output.Add(Stream.S3[S3Counter++]);
                    }
                    else
                    {
                        Current = IsChar[D1[CodeWord][0]];
                        if (Previous && Current)
                            Output.Add(Convert.ToByte(' '));
                        Output.AddRange(D1[CodeWord]);
                    }
                }
                else if (BV[i])
                {
                    byte[] parca = new byte[2];
                    parca[0] = Stream.S2[S2Counter++];
                    parca[1] = Stream.S2[S2Counter++];
                    CodeWord = BitConverter.ToInt16(parca, 0);
                    Current = IsChar[D2[CodeWord][0]];
                    if (Previous && Current)
                        Output.Add(Convert.ToByte(' '));
                    Output.AddRange(D2[CodeWord]);

                }
                Previous = Current;
            }
            OutputStream = Output.ToArray();
        }

        private void CreateDictionaries()
        {
            byte[] WordLengthArray = new byte[8];
            int DictionaryCounter = 0;
            Int64 WordLength = 0;
            D1.Add(new byte[0]);
            do
            {
                for (int j = 0; j < Stream.EscapeLength; j++)
                    WordLengthArray[j] = Stream.D1[DictionaryCounter++];

                WordLength = BitConverter.ToInt64(WordLengthArray, 0);

                List<byte> EscapeList = new List<byte>();
                for (int i = 0; i < WordLength; i++)
                    EscapeList.Add(Stream.D1[DictionaryCounter++]);

                D1.Add(EscapeList.ToArray());
            } while (DictionaryCounter < Stream.D1.Length);

            DictionaryCounter = 0;

            if (Stream.D2.Length > 0)
                do
                {
                    for (int j = 0; j < Stream.EscapeLength; j++)
                        WordLengthArray[j] = Stream.D2[DictionaryCounter++];

                    WordLength = BitConverter.ToInt64(WordLengthArray, 0);

                    List<byte> EscapeList = new List<byte>();
                    for (int i = 0; i < WordLength; i++)
                        EscapeList.Add(Stream.D2[DictionaryCounter++]);

                    D2.Add(EscapeList.ToArray());
                } while (DictionaryCounter < Stream.D2.Length);
        }

        private IEnumerable<bool> GetBits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }
    }
}
