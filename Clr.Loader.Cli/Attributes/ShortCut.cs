using System;

namespace Clr.Loader.Cli.Attributes
{
    public class ShortCut : Attribute
    {
        public char Key { get; }

        public ShortCut(char Key)
        {
            this.Key = Key;
        }
    }
}
