using Clr.Loader.Cli.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clr.Loader.Cli
{
    public enum Commands
    {
        None,
        Invalid,
        [ShortCut('i')]
        [Description("Installs the dll or directory of dlls into the sql server assembly and creates all the sql functions " +
            "\n\r --dllpath | -d path to dll assemlby file" +
            "\n\r --path | -p directory path of dlls " +
            "\n\r --conn | -c sql connection string " +
            "\n\r --xml  | -x path to xml file containing agurments " +
            "\n\r --help | -h")]

        install,
        [ShortCut('u')]
        [Description("Uninstalls The specific assembly name and all the related functions " +
            "\r\n --assemblyname | -a The name of the assembly to be removed" +
            "\n\r --help | -h")]
        uninstall,
        [ShortCut('v')]
        [Description("Creates a sql output of all the sql functions in the assembly" +
            "\n\r --dllpath | -d path to dll assemlby file" +
            "\n\r --help | -h")]
        view,
    }

}
