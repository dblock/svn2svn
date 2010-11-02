using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using System.IO;
using FolderSynchronisation;

namespace Svn2Svn
{
    public class FsCopy
    {
        private Fs2SvnCommandLineArguments _args = null;
        private SvnClient _client = new SvnClient();
        private List<FileSystemInfo> _svnAdd = null;
        private List<FileSystemInfo> _svnDelete = null;

        public FsCopy(Fs2SvnCommandLineArguments args)
        {
            _args = args;
        }

        public void Sync()
        {
            _svnAdd = new List<FileSystemInfo>();
            _svnDelete = new List<FileSystemInfo>();

            FolderSync sync = new FolderSync(_args.Source, _args.Destination, FileActions.Ask, FileActions.Ask, FileActions.Ask);
            sync.AskWhatToDo += new AskWhatToDoDelegate(sync_AskWhatToDo);
            sync.ErrorEvent += new ErrorDelegate(sync_ErrorEvent);
            sync.Sync();

            Console.WriteLine("Applying {0} svn delete changes ...", _svnDelete.Count);
            foreach (FileSystemInfo si in _svnDelete)
            {
                Console.WriteLine(" D {0}", si.FullName);
                if (!_args.simulationOnly)
                {
                    SvnDeleteArgs svnDeleteArgs = new SvnDeleteArgs();
                    svnDeleteArgs.ThrowOnError = true;
                    svnDeleteArgs.Force = true;
                    _client.Delete(si.FullName, svnDeleteArgs);
                }
            }

            Console.WriteLine("Applying {0} svn add changes ...", _svnAdd.Count);
            foreach (FileSystemInfo si in _svnAdd)
            {
                Console.WriteLine(" A {0}", si.FullName);

                if (!_args.simulationOnly)
                {
                    SvnAddArgs svnAddArgs = new SvnAddArgs();
                    svnAddArgs.ThrowOnError = true;
                    svnAddArgs.Depth = SvnDepth.Empty;
                    _client.Add(si.FullName, svnAddArgs);
                }                
            }
        }

        void sync_ErrorEvent(Exception e, string[] files)
        {
            Console.WriteLine(e.Message);
            throw e;
        }

        FileActions sync_AskWhatToDo(FileSystemInfo[] files, bool isADir, int missingIndex)
        {
            if (files[0].Name == ".svn" && isADir)
                return FileActions.Ignore;

            bool sourceExists = isADir ? Directory.Exists(files[0].FullName) : File.Exists(files[0].FullName);
            bool destinationExists = isADir ? Directory.Exists(files[1].FullName) : File.Exists(files[1].FullName);

            if (sourceExists && destinationExists)
            {
                if (isADir)
                {
                    // can this ever happen?
                    Console.WriteLine(" M {0} => {1}", files[0].FullName, files[1].FullName);
                    return FileActions.Ignore;
                }
                else
                {
                    Console.WriteLine(" M {0}", files[1].FullName);
                    return FileActions.Write1to2;
                }
            }
            else if (sourceExists)
            {
                // source exists, needs to be copied to destination and added
                Console.WriteLine(" A {0} ({1})", files[1].FullName, isADir ? "dir" : "file");
                _svnAdd.Add(files[1]);
                return _args.simulationOnly ? FileActions.Ignore : FileActions.Copy;
            }
            else if (destinationExists)
            {
                // target directory exists, needs to be deleted
                Console.WriteLine(" D {0} ({1})", files[1].FullName, isADir ? "dir" : "file");
                _svnDelete.Add(files[1]);
                return FileActions.Ignore;
            }
            else
            {
                throw new Exception(string.Format("Unexpected: {0} => {1}",
                    files[0].FullName, files[1].FullName));
            }
        }
    }
}
