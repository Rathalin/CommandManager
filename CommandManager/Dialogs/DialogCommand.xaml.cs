using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using CommandManager.Data;
using System.IO;
using Microsoft.Win32;
using static CommandManager.Dialogs.DialogUniversal;

namespace CommandManager.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogCommand.xaml
    /// </summary>
    public partial class DialogCommand : Window
    {
        // Constructors

        public DialogCommand(Command commandScript, Window owner) : this(owner)
        {
            Command = commandScript;
            TB_Name.Text = Command.Name;
            TB_Desc.Text = Command.Description;
            TB_Command.Text = Command.Script;
        }

        public DialogCommand(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            Command = new Command();
        }

        // Attributes 

        public Command Command { get; }

        // Methodes

        // Events

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            Command.Name = TB_Name.Text;
            Command.Description = TB_Desc.Text;
            Command.Script = TB_Command.Text;
            DialogResult = true;
            Close();
        }

        private void Btn_LoadBatch_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.InitialDirectory = Directory.GetCurrentDirectory();
            openDlg.DefaultExt = ".bat";
            openDlg.Filter = "batch file (*.bat)|*.bat| txt file (*.txt)|*.txt| All files (*.*)|*.*";
            openDlg.AddExtension = true;
            if (openDlg.ShowDialog() == true)
            {
                TB_Command.Text = File.ReadAllText(openDlg.FileName);
            }
        }
    }
}
