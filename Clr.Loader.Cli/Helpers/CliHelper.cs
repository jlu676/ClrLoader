using System.IO;
using Clr.Shared;
using System;
using System.ComponentModel;
using Clr.Loader.Cli.Models;
using System.Collections.Generic;

namespace Clr.Loader.Cli.Helpers
{
    public static class CliHelper
    {

        public static void Run(string[] args)
        {

            try
            {
                var arguments = new Arguments(args);

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
                    case Commands.generate:
                        Console.WriteLine(GenerateXml(arguments));
                        break;
                    case Commands.Invalid:
                        Console.WriteLine($"{arguments.InvalidCommand} is not a valid command or shortcut");
                        DisplayHelp(arguments.Command);
                        break;
                    case Commands.ExtraArguments:
                        Console.WriteLine("There are extra uneeded paramaters");

                        foreach (var arg in arguments.ExtraArguments)
                        {
                            Console.WriteLine($"Uneeded argument : {arg}");
                        }
                        DisplayHelp();
                        break;
                    default:
                        DisplayHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex);
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

        private static string GenerateXml(Arguments arguments)
        {   
            var xmlArguments = new XmlArguments {
                AssemblyName = "{AssemblyName}",
                CommandString = "{CommandString}",
                ConnectionString = "{ConnectionString }",
                Directory ="{Directory}",
                Path ="{Path}" };
            var path = IoHelper.Filecheck(IoHelper.PathFormater(arguments.Path));
            IoHelper.WriteXmlStringToFile(path, xmlArguments);
            return "completed";
        }

        private static string CheckConnectionString(string conn)
        {
            var sqlHelper = new SqlHelper(conn);
            return sqlHelper.CheckConnection();
        }

        private static void DisplayHelp(Commands command = Commands.None)
        {
            List<Commands> nonHelpCommands = new List<Commands> { Commands.None, Commands.Invalid, Commands.ExtraArguments };

            if (!nonHelpCommands.Contains(command))
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
                Console.WriteLine("\n\r --commands | -C The name of the command you wish to run ");
                foreach (Commands e in Enum.GetValues(typeof(Commands)))
                {
                    if (!nonHelpCommands.Contains(e))
                    {
                        DisplayHelp(e);
                    }
                }
            }
        }
    }
}
