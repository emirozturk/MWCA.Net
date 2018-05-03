using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWCAdotNet
{
    public static class MWCA
    {
        public static MWCAStream Compress(byte[] InputStream)
        {
            MWCACompress mc = new MWCACompress(InputStream);
            return mc.Stream;
        }
        public static byte[] Decompress(MWCAStream Stream)
        {
            MWCADecompress md = new MWCADecompress(Stream);
            return md.OutputStream;
        }
    }
}
