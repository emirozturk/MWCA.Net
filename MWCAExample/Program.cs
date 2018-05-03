using MWCAdotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MWCAConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] input = File.ReadAllBytes("sample.txt");
            MWCAStream writeMs = MWCA.Compress(input);
            writeMs.WriteMultiStream("sample.txt");

            MWCAStream readMs = new MWCAStream();
            readMs.ReadMultiStream("sample.txt");
            byte[] readBuffer = MWCA.Decompress(readMs);
            File.WriteAllBytes("sample.mwa",readBuffer);
        }
    }
}
