using System;

namespace Clr.Loader.Cli.Attributes
{
    public class CommandAttribute : Attribute
    {
        public char ShortCutKey { get; }
        public string Description { get; }

        public CommandAttribute(char ShortCutKey, string Description)
        {
            this.ShortCutKey = ShortCutKey;
            this.Description = Description;
        }
    }
}
