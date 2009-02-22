using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using System.IO;

namespace Svn2Svn
{
    public class SvnCopy
    {
        private Svn2SvnCommandLineArguments _args = null;
        private SvnClient _client = new SvnClient();
        private SortedList<long, SvnLogEventArgs> _revisions = new SortedList<long, SvnLogEventArgs>();

        public SvnCopy(Svn2SvnCommandLineArguments args)
        {
            _args = args;
        }

        private void OnLog(object sender, SvnLogEventArgs args)
        {
            _revisions.Add(args.Revision, args);
            args.Detach();
            Console.Write(".");
        }

        public void Copy()
        {
            Console.Write("Collecting svn log: ");

            if (_args.RevisionRange != null)
            {
                Console.Write("{0}:{1} ",
                    _args.RevisionRange.StartRevision,
                    _args.RevisionRange.EndRevision);
            }

            // fetch the source svn respository and 
            SvnInfo sourceInfo = new SvnInfo(_args.Source);
            Console.WriteLine("Source SVN root: {0}", sourceInfo.Info.RepositoryRoot);
            Console.WriteLine("Source SVN uri: {0}", sourceInfo.Info.Uri);

            string sourceRelativePath = sourceInfo.Info.Uri.ToString().Remove(
                0, sourceInfo.Info.RepositoryRoot.ToString().Length - 1);

            Console.WriteLine("Relative path: {0}", sourceRelativePath);

            SvnLogArgs logArgs = new SvnLogArgs();
            logArgs.StrictNodeHistory = true;
            logArgs.ThrowOnError = true;
            logArgs.Range = _args.RevisionRange;
            logArgs.RetrieveChangedPaths = true;

            _client.Log(_args.Source, logArgs, new EventHandler<SvnLogEventArgs>(OnLog));
            Console.WriteLine();
            Console.WriteLine("Collected {0} revisions.", _revisions.Count);

            SvnTarget fromSvnTarget = SvnTarget.FromString(_args.Source);
            foreach (KeyValuePair<long, SvnLogEventArgs> revisionPair in _revisions)
            {
                SvnLogEventArgs revision = revisionPair.Value;
                Console.WriteLine("Revision {0} ({1})", revision.Revision, revision.Time);

                if (_args.simulationOnly)
                    continue;

                SvnExportArgs exportArgs = new SvnExportArgs();
                exportArgs.Overwrite = true;
                exportArgs.ThrowOnError = true;
                exportArgs.Revision = revision.Revision;

                SvnUpdateResult exportResult = null;
                _client.Export(fromSvnTarget, _args.Destination, exportArgs, out exportResult);

                if (revision.ChangedPaths == null)
                {
                    throw new Exception(string.Format("No changed paths in rev. {0}",
                        revision.Revision));
                }

                SortedList<string, SvnChangeItem> changeItems = new SortedList<string, SvnChangeItem>();

                foreach (SvnChangeItem changeItem in revision.ChangedPaths)
                {
                    changeItems.Add(changeItem.Path, changeItem);
                }

                foreach (SvnChangeItem changeItem in changeItems.Values)
                {
                    string targetSvnPath = changeItem.Path.Remove(0, sourceRelativePath.Length);
                    string targetOSPath = targetSvnPath.Replace("/", @"\");
                    string targetPath = Path.Combine(_args.Destination, targetOSPath);
                    Console.WriteLine("{0} {1}", changeItem.Action, changeItem.Path);
                    switch (changeItem.Action)
                    {
                        case SvnChangeAction.Add:
                            {
                                Console.WriteLine(" A {0}", targetPath);
                                SvnAddArgs svnAddArgs = new SvnAddArgs();
                                svnAddArgs.ThrowOnError = true;
                                svnAddArgs.Depth = SvnDepth.Empty;
                                _client.Add(targetPath, svnAddArgs);
                            }
                            break;
                        case SvnChangeAction.Delete:
                            {
                                Console.WriteLine(" D {0}", targetPath);
                                SvnDeleteArgs svnDeleteArgs = new SvnDeleteArgs();
                                svnDeleteArgs.ThrowOnError = true;
                                _client.Delete(targetPath, svnDeleteArgs);
                            }
                            break;
                    }
                }

                SvnCommitArgs commitArgs = new SvnCommitArgs();
                commitArgs.LogMessage = revision.LogMessage;
                commitArgs.LogMessage += string.Format("\nCopied from {0}, rev. {1} by {2} @ {3} {4}",
                    sourceInfo.Info.Uri, revision.Revision, revision.Author, revision.Time.ToShortDateString(), revision.Time.ToShortTimeString());
                commitArgs.ThrowOnError = true;

                Console.WriteLine("Commiting {0}", _args.Destination);
                Console.WriteLine("----------------------------------------------------------------------------");
                Console.WriteLine(commitArgs.LogMessage);
                Console.WriteLine("----------------------------------------------------------------------------");

                if (_args.prompt)
                {
                    while (true)
                    {
                        Console.Write("Commit? [Y/N] ");
                        char k = Char.ToLower(Console.ReadKey().KeyChar);
                        Console.WriteLine(); 
                        if (k == 'y') break;
                        if (k == 'n') throw new Exception("Aborted by user.");
                    }
                }

                SvnCommitResult commitResult = null;
                _client.Commit(_args.Destination, commitArgs, out commitResult);
                Console.WriteLine("Commited revision {0}.", commitResult.Revision);
            }
        }
    }
}
