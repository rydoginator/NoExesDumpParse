using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using MessageBox = System.Windows.MessageBox;

namespace NoExesDumpParse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private SolidColorBrush ChangeColor(byte r, byte g, byte b)
        {
            return (new SolidColorBrush(Color.FromRgb(r, g, b)));
        }

        private bool ReadMagic(String path)
        {
            BinaryReader fs = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            byte[] buffer = new byte[8];
            fs.Read(buffer, 0, 4);
            Array.Reverse(buffer, 0, 4);
            int magic = BitConverter.ToInt32(buffer, 0);
            return (magic == 0x4E444D50);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = @"";
            fileDialog.Filter = "NoexsDumpFile(*.dmp)|*.dmp|All Files(*.*)|*.*";
            fileDialog.FilterIndex = 1;
            fileDialog.Title = "select Noexs dump file";
            if (fileDialog.ShowDialog() == true)
            {
                FilePathBox.Text = fileDialog.FileName;
                if (ReadMagic(fileDialog.FileName))
                {
                    FilePathBox.Background = ChangeColor(0x68, 0xFF, 0);
                }
                else
                {
                    FilePathBox.Background = ChangeColor(0, 0xFF, 0);
                }
            }
        }

        // ( ͡° ͜ʖ ͡°)
        private void ButtAnal_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            if (MainBoxStart.Text == "")
            {
                MainBoxStart.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            if (MainBoxEnd.Text == "")
            {
                MainBoxEnd.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            if (HeapBoxStart.Text == "")
            {
                HeapBoxStart.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            if (HeapBoxEnd.Text == "")
            {
                HeapBoxEnd.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            if (!valid)
            {
                MessageBox.Show("Please fill out all the fields.", "Error");
            }
        }

        private void MainBoxStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainBoxStart.Background = ChangeColor(0xFF, 0xFF, 0xFF);
        }

        private void MainBoxEnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainBoxEnd.Background = ChangeColor(0xFF, 0xFF, 0xFF);
        }

        private void HeapBoxStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            HeapBoxStart.Background = ChangeColor(0xFF, 0xFF, 0xFF); 
        }

        private void HeapBoxEnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            HeapBoxEnd.Background = ChangeColor(0xFF, 0xFF, 0xFF);
        }
    }
}
