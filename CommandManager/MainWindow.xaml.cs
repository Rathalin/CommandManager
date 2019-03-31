using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace CommandManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
                // No Autosave created when program first started
            }
        }

        // Variables and Attributes

        public ObservableCollection<Command> CommandList = new ObservableCollection<Command>();
        private XmlSerializer xmlS = new XmlSerializer(typeof(ObservableCollection<Command>));
        private string filenameDefault = "Autosave.xml";
        private string filenameCustom = "Commands.xml";
        private string pathDirectory = Directory.GetCurrentDirectory();
        private string pathFullDefault;
        private string pathFullCustom;

        // Methodes

        public void LoadTestCommands()
        {
            CommandList.Add(new Command("Shutdown 30", "shut down the pc after 30 seconds", "shutdown /s /t 30"));
            CommandList.Add(new Command("Start Vagrant", "starts the virtual machine for hypermedia", "C:\r\ncd hgbdev\r\nvagrant up"));
        }

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
            ExecuteScript(command.Script);
        }

        public void ExecuteScript(string script)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + script;
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

        // Events

        private void Btn_AddCmd_Click(object sender, RoutedEventArgs e)
        {
            DialogCommand dlg = new DialogCommand(this);
            if (dlg.ShowDialog() == true)
            {
                CommandList.Add(dlg.Command);
            }
        }

        private void LB_Commands_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Command cmd = (Command)LB_Commands.SelectedItem;
            DialogCommand dlg = new DialogCommand(cmd, this);
            dlg.ShowDialog();
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
                LoadXML(openDlg.FileName);
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

        private void MI_ShowHints_Click(object sender, RoutedEventArgs e)
        {
            DialogHints dlg = new DialogHints(this);
            dlg.Show();
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

        private void GroupBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Command c = GetCommandById((int)((GroupBox)sender).Tag);
            var result = MessageBox.Show("Permanently delete the command " + c.Name + "?", "Remove", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                CommandList.Remove(c);
            }
        }
    }
}
