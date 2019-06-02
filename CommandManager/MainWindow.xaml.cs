using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using CommandManager.Data;
using CommandManager.Dialogs;
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
            InitSocialMedia();
            CommandList = new ObservableCollection<Command>();
            LB_Commands.ItemsSource = CommandList;
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

        public ObservableCollection<Command> CommandList = new ObservableCollection<Command>();
        private XmlSerializer xmlS = new XmlSerializer(typeof(ObservableCollection<Command>));

        private readonly static int stackCapacity = 20;
        private Stack<CommandChange> RedoStack = new Stack<CommandChange>(stackCapacity);
        private Stack<CommandChange> UndoStack = new Stack<CommandChange>(stackCapacity);

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

        public void LoadXML(string path)
        {
            var reader = new StreamReader(path);
            CommandList = (ObservableCollection<Command>)xmlS.Deserialize(reader);
            reader.Close();
            LB_Commands.ItemsSource = CommandList;
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
            LB_Commands.SelectedIndex = id; //visual improvemend
            return GetCommandById(id);
        }

        public void ShowCommandDialog_Add()
        {
            DialogCommand dlg = new DialogCommand("Add", this);
            if (dlg.ShowDialog() == true)
            {
                CommandList.Add(dlg.Command);
                LB_Commands.SelectedItem = dlg.Command;

            }
        }

        public void ShowCommandDialog_Edit(Command cmd)
        {
            DialogCommand dlg = new DialogCommand("Edit", cmd, this);
            dlg.ShowDialog();
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

        private void InitSocialMedia()
        {
            string linkTwitter = "https://twitter.com/Rhatalin";
            Btn_Twitter.Tag = linkTwitter;
            Btn_Twitter.ToolTip = linkTwitter;

            string linkGithub = "https://github.com/Rhatalin";
            Btn_Github.Tag = linkGithub;
            Btn_Github.ToolTip = linkGithub;
        }

        private void UndoCommand()
        {
            if (UndoStack.Count > 0)
            {
                //CommandChange lastChange = UndoStack.Pop();
                //Command currentCommand = GetCommandById(lastCommand.ID);

            }
            // update undo button
            if (UndoStack.Count > 0)
            {
                UndoEnabled = true;
            }
            else
            {
                UndoEnabled = false;
            }

        }

        private void RedoCommand()
        {
            // update redo button
            if (RedoStack.Count > 0)
            {
                RedoEnabled = true;
            }
            else
            {
                RedoEnabled = false;
            }
        }

        // Events

        private void Btn_AddCmd_Click(object sender, RoutedEventArgs e)
        {
            ShowCommandDialog_Add();
        }

        private void LB_Commands_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnLastDoubleClickTimestamp != e.Timestamp) // check if last double click was from a button
            {
                if (VisualTreeHelper.HitTest(this, e.GetPosition(this)).VisualHit.GetType() != typeof(ScrollViewer)) // double click on listboxitem
                {
                    Command cmd = (Command)LB_Commands.SelectedItem;
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
            CommandList.Remove(c);
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            Command c = GetCommandByButton((Button)sender);
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
            int indexOld = CommandList.IndexOf(c);
            int indexNew = indexOld - 1;
            if (indexOld > 0)
            {
                Command swap = CommandList[indexNew];
                CommandList[indexNew] = c;
                CommandList[indexOld] = swap;
                LB_Commands.SelectedIndex = indexNew; //visual improvemend
            }
            else
            {
                LB_Commands.SelectedIndex = indexOld; //visual improvemend
            }
        }

        private void Btn_Down_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            Command c = GetCommandById(id);
            int indexOld = CommandList.IndexOf(c);
            int indNew = indexOld + 1;
            if (indexOld < CommandList.Count - 1)
            {
                Command swap = CommandList[indNew];
                CommandList[indNew] = c;
                CommandList[indexOld] = swap;
                LB_Commands.SelectedIndex = indNew; //visual improvemend
            }
            else
            {
                LB_Commands.SelectedIndex = indexOld; //visual improvemend
            }
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
            Command cmd = GetCommandById((int)btn.Tag);
            cmd.ShowOutput = !cmd.ShowOutput;
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnLastDoubleClickTimestamp = e.Timestamp; // save double click timestamp
        }

        private void MI_Undo_Click(object sender, RoutedEventArgs e)
        {
            UndoCommand();
        }

        private void MI_Redo_Click(object sender, RoutedEventArgs e)
        {
            RedoCommand();
        }
    }
}
