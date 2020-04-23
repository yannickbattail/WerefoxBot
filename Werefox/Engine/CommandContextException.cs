using System;

namespace Werefox.Engine
{
    public class CommandContextException : Exception
    {
        public CommandContextException(string message) : base(message)
        {
        }
    }
}