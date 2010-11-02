using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using System.IO;
using System.Reflection;
using Microsoft.VisualBasic.FileIO;

namespace Svn2
{
    class Program
    {
        static void Usage()
        {            
            Console.WriteLine("syntax: Svn2 [command] ...");
            Console.WriteLine(" commands:");
            Console.WriteLine("  sync: delete (recycle bin) all unversioned files");
            Console.WriteLine("  cleanup: svn cleanup");
            Console.WriteLine("  revert: svn revert");
            Console.WriteLine("  status: svn status");
        }

        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Svn2: {0}", Assembly.GetExecutingAssembly().GetName().Version);

                if (args.Length < 2)
                {
                    Usage();
                    return -2;
                }

                string command = args[0];
                SvnClient client = new SvnClient();
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "/?" || args[i] == "-?" || args[i] == "--help")
                    {
                        Usage();
                        return -2;
                    }

                    string path = Path.GetFullPath(args[i]);

                    switch (command)
                    {
                        case "sync":
                            {
                                SvnStatusArgs statusArgs = new SvnStatusArgs();
                                statusArgs.Depth = SvnDepth.Infinity;
                                statusArgs.ThrowOnError = true;
                                client.Status(path, statusArgs, new EventHandler<SvnStatusEventArgs>(delegate(object sender, SvnStatusEventArgs e)
                                {
                                    switch (e.LocalContentStatus)
                                    {
                                        case SvnStatus.NotVersioned:
                                            Console.WriteLine(" {0} {1}", StatusToChar(e.LocalContentStatus), e.FullPath);
                                            if (File.Exists(e.FullPath))
                                            {
                                                FileSystem.DeleteFile(e.FullPath, UIOption.OnlyErrorDialogs,
                                                    RecycleOption.SendToRecycleBin);
                                            }
                                            else if (Directory.Exists(e.FullPath))
                                            {
                                                FileSystem.DeleteDirectory(e.FullPath, UIOption.OnlyErrorDialogs,
                                                    RecycleOption.SendToRecycleBin);
                                            }
                                            break;
                                    }
                                }));
                            }
                            break;
                        case "cleanup":
                            {
                                Console.WriteLine("Cleaning up {0}", path);
                                SvnCleanUpArgs cleanupArgs = new SvnCleanUpArgs();
                                cleanupArgs.ThrowOnError = true;
                                cleanupArgs.Notify += new EventHandler<SvnNotifyEventArgs>(delegate(object sender, SvnNotifyEventArgs e)
                                    {
                                        Console.WriteLine(" L {0}", e.FullPath);
                                    });
                                client.CleanUp(path, cleanupArgs);
                            }
                            break;
                        case "revert":
                            {
                                Console.WriteLine("Reverting {0}", path);
                                SvnRevertArgs revertArgs = new SvnRevertArgs();
                                revertArgs.Depth = SvnDepth.Infinity;
                                revertArgs.ThrowOnError = true;
                                revertArgs.Notify += new EventHandler<SvnNotifyEventArgs>(delegate(object sender, SvnNotifyEventArgs e)
                                    {
                                        Console.WriteLine(" R {0}", e.FullPath);
                                    });
                                client.Revert(path, revertArgs);
                            }
                            break;
                        case "status":
                            {
                                SvnStatusArgs statusArgs = new SvnStatusArgs();
                                statusArgs.Depth = SvnDepth.Infinity;
                                statusArgs.ThrowOnError = true;
                                client.Status(path, statusArgs, new EventHandler<SvnStatusEventArgs>(delegate(object sender, SvnStatusEventArgs e)
                                    {
                                        Console.WriteLine(" {0} {1}", StatusToChar(e.LocalContentStatus), e.FullPath);
                                    }));
                            }
                            break;
                        default:
                            throw new Exception(string.Format("Unsupported '{0}' command", command));
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
#if DEBUG
                Console.Error.WriteLine(ex.StackTrace);
#endif
                return -1;
            }
        }

        private static char StatusToChar(SvnStatus status)
        {
            char c = '.';
            switch (status)
            {
                case SvnStatus.Added: c = 'A'; break;
                case SvnStatus.Conflicted: c = 'C'; break;
                case SvnStatus.Deleted: c = 'D'; break;
                case SvnStatus.External: c = 'X'; break;
                case SvnStatus.Ignored: c = 'I'; break;
                case SvnStatus.Incomplete: c = '*'; break;
                case SvnStatus.Merged: c = 'G'; break;
                case SvnStatus.Missing: c = '!'; break;
                case SvnStatus.Modified: c = 'M'; break;
                case SvnStatus.None: c = ' '; break;
                case SvnStatus.Normal: c = '-'; break;
                case SvnStatus.NotVersioned: c = '?'; break;
                case SvnStatus.Obstructed: c = 'O'; break;
                case SvnStatus.Replaced: c = 'R'; break;
                case SvnStatus.Zero: c = '0'; break;
            }
            return c;
        }
    }
}
