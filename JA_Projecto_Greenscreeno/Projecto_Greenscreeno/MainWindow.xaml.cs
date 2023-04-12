using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;

namespace JA_Projecto_Greenscreeno
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Bitmap input = new Bitmap(1,1);
        private Bitmap output = new Bitmap(1,1);
        Stopwatch watch = new Stopwatch();

        [DllImport("CppDll.dll")]
        public static extern unsafe void processPicture(byte* byteArray, uint start, uint end, float threshold);

        [DllImport("AsmDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void processPictureAsm(byte* byteArray, uint start, uint end, float threshold);

        public MainWindow()
        {
            InitializeComponent();
            runButton.IsEnabled = false;
            saveButton.IsEnabled = false;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            threadsSlider.Value = Environment.ProcessorCount;
        }

        private void openFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Image";
            openFileDialog.Filter = "Image File|*bmp; *.gif; *.jpg; *.jpeg; *.png";

            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    input = new Bitmap(openFileDialog.FileName);
                    inputImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                    outputImage.Source = new BitmapImage();
                    runButton.IsEnabled = true;
                    saveButton.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong, open cannot be done.", "Error");
            }
        }

        private void runAlgorithm(object sender, RoutedEventArgs e)
        {
            loadButton.IsEnabled = false;
            runButton.IsEnabled = false;
            
            output = (Bitmap)input.Clone(new Rectangle(0, 0, input.Width, input.Height), PixelFormat.Format32bppArgb);

            BitmapData outputData = output.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int height = output.Height;
            int width = output.Width;
            int stride = outputData.Stride;
            uint ustride = (uint)stride;
            uint uheight = (uint)height;
            byte[] byteArray = new byte[stride * height];

            Marshal.Copy(outputData.Scan0, byteArray, 0, stride * height);

            List<uint> splittedArray = new List<uint>();
            for(uint i = 0; i < (ustride * uheight); i += ustride)
            {
                splittedArray.Add(i);
            }

            watch.Restart();

            ParallelOptions howManyThreads = new ParallelOptions { MaxDegreeOfParallelism = (int)threadsSlider.Value };

            float threshold = (float)thresholdSlider.Value;
            if (cppDll.IsChecked == true)
            {
                watch.Start();

                Parallel.ForEach(splittedArray, howManyThreads, index =>
                {
                    RunCppDll(byteArray, index, index + ustride, threshold);
                });
                
                watch.Stop();
            }
            else {
                watch.Start();

                Parallel.ForEach(splittedArray, howManyThreads, index =>
                {
                    RunAsmDll(byteArray, index, index + ustride, threshold);
                });
                
                watch.Stop();
            }
            
            Marshal.Copy(byteArray, 0, outputData.Scan0, stride * height);

            output.UnlockBits(outputData);
            outputImage.Source = BitmapToImageSource(output);
            timer.Text = watch.Elapsed.TotalMilliseconds + " milliseconds";
            loadButton.IsEnabled = true;
            runButton.IsEnabled = true;
            saveButton.IsEnabled = true;
        }

        private void saveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save Image";
            saveFileDialog.Filter = "Image File|*.png";

            try
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    output.Save(saveFileDialog.FileName, ImageFormat.Png);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong, save cannot be done.", "Error");
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitMapImage = new BitmapImage();
                bitMapImage.BeginInit();
                bitMapImage.StreamSource = memory;
                bitMapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitMapImage.EndInit();
                return bitMapImage;
            }
        }

        private void RunAsmDll(byte[] arrayPart, uint start, uint end, float threshold)
        {
            unsafe
            {
                fixed (byte* byteArrayPtr = &arrayPart[0])
                {
                    processPictureAsm(byteArrayPtr, start, end, threshold);
                }
            }
        }

        private void RunCppDll(byte[] arrayPart, uint start, uint end, float threshold)
        {
            unsafe
            {
                fixed (byte* byteArrayPtr = &arrayPart[0])
                {
                    processPicture(byteArrayPtr, start, end, threshold);
                }
            }
        }
    }
}
