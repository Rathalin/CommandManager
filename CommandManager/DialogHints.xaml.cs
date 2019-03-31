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

namespace CommandManager
{
    /// <summary>
    /// Interaction logic for DialogHints.xaml
    /// </summary>
    public partial class DialogHints : Window
    {
        public DialogHints(Window owner)
        {
            Owner = owner;
            InitializeComponent();
        }
    }
}
