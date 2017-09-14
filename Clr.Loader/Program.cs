using System;


namespace Clr.Cli
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
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ForegroundColor = oldColor;
            }
        }

    }
}
