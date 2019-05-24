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

namespace Set_Parser
{
    /// <summary>
    /// Interaction logic for TextOutput.xaml
    /// </summary>
    public partial class TextOutput : Window
    {
        public TextOutput()
        {
            InitializeComponent();
        }

        public TextOutput(string input)
        {
            InitializeComponent();
            tbkOutput.Text = input;
        }
    }
}
