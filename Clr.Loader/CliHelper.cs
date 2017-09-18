using System.IO;
using Clr.Shared;
using System;
using System.ComponentModel;
using System.Linq;
using NDesk.Options;
using Clr.Loader;

namespace Clr.Loader.Cli
{
    public static class CliHelper
    {

        public static void Run(string[] args)
        {
            var arguments = ParseArgs(args);

            if (arguments.Help)
            {
                DisplayHelp(arguments.Command);
                return;
            }

            switch (arguments.Command)
            {
                case Commands.install:
                    Console.WriteLine(Install(arguments));
                    break;
                case Commands.uninstall:
                    Console.WriteLine(Uninstall(arguments));
                    break;
                case Commands.view:
                    Console.WriteLine(View(arguments));
                    break;
                case Commands.Invalid:
                    Console.WriteLine($"{arguments.InvalidCommand} is an invalid command");
                    break;
                default:
                    DisplayHelp();
                    break;
            }
        }

        private static string Install(Arguments arguments)
        {
            if (string.IsNullOrEmpty(arguments.Path) && string.IsNullOrEmpty(arguments.Directory))
            {
                return "Missing path or directory";
            }

            if (!string.IsNullOrEmpty(arguments.Path) && !string.IsNullOrEmpty(arguments.Directory))
            {
                return "Only Path or Directory can be used at a time";
            }

            if (!string.IsNullOrEmpty(arguments.Path) && !File.Exists(arguments.Path))
            {
                return $"{arguments.Path} does not exists";
            }

            if (!string.IsNullOrEmpty(arguments.Directory) && !Directory.Exists(arguments.Directory))
            {
                return $"{arguments.Directory} does not exists";
            }

            if (string.IsNullOrEmpty(arguments.ConnectionString))
            {
                return "Connection string blank";
            }

            var connCheck = CheckConnectionString(arguments.ConnectionString);

            if (connCheck != "")
            {
                return connCheck;
            }

            var clrHelper = new LoaderHelper(arguments.ConnectionString);

            if (arguments.Path != "")
            {
                clrHelper.InstallClrAssembly(arguments.Path, permissionSetType.SAFE);
            }
            else
            {
                clrHelper.InstallClrAssemblyDirectory(arguments.Directory, permissionSetType.SAFE);
            }

            return "Install Completed!!";
        }

        private static string Uninstall(Arguments arguments)
        {
            if (string.IsNullOrEmpty(arguments.AssemblyName))
            {
                return $"Assembly is empty";
            }

            if (string.IsNullOrEmpty(arguments.ConnectionString))
            {
                return "Connection string blank";
            }

            var connCheck = CheckConnectionString(arguments.ConnectionString);

            if (connCheck != "")
            {
                return connCheck;
            }

            var clrHelper = new LoaderHelper(arguments.ConnectionString);

            clrHelper.UninstallClrAssembly(arguments.AssemblyName);

            return "Uninstall Completed!!";
        }


        private static string View(Arguments arguments)
        {

            if (string.IsNullOrEmpty(arguments.Path))
            {
                return "Missing path";
            }


            if (!string.IsNullOrEmpty(arguments.Path) && !File.Exists(arguments.Path))
            {
                return $"{arguments.Path} does not exists";
            }

            var clrHelper = new LoaderHelper();
            clrHelper.ViewClrAssemblyFunctions(arguments.Path);
            return "";
        }



        private static string CheckConnectionString(string conn)
        {

            var sqlHelper = new SqlHelper(conn);
            return sqlHelper.CheckConnection();
        }

        private static void DisplayHelp(Commands command = Commands.None)
        {
            if (command != Commands.None && command != Commands.Invalid)
            {
                Console.WriteLine($"{command.ToString()}");

                foreach (DescriptionAttribute attribute in command.GetType().GetField(command.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false))
                {
                    Console.WriteLine($"{attribute.Description}");
                }
                Console.WriteLine("");
            }
            else
            {
                foreach (Commands e in Enum.GetValues(typeof(Commands)))
                {
                    if (e != Commands.None && e != Commands.Invalid)
                    {
                        DisplayHelp(e);
                    }
                }
            }
        }

        private static Arguments ParseArgs(string[] args)
        {
            var commandString = args.Length >= 1 ? args[0].Trim().ToLower() : "";
            var arguments = new Arguments(commandString);

            if (arguments.Command != Commands.Invalid && arguments.Command != Commands.None)
            {
                args = args.Where((val, idx) => idx != 0).ToArray();
            }

            string xmlFilePath = "";

            var extras = new OptionSet(){
                { "xml|x=",x=> xmlFilePath = x},
                { "conn|c=",x=> arguments.ConnectionString = x },
                { "path|p=", x=> arguments.Path = PathFormater(x) },
                { "dir|d=", x=> arguments.Directory = x },
                { "assembly|a=", x=> arguments.AssemblyName = x },
                { "h|?|help", v => arguments.Help = v != null}
            }.Parse(args);

            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                arguments = ParseXmlArguments(arguments);
            }

            return arguments;
        }

        private static Arguments ParseXmlArguments(Arguments clrArguments)
        {



            return clrArguments;
        }


        private static string PathFormater(string path)
        {
            if(!Path.IsPathRooted(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(),path);
            }

            return path;
        }


    }
}
