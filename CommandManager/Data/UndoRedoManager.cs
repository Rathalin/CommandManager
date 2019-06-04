using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandManager.Data
{
    public class UndoRedoManager : INotifyPropertyChanged
    {
        // Constructors 

        public UndoRedoManager(MainWindow window)
        {
            this.window = window;
            UndoStack = new Stack<CommandChange>(MainWindow.StackCapacity);
            RedoStack = new Stack<CommandChange>(MainWindow.StackCapacity);
        }

        // Attributes and Variables

        private MainWindow window;

        public Stack<CommandChange> UndoStack { get; private set; }
        public Stack<CommandChange> RedoStack { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _undoEnabled = false;
        public bool UndoEnabled
        {
            get { return _undoEnabled; }
            set
            {
                _undoEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UndoEnabled"));
            }
        }

        private bool _redoEnabled = false;
        public bool RedoEnabled
        {
            get { return _redoEnabled; }
            set
            {
                _redoEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RedoEnabled"));
            }
        }

        // Methodes

        public void PushUndo(CommandChange change)
        {
            UndoStack.Push(change);
            if (RedoEnabled)
            {
                RedoStack.Clear();
                RedoEnabled = RedoStack.Count > 0;
            }
            UndoEnabled = UndoStack.Count > 0;
        }

        public CommandChange PopUndo()
        {
            UndoEnabled = UndoStack.Count - 1 > 0;
            return UndoStack.Pop();
        }

        public void PushRedo(CommandChange change)
        {
            RedoStack.Push(change);
            RedoEnabled = RedoStack.Count > 0;
        }

        public CommandChange PopRedo()
        {
            RedoEnabled = RedoStack.Count - 1 > 0;
            return RedoStack.Pop();
        }

        public void UndoCommand()
        {
            if (UndoStack.Count > 0)
            {
                CommandChange lastChange = UndoStack.Pop();
                Command affectedCommand = window.GetCommandById(lastChange.Command.ID);
                switch (lastChange.Action)
                {
                    case CommandAction.Create:
                        window.CommandList.Remove(affectedCommand);
                        RedoStack.Push(new CommandChange(CommandAction.Create, affectedCommand, lastChange.LastIndex));
                        RedoEnabled = RedoStack.Count > 0;
                        window.LB_Commands.SelectedItem = lastChange.Command;
                        break;
                    case CommandAction.Update:
                        RedoStack.Push(new CommandChange(CommandAction.Update, Command.CreateCopy(affectedCommand), lastChange.LastIndex));
                        RedoEnabled = RedoStack.Count > 0;
                        affectedCommand.Name = lastChange.Command.Name;
                        affectedCommand.Description = lastChange.Command.Description;
                        affectedCommand.Script = lastChange.Command.Script;
                        window.LB_Commands.SelectedItem = affectedCommand;
                        break;
                    case CommandAction.Delete:
                        window.CommandList.Insert(lastChange.LastIndex, lastChange.Command);
                        RedoStack.Push(new CommandChange(CommandAction.Delete, lastChange.Command, lastChange.LastIndex));
                        RedoEnabled = RedoStack.Count > 0;
                        window.LB_Commands.SelectedItem = lastChange.Command;
                        window.LB_Commands.UpdateLayout();
                        break;
                    case CommandAction.MoveUp:

                        break;
                    case CommandAction.MoveDown:

                        break;
                }
            }
            // update undo button
            UndoEnabled = UndoStack.Count > 0;
        }

        public void RedoCommand()
        {
            if (RedoStack.Count > 0)
            {
                CommandChange lastChange = RedoStack.Pop();
                Command affectedCommand = window.GetCommandById(lastChange.Command.ID);
                switch (lastChange.Action)
                {
                    case CommandAction.Create:
                        window.CommandList.Insert(lastChange.LastIndex, lastChange.Command);
                        UndoStack.Push(new CommandChange(CommandAction.Create, lastChange.Command, lastChange.LastIndex));
                        UndoEnabled = UndoStack.Count > 0;
                        window.LB_Commands.SelectedItem = lastChange.Command;
                        break;
                    case CommandAction.Update:
                        UndoStack.Push(new CommandChange(CommandAction.Update, Command.CreateCopy(affectedCommand), lastChange.LastIndex));
                        UndoEnabled = UndoStack.Count > 0;
                        affectedCommand.Name = lastChange.Command.Name;
                        affectedCommand.Description = lastChange.Command.Description;
                        affectedCommand.Script = lastChange.Command.Script;
                        window.LB_Commands.SelectedItem = affectedCommand;
                        break;
                    case CommandAction.Delete:
                        window.CommandList.Remove(affectedCommand);
                        UndoStack.Push(new CommandChange(CommandAction.Delete, affectedCommand, lastChange.LastIndex));
                        UndoEnabled = UndoStack.Count > 0;
                        window.LB_Commands.SelectedItem = lastChange.Command;
                        break;
                    case CommandAction.MoveUp:

                        break;
                    case CommandAction.MoveDown:

                        break;
                }
            }
            // update redo button
            RedoEnabled = RedoStack.Count > 0;
        }
    }
}
