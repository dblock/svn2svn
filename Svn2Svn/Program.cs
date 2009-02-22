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
        public bool simulationOnly = false;
        [Argument(ArgumentType.Required, ShortName = "s", LongName = "source", HelpText = "Source SVN path.")]
        public string Source;
        [Argument(ArgumentType.AtMostOnce, ShortName = "d", LongName = "destination", HelpText = "Target SVN path, default to current.")]
        public string Destination = Directory.GetCurrentDirectory();
        [Argument(ArgumentType.AtMostOnce, ShortName = "r", LongName = "revision", HelpText = "Copy from revision.")]
        public string revision;
        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "p", LongName = "prompt", HelpText = "Describe the change and prompt before commiting.")]
        public bool prompt = false;

        public SvnRevisionRange RevisionRange
        {
            get
            {
                if (!string.IsNullOrEmpty(revision))
                {
                    string[] revisionRange = revision.Split(":".ToCharArray(), 2);
                    return new SvnRevisionRange(int.Parse(revisionRange[0]), int.Parse(revisionRange[1]));
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
                return -1;
            }
        }
    }
}
