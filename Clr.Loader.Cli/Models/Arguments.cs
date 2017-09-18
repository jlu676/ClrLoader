using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Serialization;
using System.Linq;
using NDesk.Options;
using Clr.Loader.Cli.Helpers;
using Clr.Loader.Cli.Attributes;


namespace Clr.Loader.Cli.Models
{
    [XmlRoot("clrarguments")]
    public class Arguments
    {

        private SecureString _secureConnectionString;

        public Arguments()
        {
            Command = Commands.None;
        }

        public Arguments(string[] args)
        {
            Command = Commands.None;
            ParseCommand(args);
            ParseArgs(args);

            if (!string.IsNullOrEmpty(XmlFilePath))
            {
                ParseXmlArguments();
            }
        }

        private void ParseCommand(string[] args)
        {
            var commandString = args.Length >= 1 ? args[0].Trim().ToLower() : "";

            Commands command = Commands.None;

            if (commandString.Length <= 1)
            {
                var shortcutCommand = typeof(Commands).GetFields().Where(x => x.GetCustomAttributes(typeof(ShortCut), false).Count() >= 1)
                    .Where(x=> ((ShortCut)x.GetCustomAttributes(typeof(ShortCut), false).SingleOrDefault()).Key == commandString.ToLower().ToCharArray()[0]).SingleOrDefault();

                if (shortcutCommand != null)
                {
                    command = (Commands)shortcutCommand.GetValue(shortcutCommand);
                }
                else
                {
                    command = Commands.None;
                }
            }
            else
            {
                Enum.TryParse(commandString, out command);             
            }

            Command = command == Commands.None ? Commands.Invalid : command;

            if  (Command == Commands.Invalid)
            {
                InvalidCommand =  commandString;
                Help = true;
            }           
        }

        private void ParseArgs(string[] args)
        {
            //remove command string
            args = args.Where((val, idx) => idx != 0).ToArray();

            var extras = new OptionSet(){
                { "xml|x=",x=> XmlFilePath = IoHelper.PathFormater(x)},
                { "conn|c=",x=> ConnectionString = x },
                { "path|p=", x=> Path = IoHelper.PathFormater(x) },
                { "dir|d=", x=> Directory = x },
                { "assemblyname|a=", x=> AssemblyName = x },
                { "h|?|help", v => Help = v != null}
            }.Parse(args);
        }

        private void ParseXmlArguments()
        {
            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(XmlFilePath))))
            {
                var serializer = new XmlSerializer(GetType());
                var result = (Arguments)serializer.Deserialize(reader);
                var properties = typeof(Arguments).GetProperties().Where(x => x.IsDefined(typeof(XmlElementAttribute), false));

                foreach (var property in properties)
                {
                    var value = property.GetValue(result);

                    if ((value != null && !(value is string)) || (value is string && !string.IsNullOrEmpty(value.ToString())))
                    {
                        property.SetValue(this, value);
                    }
                }
            }
        }
        public string XmlFilePath { get; set; }
        public Commands Command { get; set; }
        public string InvalidCommand { get; set; }
        public Boolean Help { get; set; }
        [XmlElement("ConnectionString")]
        public string ConnectionString {
            get
            {
                if (_secureConnectionString != null)
                {
                    IntPtr valuePtr = IntPtr.Zero;
                    try
                    {
                        valuePtr = Marshal.SecureStringToGlobalAllocUnicode(_secureConnectionString);
                        return Marshal.PtrToStringUni(valuePtr);
                    }
                    finally
                    {
                        Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                    }
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (_secureConnectionString != null)
                    {
                        _secureConnectionString.Clear();
                    }
                    else
                    {
                        _secureConnectionString = new SecureString();
                    }
                    foreach (char c in value)
                        _secureConnectionString.AppendChar(c);

                    _secureConnectionString.MakeReadOnly();
                }
            }
        }
        [XmlElement("directory")]
        public string Directory { get; set; }
        [XmlElement("path")]
        public string Path { get; set; }
        public string AssemblyName { get; set; }
    }
}
