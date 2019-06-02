using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandManager.Data
{
    public class CommandChange
    {
        // Constructors

        public CommandChange() { }

        public CommandChange(CommandAction action, Command command)
        {
            Action = action;
            Command = command;
        }

        // Attributes

        public CommandAction Action { get; set; }
        public Command Command { get; set; }
    }

    public enum CommandAction
    {
        Create, Update, Delete
    }
}
