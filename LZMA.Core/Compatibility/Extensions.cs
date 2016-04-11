using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LZMA.Core.Compatibility
{
    public static class Extensions
    {
        public static void Close(this Stream stream)
        {
            stream.Dispose();
        }
    }
}
