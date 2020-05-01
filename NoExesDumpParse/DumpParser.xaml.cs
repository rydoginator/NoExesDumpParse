using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public class ColumnData
    {
        public String ramStart { get; set; }
        public String ramEnd { get; set; }
        public String region { get; set; }

        public String fPos { get; set; }
        public String size { get; set; }
    }
    public partial class MainWindow : Window
    {
        ObservableCollection<ColumnData> _collection = new ObservableCollection<ColumnData>();
        private IDumpDataReader reader;
        private List<NoexsDumpIndex> _dumps = new List<NoexsDumpIndex>();
        public MainWindow()
        {
            InitializeComponent();
            OffsetPathsGrid.ItemsSource = _collection;
        }

        private bool range(long low, long high, long addr)
        {
            return (addr >= low && addr <= high);
        }

        /*
         * GetPos will get the file position based on physical address
         */
        private long GetPos(long addr)
        {
            if (reader != null)
            {
                if (_dumps != null)
                {
                    // loop through the collection 
                    for (int i = 0; i < _dumps.Count; i++)
                    {
                        long address = _dumps[i].address;
                        // check if it's in range
                        if (range(address, (address + _dumps[i].size), addr))
                        {
                            // get the file position based on how many bytes you are away from the start of the memory pool
                            long _pos = _dumps[i].pos + (addr - _dumps[i].address);
                            return _pos;
                        }
                    }
                }
            }
            // return that it wasn't found
            return -1;
        }

        /*
         * GetAddr will get the physical address based on file position
         */
        private long GetAddr(long pos)
        {
            if (reader != null)
            {
                if (_dumps != null)
                {
                    for (int i = 0; i < _dumps.Count; i++)
                    {
                        long _pos = _dumps[i].pos;
                        if (range(_pos, (_pos + _dumps[i].size), pos))
                        {
                            // get your address based on your file position - the pool's start
                            long _addr = _dumps[i].address + (pos - _pos);
                            return _addr;
                        }
                    }
                }
            }
            return -1;
        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = OffsetPathsGrid.SelectedIndex;
            if (reader != null)
            {
                if (_dumps != null)
                {
                    GameOffsetBox.Text = _dumps[i].address.ToString("X");
                    FileOffsetBox.Text = _dumps[i].pos.ToString("X");
                }
                else
                {
                    _dumps = reader.Read();
                }
            }
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
                    MessageBox.Show("Not a valid NoExes RAM Dump !", "Error");
                    FilePathBox.Background = ChangeColor(0, 0xFF, 0);
                }
            }
        }

        // ( ͡° ͜ʖ ͡°)
        private void ButtAnal_Click(object sender, RoutedEventArgs e)
        {
            reader = CreateDumpDataReader();
            if (reader == null)
            {
                System.Media.SystemSounds.Asterisk.Play();
                MessageBox.Show("The red fields are invalid. Please try again.", "Error");
                return;
            }
            _collection.Clear();
            _dumps.Clear();
            _dumps = reader.Read();
            foreach (NoexsDumpIndex x in _dumps)
            {
                _collection.Add(new ColumnData()
                {
                    ramStart = x.address.ToString("X"),
                    ramEnd = (x.address + x.size).ToString("X"),
                    region = reader.IsHeap(x.address) ? "heap" : "main",
                    size = x.size.ToString("X"),
                    fPos = x.pos.ToString("X")
                });
            }
        }

        private IDumpDataReader CreateDumpDataReader()
        {
            bool valid = true;
            String path = "";
            long mainStart = -1;
            long mainEnd = -1;
            long heapStart = -1;
            long heapEnd = -1;

            if (FilePathBox.Background == ChangeColor(0xFF, 0, 0))
            {
                valid = false;
            }
            else
            {
                path = FilePathBox.Text;
            }
            try
            {
                mainStart = Convert.ToInt64(MainBoxStart.Text, 16);
            }
            catch
            {
                MainBoxStart.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            try
            {
                mainEnd = Convert.ToInt64(MainBoxEnd.Text, 16);
            }
            catch
            {
                MainBoxEnd.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            try
            {
                heapStart = Convert.ToInt64(HeapBoxStart.Text, 16);
            }
            catch
            {
                HeapBoxStart.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            try
            {
                heapEnd = Convert.ToInt64(HeapBoxEnd.Text, 16);
            }
            catch
            {
                HeapBoxEnd.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            if (mainEnd <= mainStart)
            {
                MainBoxStart.Background = ChangeColor(0xFF, 0, 0);
                MainBoxEnd.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            if (heapEnd <= heapStart)
            {
                HeapBoxStart.Background = ChangeColor(0xFF, 0, 0);
                HeapBoxEnd.Background = ChangeColor(0xFF, 0, 0);
                valid = false;
            }
            if (!valid)
            {
                return null;
            }
            return new NoexsDumpDataReader(path, mainStart, mainEnd, heapStart, heapEnd);
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

        private void OffsetPathsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //Modify the header 
            if (e.Column.Header.ToString() == "fPos")
            {
                e.Column.Header = "File Position";
                e.Column.IsReadOnly = true;
            }
            if (e.Column.Header.ToString() == "ramStart")
            {
                e.Column.Header = "RAM Start";
                e.Column.IsReadOnly = true;
            }
            if (e.Column.Header.ToString() == "ramEnd")
            {
                e.Column.Header = "RAM End";
                e.Column.IsReadOnly = true;
            }
            if (e.Column.Header.ToString() == "region")
            {
                e.Column.Header = "Region";
                e.Column.IsReadOnly = true;
            }
            if (e.Column.Header.ToString() == "size")
            {
                e.Column.Header = "Size";
                e.Column.IsReadOnly = true;
            }
        }

        private void GameOffsetBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GameOffsetBox.Text != "")
            {
                // convert from hex string to long
                long pos = GetPos(Convert.ToInt64(GameOffsetBox.Text, 16));
                if (pos != -1)
                {
                    FileOffsetBox.Text = pos.ToString("X");
                }
            }
        }

        private void FileOffsetBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FileOffsetBox.Text != "")
            {
                // convert from hex string to long
                long addr = GetAddr(Convert.ToInt64(FileOffsetBox.Text, 16));
                if (addr != -1)
                {
                    GameOffsetBox.Text = addr.ToString("X");
                }
            }

        }

        private long GetOperand(String operand)
        {
            long offset = 0;
            switch (operand.ToLower())
            {
                case "main":
                    offset = reader.GetMain();
                    break;
                case "heap":
                    offset = reader.GetHeap();
                    break;
                default:
                    offset = long.Parse(operand, System.Globalization.NumberStyles.HexNumber);
                    break;
            }
            return offset;
        }
        private long Arithemetic(String[] operands)
        {
            long offset = GetOperand(operands[0]);
            if (operands.Length == 1)
            {
                return offset;
            }
            switch (operands[1])
            {
                case "+":
                    offset += GetOperand(operands[2]);
                    break;
                case "-":
                    offset -= GetOperand(operands[2]);
                    break;
                case "*":
                    offset *= GetOperand(operands[2]);
                    break;
                case "/":
                    offset /= GetOperand(operands[2]);
                    break;
            }
            return offset;
        }

        private long FollowPointerPath(List<String> path)
        {
            long offset = 0;
            String tmp;
            bool read = false;
            foreach (String p in path)
            {
                tmp = p;
                // start pointer reading
                if (p.StartsWith("["))
                {
                    // remove the square brackets
                    tmp = p.Substring(1, p.Length - 2);
                    read = true;
                }
                // split the operands up
                String[] operands = Regex.Split(tmp, @"\s*([-+/*])\s*");
                // operands ouside of a nest will be formatted like +23, to the loperand will be blank
                if (operands[0] == "")
                {
                    operands[0] = offset.ToString("X");
                }
                offset = Arithemetic(operands); 
                if (read)
                {
                    offset = reader.ReadLittleEndianInt64(offset);
                    read = false;
                }
            }
            return offset;
        }

        /*
         * SplitOffsets will split pointer paths into a list of strings
         * ex: [[[main+12345]+90]+23]+60
         * will output
            [main+12345]
            [+90]
            [+23]
            +60
         */
        private List<String> SplitOffset(String path)
        {
            String tmp, res;
            List<String> result = new List<String>();
            int depth = 0;
            int end = 0;
            foreach (char c in path)
            {
                if (c == '[')
                    depth++;
                if (c == ']')
                    end++;
            }
            if (depth != end)
            {
                result.Add("Invalid Syntax");
                return result;
            }
            tmp = path;
            while (depth > 0)
            {
                int index = tmp.IndexOf(']');
                res = tmp.Substring(depth - 1, index - depth + 2);
                result.Add(res);
                depth--;
                tmp = tmp.Remove(depth, index - depth + 1);
            }
            if (tmp != "")
                result.Add(tmp);
            return result;
        }

        private void ButtParse_Click(object sender, RoutedEventArgs e)
        {
            List<String> paths = SplitOffset(ExpressionBox.Text);
            if (reader != null)
            {
                if (paths[0] == "Invalid Syntax")
                {
                    ResultBox.Text = paths[0];
                    return;
                }
                else
                {

                    ResultBox.Text = FollowPointerPath(paths).ToString("X");
                }
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
                MessageBox.Show("Please analyze your dump first.", "Error");
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/rydoginator/NoExesDumpParse");
        }
    }
}
