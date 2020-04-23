using System;

namespace WerefoxBot
{
    class CommandContextException: Exception
    {
        public CommandContextException(string message) : base(message)
        {
        }
    }
}