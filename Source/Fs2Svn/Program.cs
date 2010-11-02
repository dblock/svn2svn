using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CommandLine;
using System.Reflection;
using System.IO;
using SharpSvn;

namespace Svn2Svn
{
    public class Fs2SvnCommandLineArguments
    {
        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "x", LongName = "simulationOnly", HelpText = "Simulation mode, don't commit anything.")]
        public bool simulationOnly = false;
        [Argument(ArgumentType.Required, ShortName = "s", LongName = "source", HelpText = "Source SVN path.")]
        public string Source;
        [Argument(ArgumentType.AtMostOnce, ShortName = "d", LongName = "destination", HelpText = "Target SVN path, default to current.")]
        public string Destination = Directory.GetCurrentDirectory();
    }

    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Fs2Svn: {0}", Assembly.GetExecutingAssembly().GetName().Version);

                Fs2SvnCommandLineArguments commandLineArgs = new Fs2SvnCommandLineArguments();
                if (!Parser.ParseArgumentsWithUsage(args, commandLineArgs))
                    return -1;

                Console.WriteLine("From: {0}", commandLineArgs.Source);
                Console.WriteLine("To: {0}", commandLineArgs.Destination);

                FsCopy fsCopy = new FsCopy(commandLineArgs);
                fsCopy.Sync();
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
