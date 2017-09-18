using System;
using Clr.Loader.Cli.Helpers;

namespace Clr.Loader.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CliHelper.Run(args);
            }
            catch(Exception ex)
            {
                ConsoleHelper.WriteError(ex);
            }
        }

    }
}
