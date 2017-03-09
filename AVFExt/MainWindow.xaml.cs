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
using System.IO;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;

namespace AVFExt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AVF avfFile;

        public ObservableCollection<FrameItem> frameItems = new ObservableCollection<FrameItem>();

        public MainWindow()
        {
            InitializeComponent();
            
            listBox.ItemsSource = frameItems;
            listBox.DisplayMemberPath = "name";
            listBox.SelectedValuePath = "bmp";
            listBox.SelectionMode = SelectionMode.Single;
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.DefaultExt = "*.avf";
            d.Filter = "AVF Files (*.avf)|*.avf";

            if (d.ShowDialog() == true)
            {
                openFile(d.FileName, d.SafeFileName);
            }
        }
        
        private void openFile(string fName, string safeName)
        {
            FileStream f = new FileStream(fName, FileMode.Open);

            try
            {
                avfFile = AVF.load(f, safeName);

                frameItems.Clear();

                for(int i = 0; i < avfFile.frames.Length; i++)
                {
                    FrameItem fi = new FrameItem();
                    fi.name = "Frame " + i;
                    fi.bmp = avfFile.frames[i];
                    frameItems.Add(fi);
                }

                listBox.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "File loading failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                f.Close();
            }
        }

        private void extractBtn_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog d = new VistaFolderBrowserDialog();
            
            if(d.ShowDialog() == true)
            {
                extractFiles(d.SelectedPath);
            }
        }

        private void extractFiles(string outDir)
        {
            try
            {
                avfFile.saveFrames(outDir);

                MessageBox.Show("Extracted all images to " + outDir + "!", "Extraction successful", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Extraction failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dumpBtn_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog d = new VistaFolderBrowserDialog();

            if (d.ShowDialog() == true)
            {
                dumpFiles(d.SelectedPath);
            }
        }

        private void dumpFiles(string outDir)
        {
            try
            {
                avfFile.dumpFrame(outDir, listBox.SelectedIndex);

                MessageBox.Show("Dumped selected image to " + outDir + "!", "Dump successful", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dump failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void onListBoxSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.Items.Count == 0)
            {
                extractBtn.IsEnabled = false;
                dumpBtn.IsEnabled = false;
                return;
            }

            extractBtn.IsEnabled = true;
            dumpBtn.IsEnabled = true;

            using (MemoryStream mem = new MemoryStream())
            {
                ((Bitmap)listBox.SelectedValue).Save(mem, ImageFormat.Png);
                mem.Position = 0;
                BitmapImage bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.StreamSource = mem;
                bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                bmpImg.EndInit();
                image.Source = bmpImg;
                image.Width = bmpImg.Width;
                image.Height = bmpImg.Height;
            }
        }
    }

    public struct FrameItem
    {
        public string name
        {
            get;
            set;
        }
        public Bitmap bmp
        {
            get;
            set;
        }
    }
}
