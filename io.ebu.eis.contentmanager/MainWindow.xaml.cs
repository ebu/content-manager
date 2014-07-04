using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace io.ebu.eis.contentmanager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ManagerContext context;
        
        public MainWindow()
        {
            InitializeComponent();

            // Data Context
            context = new ManagerContext();
            DataContext = context;

            context.DummyData();

        }
    }
}
