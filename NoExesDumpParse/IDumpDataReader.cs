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
        PointerInfo Read(CancellationToken token, IProgress<int> prog);
        long TryToParseAbs(List<IReverseOrderPath> path);
        Address TryToParseRel(List<IReverseOrderPath> path);
        bool IsHeap(long address);
    }
}
