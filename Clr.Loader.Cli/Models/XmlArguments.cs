using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Serialization;

namespace Clr.Loader.Cli.Models
{
    [XmlRoot("xmlarguments")]
    public class XmlArguments
    {
        #region private properties

        private SecureString _secureConnectionString;
        protected string _commandString;

        #endregion

        public virtual void SetCommand(string commandString)
        {
            _commandString = commandString;
        }

        [XmlElement("commandstring")]
        public string CommandString
        {
            get
            {
                return _commandString;
            }
            set
            {
                SetCommand(value);
            }
        }
        [XmlElement("connectionstring")]
        public string ConnectionString
        {
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
        [XmlElement("assemblyname")]
        public string AssemblyName { get; set; }
    }
}
