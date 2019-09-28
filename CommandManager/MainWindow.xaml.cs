using CommandManager.Data;
using CommandManager.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Serialization;
using static CommandManager.Dialogs.DialogUniversal;

namespace CommandManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            UndoRedoMgr = new UndoRedoManager(this);
            InitSocialMedia();
            SelectedListBox.ItemsSource = CommandList;

            Testcontent();
            pathFullDefault = pathDirectory + "\\" + filenameDefault;
            pathFullCustom = pathDirectory + "\\" + filenameCustom;
            Directory.CreateDirectory(pathDirectory);
            try
            {
                LoadXML(pathFullDefault);
            }
            catch (FileNotFoundException)
            {
                // No Autosave yet created when program first started
            }
            catch (InvalidOperationException)
            {
                // Error in XML file
            }
            catch (UnauthorizedAccessException)
            {
                // Permission Denied
            }
            ShowHints = false;
        }

        // Variables and Attributes

        public ObservableCollection<CommandGroup> CommandGroups = new ObservableCollection<CommandGroup>();
        public ObservableCollection<Command> CommandList = new ObservableCollection<Command>();
        public ListBox SelectedListBox = new ListBox();
        private XmlSerializer xmlS = new XmlSerializer(typeof(ObservableCollection<Command>));

        public readonly static int StackCapacity = 20;
        public UndoRedoManager UndoRedoMgr { get; set; }

        private string filenameDefault = "Autosave.xml";
        private string filenameCustom = "Commands.xml";
        private string pathDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Command Manager";
        private string pathFullDefault;
        private string pathFullCustom;

        private int btnLastDoubleClickTimestamp = 0;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _showHints;
        public bool ShowHints
        {
            get { return _showHints; }
            set
            {
                _showHints = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowHints"));
                if (_showHints)
                {
                    HintVisibility = Visibility.Visible;
                }
                else
                {
                    HintVisibility = Visibility.Collapsed;
                }
            }
        }

        private Visibility _hintVisibility;
        public Visibility HintVisibility
        {
            get { return _hintVisibility; }
            set
            {
                _hintVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HintVisibility"));
            }
        }

        // Methodes

        private void Testcontent()
        {

            CommandGroups.Add(new CommandGroup()
            {
                CommandList = new ObservableCollection<Command>()
                {
                    new Command("Command One", "Exampledesc 1", "", false),
                    new Command("Command Two", "Exampledesc 2", "", true),
                    new Command("Command Three", "Exampledesc 3", "", false)
                },
                Name = "Group1"
            });
            CommandGroups.Add(new CommandGroup()
            {
                CommandList = new ObservableCollection<Command>()
                {
                    new Command("Command One :)", "Exampledesc 1", "", false),
                    new Command("Command Two :=)", "Exampledesc 2", "", true),
                    new Command("Command Three :(", "Exampledesc 3", "", false)
                },
                Name = "Group2"
            });
        }

        public void LoadXML(string path)
        {
            var reader = new StreamReader(path);
            CommandList = (ObservableCollection<Command>)xmlS.Deserialize(reader);
            reader.Close();
            SelectedListBox.ItemsSource = CommandList;
        }

        public void SaveXML(string path)
        {
            var writer = new StreamWriter(path);
            xmlS.Serialize(writer, CommandList);
            writer.Close();
        }

        public void ExecuteScript(Command command)
        {
            ExecuteScript(command.Script, command.ShowOutput);
        }

        public void ExecuteScript(string script, bool showOutput)
        {
            script = script.Replace((char)10, '&'); // replace LF with &
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            string startArg = "/C";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (showOutput)
            {
                startArg = "/K";
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
            }
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = startArg + script;
            process.StartInfo = startInfo;
            process.Start();
        }

        private void InitSocialMedia()
        {
            string linkTwitter = "https://twitter.com/Rhatalin";
            Btn_Twitter.Tag = linkTwitter;
            Btn_Twitter.ToolTip = linkTwitter;

            string linkGithub = "https://github.com/Rhatalin";
            Btn_Github.Tag = linkGithub;
            Btn_Github.ToolTip = linkGithub;
        }

        public Command GetCommandById(int id)
        {
            foreach (Command c in CommandList)
            {
                if (c.ID == id)
                    return c;
            }
            return null;
        }

        public Command GetCommandByButton(Button btn)
        {
            int id = (int)btn.Tag;
            SelectedListBox.SelectedIndex = id; //visual improvemend
            return GetCommandById(id);
        }

        public void ShowCommandDialog_Add()
        {
            DialogCommand dlg = new DialogCommand("Add", this);
            if (dlg.ShowDialog() == true)
            {
                CommandList.Add(dlg.Command);
                SelectedListBox.SelectedItem = dlg.Command;
                UndoRedoMgr.PushUndo(new CommandChange(CommandAction.Create, Command.CreateCopy(dlg.Command), CommandList.IndexOf(dlg.Command)));
                
            }
        }

        public void ShowCommandDialog_Edit(Command cmd)
        {
            // save values to detect changes
            Command oldCommand = Command.CreateCopy(cmd);
            DialogCommand dlg = new DialogCommand("Edit", cmd, this);
            dlg.ShowDialog();
            if (!dlg.Command.Equals(oldCommand)) // command was changed in dialog
            {
                UndoRedoMgr.PushUndo(new CommandChange(CommandAction.Update, Command.CreateCopy(oldCommand), CommandList.IndexOf(cmd)));
            }
        }

        public void ShowCommandDialog_Remove(Command c)
        {
            List<BtnData> dlgButtons = new List<BtnData>();
            dlgButtons.Add(new BtnData("Remove", "btn-danger", true));
            dlgButtons.Add(new BtnData("Cancel", "btn-secondary", false));
            DialogUniversal dlg = new DialogUniversal("Permanently delete the command " + c.Name + "?", "Delete", dlgButtons, this);
            dlg.Height = 150;
            dlg.Width = 350;
            if (dlg.ShowDialog() == true)
            {
                UndoRedoMgr.PushUndo(new CommandChange(CommandAction.Delete, Command.CreateCopy(c), CommandList.IndexOf(c)));
                CommandList.Remove(c);
            }
        }

        public void ShowUniversalDialog_PermissionDenied()
        {
            List<BtnData> buttons = new List<BtnData>() { new BtnData("OK", "btn-primary", true) };
            DialogUniversal dlg = new DialogUniversal("Permission Denied! Try running as administrator\n\nCould not save to " + pathFullDefault, "Save Error", buttons, this);
            dlg.Height = 250;
            dlg.Width = 350;
            dlg.ShowDialog();
        }

        public bool Command_MoveUp(Command c)
        {
            bool couldMove;
            int indexOld = CommandList.IndexOf(c);
            int indexNew = indexOld - 1;
            if (indexOld > 0)
            {
                Command swap = CommandList[indexNew];
                CommandList[indexNew] = c;
                CommandList[indexOld] = swap;
                SelectedListBox.SelectedIndex = indexNew; //visual improvemend
                couldMove = true;
            }
            else
            {
                SelectedListBox.SelectedIndex = indexOld; //visual improvemend
                couldMove = false;
            }
            return couldMove;
        }

        public bool Command_MoveDown(Command c)
        {
            bool couldMove;
            int indexOld = CommandList.IndexOf(c);
            int indexNew = indexOld + 1;
            if (indexOld < CommandList.Count - 1)
            {
                Command swap = CommandList[indexNew];
                CommandList[indexNew] = c;
                CommandList[indexOld] = swap;
                SelectedListBox.SelectedIndex = indexNew; //visual improvemend
                couldMove = true;
            }
            else
            {
                SelectedListBox.SelectedIndex = indexOld; //visual improvemend
                couldMove = false;
            }
            return couldMove;
        }

        // Events

        private void Btn_AddCmd_Click(object sender, RoutedEventArgs e)
        {
            ShowCommandDialog_Add();
        }

        private void SelectedListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnLastDoubleClickTimestamp != e.Timestamp) // check if last double click was from a button
            {
                if (VisualTreeHelper.HitTest(this, e.GetPosition(this)).VisualHit.GetType() != typeof(ScrollViewer)) // double click on listboxitem
                {
                    Command cmd = (Command)SelectedListBox.SelectedItem;
                    ShowCommandDialog_Edit(cmd);
                }
                else // double click on empty space in listbox
                {
                    ShowCommandDialog_Add();
                }
            }
        }

        private void LB_Commands_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnLastDoubleClickTimestamp != e.Timestamp) // check if last double click was from a button
            {
                if (VisualTreeHelper.HitTest(this, e.GetPosition(this)).VisualHit.GetType() != typeof(ScrollViewer)) // double click on listboxitem
                {
                    Command cmd = (Command)SelectedListBox.SelectedItem;
                    ShowCommandDialog_Edit(cmd);
                }
                else // double click on empty space in listbox
                {
                    ShowCommandDialog_Add();
                }
            }
        }

        private void MI_Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = pathDirectory;
            openDlg.DefaultExt = ".xml";
            openDlg.Filter = "xml file (*.xml)|*.xml";
            openDlg.AddExtension = true;
            openDlg.FileName = filenameCustom;
            if (openDlg.ShowDialog() == true)
            {
                try
                {
                    LoadXML(openDlg.FileName);
                }
                catch (InvalidOperationException)
                {
                    List<BtnData> dlgButtons = new List<BtnData>();
                    dlgButtons.Add(new BtnData("Ok", "btn-primary", true));
                    DialogUniversal dlg = new DialogUniversal("Error in parsing XML file!", "Invalid XML", dlgButtons, this);
                    dlg.Height = 150;
                    dlg.Width = 280;
                    dlg.ShowDialog();
                }
                catch (UnauthorizedAccessException)
                {
                    ShowUniversalDialog_PermissionDenied();
                }
            }
        }

        private void MI_Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.FileName = filenameCustom;
            saveDlg.InitialDirectory = pathDirectory;
            saveDlg.DefaultExt = ".xml";
            saveDlg.Filter = "xml file (*.xml)|*.xml";
            saveDlg.AddExtension = true;
            if (saveDlg.ShowDialog() == true)
            {
                Directory.CreateDirectory(pathDirectory);
                try
                {
                    SaveXML(saveDlg.FileName);
                }
                catch (UnauthorizedAccessException)
                {
                    ShowUniversalDialog_PermissionDenied();
                }
            }

        }

        private void Btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            Command c = GetCommandByButton((Button)sender);
            ShowCommandDialog_Edit(c);
        }

        private void Btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            Command c = GetCommandByButton((Button)sender);
            UndoRedoMgr.PushUndo(new CommandChange(CommandAction.Delete, Command.CreateCopy(c), CommandList.IndexOf(c)));
            CommandList.Remove(c);
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Command c = GetCommandByButton(btn);
            ExecuteScript(c);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Directory.CreateDirectory(pathDirectory);
            try
            {
                SaveXML(pathFullDefault);
            }
            catch (UnauthorizedAccessException)
            {
                ShowUniversalDialog_PermissionDenied();
            }
        }

        private void Btn_Up_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            Command c = GetCommandById(id);
            Command_MoveUp(c);
            UndoRedoMgr.PushUndo(new CommandChange(CommandAction.MoveUp, c, CommandList.IndexOf(c)));
        }

        private void Btn_Down_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            Command c = GetCommandById(id);
            Command_MoveDown(c);
            UndoRedoMgr.PushUndo(new CommandChange(CommandAction.MoveDown, c, CommandList.IndexOf(c)));
        }

        private void G_Wrapping_PreviwMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Command c = GetCommandById((int)((Grid)sender).Tag);
            ShowCommandDialog_Remove(c);
        }

        private void Btn_SocialMedia_Click(object sender, RoutedEventArgs e)
        {
            string uri = ((Button)sender).Tag.ToString();
            Process.Start(uri);
        }

        private void Btn_OutputState_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Command c = GetCommandById((int)btn.Tag);
            UndoRedoMgr.PushUndo(new CommandChange(CommandAction.ChangeOutputState, c, CommandList.IndexOf(c)));
            c.ShowOutput = !c.ShowOutput;
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnLastDoubleClickTimestamp = e.Timestamp; // save double click timestamp
        }

        private void MI_Undo_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoMgr.UndoCommand();
        }

        private void MI_Redo_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoMgr.RedoCommand();
        }

        private void TabControl_CommandGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show(sender.GetType().ToString());
        }
    }
}
