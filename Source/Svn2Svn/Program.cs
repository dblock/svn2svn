using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CommandLine;
using System.Reflection;
using System.IO;
using SharpSvn;

namespace Svn2Svn
{
    public class Svn2SvnCommandLineArguments
    {
        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "x", LongName = "simulationOnly", HelpText = "Simulation mode, don't commit anything.")]
        public bool SimulationOnly = false;
        [Argument(ArgumentType.Required, ShortName = "s", LongName = "source", HelpText = "Source SVN path.")]
        public string Source;
        [Argument(ArgumentType.AtMostOnce, ShortName = "d", LongName = "destination", HelpText = "Target SVN path, defaults to current.")]
        public string Destination = Directory.GetCurrentDirectory();
        [Argument(ArgumentType.AtMostOnce, ShortName = "r", LongName = "revision", HelpText = "Copy from revision to HEAD or within a start:end revision range.")]
        public string Revision;
        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "p", LongName = "prompt", HelpText = "Describe the change and prompt before committing.")]
        public bool Prompt = false;
        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "i", LongName = "incremental", HelpText = "Starts the replay action from last synced revision.")]
        public bool Incremental = false;
        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "t", LongName = "stopOnCopy", HelpText = "Stop on copy when fetching the oldest revision.")]
        public bool StopOnCopy = false;
        [Argument(ArgumentType.AtMostOnce, ShortName = "o", LongName = "root", HelpText = "Relative root before current branch.")]
        public string Root;

        public SvnRevisionRange RevisionRange
        {
            get
            {
                if (!string.IsNullOrEmpty(Revision))
                {
                    string[] revisionRange = Revision.Split(":".ToCharArray(), 2);
                    
                    if (revisionRange.Length > 2)
                    {
                        throw new Exception(string.Format("Invalid revision range '{0}', must be in the format X:Y.", 
                            Revision));
                    }

                    SvnRevision start = SvnRevisionParser.Parse(revisionRange[0]);
                    SvnRevision end = (revisionRange.Length == 2)
                        ? SvnRevisionParser.Parse(revisionRange[1]) 
                        : SvnRevisionType.Head;

                    return new SvnRevisionRange(start, end);
                }

                return null;
            }
        }
    }

    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Svn2Svn: {0}", Assembly.GetExecutingAssembly().GetName().Version);

                Svn2SvnCommandLineArguments commandLineArgs = new Svn2SvnCommandLineArguments();
                if (!Parser.ParseArgumentsWithUsage(args, commandLineArgs))
                    return -1;

                Console.WriteLine("From: {0}", commandLineArgs.Source);
                Console.WriteLine("To: {0}", commandLineArgs.Destination);

                SvnCopy svnCopy = new SvnCopy(commandLineArgs);
                svnCopy.Copy();

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
    }
}
