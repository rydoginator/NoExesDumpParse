using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoExesDumpParse
{
    internal interface IDumpDataReader
    {
        List<NoexsDumpIndex> Read();
        //long TryToParseAbs(List<IReverseOrderPath> path);
        Int64 ReadLittleEndianInt64(long address);

        long GetMain();
        long GetHeap();
        //Address TryToParseRel(List<IReverseOrderPath> path);
        bool IsHeap(long address);
    }
}
