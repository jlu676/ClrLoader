using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Clr.Cli
{
    public class Arguments
    {

        private SecureString _secureConnectionString;

        public Arguments()
        {
            Command = Commands.None;
        }

        public Arguments(string commandString)
        {
            var command = Commands.None;
            Enum.TryParse(commandString, out command);
            Command = command == Commands.None ? Commands.Invalid : command;
            InvalidCommand = command == Commands.Invalid ? commandString : "";
        }

        public Commands Command { get; set; }
        public string InvalidCommand { get; set; }
        public Boolean Help { get; set; }
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
        public string Directory { get; set; }
        public string Path { get; set; }
        public string Assembly { get; set; }
    }
}
