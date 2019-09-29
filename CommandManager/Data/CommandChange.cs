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

        public CommandChange(CommandAction action, Command command, int lastIndex)
        {
            Action = action;
            Command = command;
            LastIndex = lastIndex;
        }

        // Attributes

        public CommandAction Action { get; set; }
        public Command Command { get; set; }
        public int LastIndex { get; set; }
    }

    public enum CommandAction
    {
        Create, Update, Delete, MoveUp, MoveDown, ChangeOutputState
    }
}
