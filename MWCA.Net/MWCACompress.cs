using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MWCAdotNet
{
    class MWCACompress
    {
        public MWCAStream Stream { get; set; }
        byte[] InputStream;
        bool[] IsChar;
        Dictionary<string, byte> D1Dict;
        Dictionary<string, ushort> D2Dict;
        HashSet<string> D1Hash;
        HashSet<string> D2Hash;
        int MaxWordLength;
        byte EscapeLength;

        public MWCACompress(byte[] InputStream)
        {
            Stream = new MWCAStream();
            this.InputStream = InputStream;
            IsChar = new bool[256];
            for (int i = 0; i < 256; i++)
                IsChar[i] = char.IsLetterOrDigit((char)i);
            D1Dict = new Dictionary<string, byte>(255);
            D2Dict = new Dictionary<string, ushort>(65536);
            D1Hash = new HashSet<string>();
            D2Hash = new HashSet<string>();
            MaxWordLength = 0;
            EscapeLength = 0;
            FirstPass();
            SecondPass();
        }

        private void FirstPass()
        {
            int Pointer = 0;
            int Start;
            string Word = "";
            Dictionary<string, int> WordFrequency = new Dictionary<string, int>();
            HashSet<string> wordList = new HashSet<string>();

            while (Pointer < InputStream.Length)
            {
                Start = Pointer;
                if (Pointer < InputStream.Length && IsChar[InputStream[Pointer]])
                    while (IsChar[InputStream[Pointer]] && Pointer < InputStream.Length)
                        Pointer++;
                else
                {
                    if (InputStream[Pointer] != ' ')
                        while (Pointer < InputStream.Length && !IsChar[InputStream[Pointer]])
                            Pointer++;
                    else
                    {
                        Pointer++;
                        if (Pointer < InputStream.Length && IsChar[InputStream[Pointer]])
                        {
                            Start = Pointer;
                            while (Pointer < InputStream.Length && IsChar[InputStream[Pointer]])
                                Pointer++;
                        }
                        else
                            while (Pointer < InputStream.Length && !IsChar[InputStream[Pointer]])
                                Pointer++;
                    }
                }
                for (int i = Start; i < Pointer; i++)
                    Word += Convert.ToChar(InputStream[i]);

                if (Word.Length > MaxWordLength)
                    MaxWordLength = Word.Length;

                if (wordList.Contains(Word))
                    WordFrequency[Word]++;
                else
                {
                    wordList.Add(Word);
                    WordFrequency.Add(Word, 1);
                }
                Word = "";
            }
            if (Word.Length > 0)
            {
                if (Word.Length > MaxWordLength)
                    MaxWordLength = Word.Length;

                if (wordList.Contains(Word))
                    WordFrequency[Word]++;
                else
                {
                    wordList.Add(Word);
                    WordFrequency.Add(Word, 1);
                }
            }

            string[] Words = WordFrequency.OrderByDescending(x => x.Value).Select(x => x.Key).Take(255 + 65536).ToArray();

            for (int i = 0; i < 255; i++)
            {
                if (i == Words.Length) break;
                D1Dict.Add(Words[i], Convert.ToByte(i + 1));
                D1Hash.Add(Words[i]);
            }
            if (Words.Length > 255)
                for (int i = 255; i < 255 + 65536; i++)
                {
                    if (i == Words.Length) break;
                    D2Dict.Add(Words[i], Convert.ToUInt16(i - 255));
                    D2Hash.Add(Words[i]);
                }

            EscapeLength = Convert.ToByte(Math.Ceiling(Math.Log(MaxWordLength, 256)));
            Stream.EscapeLength = EscapeLength;

            List<byte> D1 = new List<byte>();
            List<byte> D2 = new List<byte>();

            for (int i = 0; i < 255; i++)
            {
                if (i == Words.Length) break;
                D1.AddRange(ConvertToLengthWordFormat(Words[i]));
            }
            if (Words.Length > 255)
                for (int i = 255; i < 255 + 65536; i++)
                {
                    if (i == Words.Length) break;
                    D2.AddRange(ConvertToLengthWordFormat(Words[i]));
                }
            Stream.D1 = D1.ToArray();
            Stream.D2 = D2.ToArray();
        }

        private void SecondPass()
        {
            List<bool> BV = new List<bool>();
            List<byte> S1 = new List<byte>();
            List<ushort> S2 = new List<ushort>();
            List<byte> S3 = new List<byte>();

            int Pointer = 0;
            int Start = 0;
            string Word = "";

            while (Pointer < InputStream.Length)
            {
                Start = Pointer;
                if (Pointer < InputStream.Length && IsChar[InputStream[Pointer]])
                    while (Pointer < InputStream.Length && IsChar[InputStream[Pointer]])
                        Pointer++;
                else
                {
                    if (InputStream[Pointer] != ' ')
                        while (Pointer < InputStream.Length && !IsChar[InputStream[Pointer]])
                            Pointer++;
                    else
                    {
                        Pointer++;
                        if (Pointer < InputStream.Length && IsChar[InputStream[Pointer]])
                        {
                            Start = Pointer;
                            while (Pointer < InputStream.Length && IsChar[InputStream[Pointer]])
                                Pointer++;
                        }
                        else
                            while (Pointer < InputStream.Length && !IsChar[InputStream[Pointer]])
                                Pointer++;
                    }
                }
                for (int i = Start; i < Pointer; i++)
                    Word += Convert.ToChar(InputStream[i]);

                if (D1Hash.Contains(Word))
                {
                    BV.Add(false);
                    S1.Add(D1Dict[Word]);
                }
                else if (D2Hash.Contains(Word))
                {
                    BV.Add(true);
                    S2.Add(D2Dict[Word]);
                }
                else
                {
                    BV.Add(false);
                    S1.Add(0);
                    S3.AddRange(ConvertToLengthWordFormat(Word));
                }
                Word = "";
            }
            if (Word.Length > 0)
            {
                if (D1Hash.Contains(Word))
                {
                    BV.Add(false);
                    S1.Add(D1Dict[Word]);
                }
                else if (D2Hash.Contains(Word))
                {
                    BV.Add(true);
                    S2.Add(D2Dict[Word]);
                }
                else
                {
                    BV.Add(false);
                    S1.Add(0);
                    S3.AddRange(ConvertToLengthWordFormat(Word));
                }
            }
            Stream.RedundantBitLength = Convert.ToByte(8 - (BV.Count % 8));
            Stream.S1 = new byte[S1.Count];
            Stream.S2 = new byte[S2.Count * 2];
            Stream.S3 = new byte[S3.Count];

            if (S1.Count > 0) Buffer.BlockCopy(S1.ToArray(), 0, Stream.S1, 0, S1.Count);
            if (S2.Count > 0) Buffer.BlockCopy(S2.ToArray(), 0, Stream.S2, 0, S2.Count * 2);
            if (S3.Count > 0) Buffer.BlockCopy(S3.ToArray(), 0, Stream.S3, 0, S3.Count);

            BitArray ba = new BitArray(BV.ToArray());
            Stream.BV = ToByteArray(ba);
        }

        private List<byte> ConvertToLengthWordFormat(string Word)
        {
            List<byte> List = new List<byte>();
            byte[] Length = BitConverter.GetBytes(Word.Length);
            for (int i = 0; i < EscapeLength; i++)
                List.Add(Length[i]);
            for (int i = 0; i < Word.Length; i++)
                List.Add(Convert.ToByte(Word[i]));
            return List;
        }

        public byte[] ToByteArray(BitArray Bits)
        {
            int NumBytes = Bits.Count / 8;
             NumBytes++;

            byte[] Bytes = new byte[NumBytes];
            int ByteIndex = 0, BitIndex = 0;

            for (int i = 0; i < Bits.Count; i++)
            {
                if (Bits[i])
                    Bytes[ByteIndex] |= (byte)(1 << (7 - BitIndex));

                BitIndex++;
                if (BitIndex == 8)
                {
                    BitIndex = 0;
                    ByteIndex++;
                }
            }
            return Bytes;
        }

    }
}
