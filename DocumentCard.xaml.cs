using Open_When.Models;
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

namespace Open_When
{
    /// <summary>
    /// Interaction logic for DocumentCard.xaml
    /// </summary>
    public partial class DocumentCard : UserControl
    {
        public MainWindow MainWindow { get; set; }
        public Letter Letter { get; set; }
        public string Title
        {
            set
            {
                MainButton.Content = value;
            }
        }
        public DocumentCard()
        {
            InitializeComponent();
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.openLetter(Letter);
        }
    }
}
