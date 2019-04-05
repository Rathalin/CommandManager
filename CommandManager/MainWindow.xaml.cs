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
            ShowHints = false;
        }

        // Variables and Attributes

        public ObservableCollection<Command> CommandList = new ObservableCollection<Command>();
        private XmlSerializer xmlS = new XmlSerializer(typeof(ObservableCollection<Command>));
        private string filenameDefault = "Autosave.xml";
        private string filenameCustom = "Commands.xml";
        private string pathDirectory = Directory.GetCurrentDirectory();
        private string pathFullDefault;
        private string pathFullCustom;
        public event PropertyChangedEventHandler PropertyChanged;
        private int btnLastDoubleClickTimestamp = 0;

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

        public Command SelectCommandFromButton(Button btn)
        {
            int id = (int)btn.Tag;
            LB_Commands.SelectedIndex = id; //visual improvemend
            return GetCommandById(id);
        }

        public void LB_Commands_AddNew()
        {
            DialogCommand dlg = new DialogCommand(this);
            if (dlg.ShowDialog() == true)
            {
                CommandList.Add(dlg.Command);
                LB_Commands.SelectedItem = dlg.Command;
                
            }
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

        // Events

        private void Btn_AddCmd_Click(object sender, RoutedEventArgs e)
        {
            LB_Commands_AddNew();
        }

        private void LB_Commands_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnLastDoubleClickTimestamp != e.Timestamp) // check if last double click was from a button
            {
                double height = 0;
                foreach (Command c in CommandList)
                {
                    var item = (ListBoxItem)LB_Commands.ItemContainerGenerator.ContainerFromItem(c);
                    height += item.ActualHeight;
                }

                // check if mouseevent happend over a listbox item
                int heigthFix = 2; // somehow the height is 2 pixles to small
                if (e.GetPosition(this).Y <= LB_Commands.TransformToAncestor(this).Transform(new Point(0, 0)).Y + height + heigthFix)
                {
                    Command cmd = (Command)LB_Commands.SelectedItem;
                    DialogCommand dlg = new DialogCommand(cmd, this);
                    dlg.ShowDialog();

                }
                else // add item dialog
                {
                    LB_Commands_AddNew();
                }
            }
            e.Handled = true;
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
                SaveXML(saveDlg.FileName);
            }

        }

        private void Btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            Command c = SelectCommandFromButton((Button)sender);
            DialogCommand dlg = new DialogCommand(c, this);
            dlg.ShowDialog();

        }

        private void Btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            Command c = SelectCommandFromButton((Button)sender);
            CommandList.Remove(c);
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            Command c = SelectCommandFromButton((Button)sender);
            ExecuteScript(c);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveXML(pathFullDefault);
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

        private void GB_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Command c = GetCommandById((int)((GroupBox)sender).Tag);
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
    }
}
