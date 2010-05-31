using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace Svn2Svn
{
    public class SvnLogParser
    {
        private String _svnPath;
        private SvnClient _client = new SvnClient();

        private const string _initialSearchString = "Copied from ";
        private const string _revIndicatorString = ", rev. ";

        public SvnLogParser(String svnPath)
        {
            _svnPath = svnPath;
        }

        public int GetLastSyncedRevisionFromDestination()
        {
            Svn2SvnLogArgs logArgs = new Svn2SvnLogArgs();
            logArgs.StrictNodeHistory = true;
            logArgs.ThrowOnError = true;

            _client.Log(_svnPath, logArgs, new EventHandler<SvnLogEventArgs>(OnLogDelegate));

            int lastSyncedRevision = 0;

            foreach (KeyValuePair<long, SvnLogEventArgs> revisionPair in logArgs.revisions)
            {
                String msg = revisionPair.Value.LogMessage;

                if (msg.Contains(_initialSearchString))
                {
                    int idx = msg.IndexOf(_revIndicatorString) + _revIndicatorString.Length;
                    int endidx = msg.IndexOf(' ', idx);

                    String sr = msg.Substring(idx, endidx - idx);

                    lastSyncedRevision = Convert.ToInt32(sr);
                }
            }

            return lastSyncedRevision;
        }

        private void OnLogDelegate(object sender, SvnLogEventArgs args)
        {
            var largs = sender as Svn2SvnLogArgs;

            if (largs != null)
            {
                largs.revisions.Add(args.Revision, args);
            }

            args.Detach();            
        }
    }
}
