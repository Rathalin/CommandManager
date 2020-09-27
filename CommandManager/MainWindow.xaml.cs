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
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Init Undo Redo Management
            UndoRedoMgr = new UndoRedoManager(this);

            // Init Social Media Buttons
            InitSocialMedia();

            // Init and assign CommandList
            CommandList = new ObservableCollection<Command>();
            LB_Commands.ItemsSource = CommandList;

            // Init default paths
            pathFullDefault = pathDirectory + "\\" + filenameDefault;
            pathFullCustom = pathDirectory + "\\" + filenameCustom;

            // Create directory for saving
            Directory.CreateDirectory(pathDirectory);

            // Try to load autosaved xml file
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

            // Init show hints variable
            ShowHints = false;

            // Init Gradient Stops for Animation, when a command is executed
            stop0 = new GradientStop(Colors.Transparent, 0.0);
            stop1 = new GradientStop(Colors.Transparent, 0.003);
            stop2 = new GradientStop(Colors.Transparent, 0.997);
            stop3 = new GradientStop(Colors.Transparent, 1.0);
            RegisterName("GradientStop1", stop1);
            RegisterName("GradientStop2", stop2);

            Color_ExecAnimation = (Color)FindResource("listbox-border");
        }

        #endregion Constructors

        #region Variables and Attributes

        // Observable Collection holding the displayed commands
        public ObservableCollection<Command> CommandList = new ObservableCollection<Command>();

        // XmlSerializer
        private XmlSerializer xmlS = new XmlSerializer(typeof(ObservableCollection<Command>));

        // Capacity for Undo Redo Manager
        public readonly static int StackCapacity = 20;

        // Undo Redo Manager
        public UndoRedoManager UndoRedoMgr { get; set; }

        // Paths and filenames
        private string filenameDefault = "Autosave.xml";
        private string filenameCustom = "Commands.xml";
        private string pathDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Command Manager";
        private string pathFullDefault;
        private string pathFullCustom;

        // Button timestamp for double click checking
        private int btnLastDoubleClickTimestamp = 0;

        // Gradient stops for execution animation
        private GradientStop stop0;
        private GradientStop stop1;
        private GradientStop stop2;
        private GradientStop stop3;

        // Event Handler for Properties
        public event PropertyChangedEventHandler PropertyChanged;

        // Property for showing hints
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

        // Property for hint visibility
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

        // Property for Execution Animation Color
        public Color Color_ExecAnimation { get; set; }

        #endregion Variables and Attributes

        #region Methodes

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
                LB_Commands.SelectedIndex = indexNew; //visual improvemend
                couldMove = true;
            }
            else
            {
                LB_Commands.SelectedIndex = indexOld; //visual improvemend
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
                LB_Commands.SelectedIndex = indexNew; //visual improvemend
                couldMove = true;
            }
            else
            {
                LB_Commands.SelectedIndex = indexOld; //visual improvemend
                couldMove = false;
            }
            return couldMove;
        }

        public void Animate_Execution(Button senderBtn, Color color)
        {
            // Select the top Listbox entry which is a grid
            Grid selectedControl = (Grid)((GroupBox)((DockPanel)((DockPanel)senderBtn.Parent).Parent).Parent).Parent;

            //ListBoxItem selectedLBItem = (ListBoxItem)LB_Commands.ItemContainerGenerator.ContainerFromItem(LB_Commands.SelectedItem);

            // Create a LinearGradientBrush for the grid
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0.5);
            brush.EndPoint = new Point(1, 0.5);

            // Add predefined gradient stops
            brush.GradientStops.Add(stop0);
            brush.GradientStops.Add(stop1);
            brush.GradientStops.Add(stop2);
            brush.GradientStops.Add(stop3);

            // Apply the brush to the grid.
            selectedControl.Background = brush;

            // first animation part
            ColorAnimation animation1 = new ColorAnimation();
            animation1.From = Colors.Transparent;
            animation1.To = color;
            animation1.Duration = TimeSpan.FromSeconds(0.2);
            animation1.AutoReverse = false;
            Storyboard.SetTargetName(animation1, "GradientStop1");
            Storyboard.SetTargetProperty(animation1,
                new PropertyPath(GradientStop.ColorProperty));

            // second animation part 
            ColorAnimation animation2 = new ColorAnimation();
            animation2.From = Colors.Transparent;
            animation2.To = color;
            animation2.Duration = TimeSpan.FromSeconds(0.2);
            animation2.AutoReverse = false;
            Storyboard.SetTargetName(animation2, "GradientStop2");
            Storyboard.SetTargetProperty(animation2,
                new PropertyPath(GradientStop.ColorProperty));

            // third animation part 
            ColorAnimation animation3 = new ColorAnimation();
            animation3.From = color;
            animation3.To = Colors.Transparent;
            animation3.Duration = TimeSpan.FromSeconds(0.2);
            animation3.AutoReverse = false;
            Storyboard.SetTargetName(animation3, "GradientStop1");
            Storyboard.SetTargetProperty(animation3,
                new PropertyPath(GradientStop.ColorProperty));

            // fourth animation part 
            ColorAnimation animation4 = new ColorAnimation();
            animation4.From = color;
            animation4.To = Colors.Transparent;
            animation4.Duration = TimeSpan.FromSeconds(0.2);
            animation4.AutoReverse = false;
            Storyboard.SetTargetName(animation4, "GradientStop2");
            Storyboard.SetTargetProperty(animation4,
                new PropertyPath(GradientStop.ColorProperty));

            // Set the animation to begin after the first animation
            // ends.
            double delay = 0.0;
            animation1.BeginTime = TimeSpan.FromSeconds(delay + 0);
            animation2.BeginTime = TimeSpan.FromSeconds(delay + 0.2);
            animation3.BeginTime = TimeSpan.FromSeconds(delay + 0.3);
            animation4.BeginTime = TimeSpan.FromSeconds(delay + 0.4);


            // Create a Storyboard to apply the animations.
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation1);
            storyboard.Children.Add(animation2);
            storyboard.Children.Add(animation3);
            storyboard.Children.Add(animation4);

            // Reset brush after animation is complete
            storyboard.Completed += delegate { selectedControl.Background = Brushes.Transparent; };

            storyboard.Begin(this);
        }

        private void ResizeCommandPreview()
        {
            double resizeFactor = 1.3;
            Command.MaxPreviewColumns = (int)Math.Round(ActualWidth * resizeFactor / 10);
        }

        #endregion Methodes

        #region Events

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
            UndoRedoMgr.PushUndo(new CommandChange(CommandAction.Delete, Command.CreateCopy(c), CommandList.IndexOf(c)));
            CommandList.Remove(c);
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Command c = GetCommandByButton(btn);
            Animate_Execution(btn, Color_ExecAnimation);
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                ResizeCommandPreview();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            ResizeCommandPreview();
        }

        #endregion Events
    }
}
