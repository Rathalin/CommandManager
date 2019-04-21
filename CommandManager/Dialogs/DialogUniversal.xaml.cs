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

namespace CommandManager.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogUniversal.xaml
    /// </summary>
    public partial class DialogUniversal : Window
    {
        // Constructors

        public DialogUniversal(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }

        public DialogUniversal(string message, Window owner) : this(owner)
        {
            TB_Message.Text = message;
        }

        public DialogUniversal(string message, string title, Window owner) : this(message, owner)
        {
            Title = title;
        }

        public DialogUniversal(string message, string title, List<BtnData> btnDataList, Window owner) : this(message, title, owner)
        {
            BtnDataList = btnDataList;
        }

        // Attributes and Variables

        private string _buttonDefaultStyle = "btn-secondary";
        private string[] _buttonDefaultLabels = new string[] { "Ok", "Cancel" };
        private List<BtnData> _btnDataList;
        public List<BtnData> BtnDataList
        {
            get { return _btnDataList; }
            set
            {
                _btnDataList = value;
                List<Button> buttons = new List<Button>();
                foreach (BtnData btnData in value)
                {
                    Button btn = new Button();
                    try
                    {
                        btn.Style = (Style)FindResource(btnData.StaticResourceKey);
                    }
                    catch (Exception)
                    {
                        btn.Style = (Style)FindResource(_buttonDefaultStyle);
                    }
                    btn.Content = btnData.Text;
                    btn.Click += delegate (object sender, RoutedEventArgs e)
                    {
                        DialogResult = btnData.BtnDialogResult;
                    };
                    btn.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                    btn.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Center);
                    btn.SetValue(MinWidthProperty, 100.0);
                    SP_Buttons.Children.Add(btn);
                }
            }
        }

        // Methodes

        // Subclasses

        public class BtnData
        {
            public BtnData() { }

            public BtnData(string text, string staticResourceKey, bool? dialogResult)
            {
                Text = text;
                StaticResourceKey = staticResourceKey;
                BtnDialogResult = dialogResult;
            }

            public string Text { get; set; }
            public string StaticResourceKey { get; set; }
            public bool? BtnDialogResult { get; set; }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
