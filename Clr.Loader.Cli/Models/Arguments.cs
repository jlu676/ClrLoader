using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using NDesk.Options;
using Clr.Loader.Cli.Helpers;
using Clr.Loader.Cli.Attributes;


namespace Clr.Loader.Cli.Models
{
    
    public class Arguments : XmlArguments
    {
        #region constructors

        public Arguments()
        {
            Command = Commands.None;
        }

        public Arguments(string[] args)
        {
            Command = Commands.None;
            ParseArgs(args);
        }

        #endregion

        #region properties

        public string XmlFilePath { get; set; }

        public Commands Command { get; set; }
        public string InvalidCommand { get; set; }
        public Boolean Help { get; set; }
        public string[] ExtraArguments { get; set; }

        #endregion

        #region private methods 

        public override void SetCommand(string commandString)
        {
            base.SetCommand(commandString);
            Command = ParseCommand(_commandString);
        }

        private void ParseArgs(string[] args, bool HasCommand = true)
        {
            ExtraArguments = new OptionSet(){
                { "xml|x=",x=> XmlFilePath = IoHelper.PathFormater(x)},
                { "conn|c=",x=> ConnectionString = x },
                { "path|p=", x=> Path = IoHelper.PathFormater(x) },
                { "dir|d=", x=> Directory = x },
                { "assemblyname|a=", x=> AssemblyName = x },
                { "command|C=", x=> CommandString = x },
                { "h|?|help", v => Help = v != null}
            }.Parse(args).ToArray();


            if (ExtraArguments.Count() > 0)
            {
                Command = Commands.ExtraArguments;
            }
            else if (!string.IsNullOrEmpty(XmlFilePath))
            {
                ParseXmlArguments();
            }
        }

        private void ParseXmlArguments()
        {
            using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(XmlFilePath))))
            {
                var xmlType = typeof(XmlArguments);
                var serializer = new XmlSerializer(xmlType);
                var result = Convert.ChangeType(serializer.Deserialize(reader), xmlType);
                var properties = xmlType.GetProperties();

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

        private Commands ParseCommand(string commandString)
        {
            Commands command = Commands.None;

            if (commandString.Length <= 1)
            {
                var shortcutCommand = typeof(Commands).GetFields().Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).Count() >= 1)
                    .Where(x => ((CommandAttribute)x.GetCustomAttributes(typeof(CommandAttribute), false).SingleOrDefault()).ShortCutKey == commandString.ToLower().ToCharArray()[0]).SingleOrDefault();

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

            command = command == Commands.None ? Commands.Invalid : command;

            if (Command == Commands.Invalid)
            {
                InvalidCommand = commandString;
                Help = true;
            }

            return command;
        }

        #endregion
    }
}
