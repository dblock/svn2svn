using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace Svn2Svn
{
    public class Svn2SvnLogArgs : SvnLogArgs
    {
        public readonly SortedList<long, SvnLogEventArgs> revisions = new SortedList<long, SvnLogEventArgs>();
    }
}
