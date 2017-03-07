using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using FontStyle = System.Drawing.FontStyle;
using MessageBox = System.Windows.MessageBox;

namespace FontConverter
{
    public partial class MainWindow : MetroWindow
    {
        private readonly Methods _methods = new Methods();
        private readonly List<FontStyle> _fontStyle = new List<FontStyle>();
        private readonly BackgroundWorker _worker;
        private List<FontFamily> _fonts;
        private Color _forground;
        private Color _background;
        private int _fontsize;
        private string _path;
        private int _styleIndex;
        private int _fontIndex;

        public MainWindow()
        {
            InitializeComponent();
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += WorkerOnDoWork;
            _worker.ProgressChanged += WorkerOnProgressChanged;
            _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
        }

        /// <summary>
        ///     Load Fonts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFonts_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            _fonts = installedFontCollection.Families.ToList();
            foreach (FontFamily font in _fonts)
            {
                CbFonts.Items.Add(font.Name);
            }
            int index = CbFonts.Items.IndexOf("Segoe MDL2 Assets");
            CbFonts.SelectedIndex = index == -1 ? 0 : index;
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        ///     Path for save directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbPath_Loaded(object sender, RoutedEventArgs e)
        {
            TbPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        ///     Open path to convert font
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbPath.Text != "")
                {
                    Process.Start("explorer.exe", TbPath.Text);
                }
            }
            catch (Exception exc)
            {
                await this.ShowMessageAsync("FontConverter Error", "Can't open folder. Please check your path.\n" + exc.Message);
                //MessageBox.Show("Can't open folder. Please check your path.\n" + exc.Message, "FontConverter Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        ///     Convert font to picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btConvert_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            BtBrowse.IsEnabled = false;
            BtOpen.IsEnabled = false;
            BtConvert.IsEnabled = false;
            tbPercent.Text = "0  %";
            _fontIndex = CbFonts.SelectedIndex;
            _styleIndex = CbStyle.SelectedIndex;
            _path = _methods.CreatePath(_fonts, CbFonts.SelectedIndex, TbPath.Text, _fontsize, _fontStyle[CbStyle.SelectedIndex]);
            char[] allChar = _methods.GetAllChar(_fonts, CbFonts.SelectedIndex);
            _worker.RunWorkerAsync(allChar);
        }

        /// <summary>
        ///     Worker completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="runWorkerCompletedEventArgs"></param>
        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Mouse.OverrideCursor = null;
            BtBrowse.IsEnabled = true;
            BtOpen.IsEnabled = true;
            BtConvert.IsEnabled = true;
            tbPercent.Text = "100 %";
        }

        /// <summary>
        ///     Progresschanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progressChangedEventArgs"></param>
        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            ProgressBar.Value = progressChangedEventArgs.ProgressPercentage;
            tbPercent.Text = $"{progressChangedEventArgs.ProgressPercentage} %";
        }

        /// <summary>
        ///     Backgroundwoker main thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        private async void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            try
            {
                Methods methods = new Methods();
                char[] allChar = (char[]) doWorkEventArgs.Argument;
                int i = 0;
                foreach (char c in allChar)
                {
                    methods.DrawImage(_fonts, _fontIndex, _path, _fontsize, _fontStyle[_styleIndex], _background, _forground, c, i);
                    double percentage = (double)i/allChar.Length*100.0;
                    _worker.ReportProgress((int)percentage);
                    i++;
                }
            }
            catch (Exception exc)
            {
                Mouse.OverrideCursor = null;
                await this.ShowMessageAsync("FontConverter Error", "Can't convert font to picture\n" + exc.Message);
                //MessageBox.Show("Can't convert font to picture\n" + exc.Message, "FontConverter Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        ///     Close Application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MiClose_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        ///     Set backgroundcolor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CpBackground_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            _background = Color.FromArgb
                    (
                        CpBackground.SelectedColor.Value.A
                        ,
                        CpBackground.SelectedColor.Value.R
                        ,
                        CpBackground.SelectedColor.Value.G
                        ,
                        CpBackground.SelectedColor.Value.B
                    );
        }

        /// <summary>
        ///     Set foregroundcolor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CpForeground_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            _forground = Color.FromArgb
                    (
                        CpForeground.SelectedColor.Value.A
                        ,
                        CpForeground.SelectedColor.Value.R
                        ,
                        CpForeground.SelectedColor.Value.G
                        ,
                        CpForeground.SelectedColor.Value.B
                    );
        }

        //private void UdSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    if (UdSize.Value == null)
        //    {
        //        _fontsize = 50;
        //    }
        //    else
        //    {
        //        _fontsize = (int)UdSize.Value;
        //    }
        //}

        /// <summary>
        ///     Load foregroundcolor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CpForeground_OnLoaded(object sender, RoutedEventArgs e)
        {
            _forground = Color.FromArgb(CpForeground.SelectedColor.Value.A, CpForeground.SelectedColor.Value.R,
                CpForeground.SelectedColor.Value.G, CpForeground.SelectedColor.Value.B);
        }

        /// <summary>
        ///     Load backgroundcolor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CpBackground_OnLoaded(object sender, RoutedEventArgs e)
        {
            _background = Color.FromArgb(CpBackground.SelectedColor.Value.A, CpBackground.SelectedColor.Value.R,
                CpBackground.SelectedColor.Value.G, CpBackground.SelectedColor.Value.B);
        }

        /// <summary>
        ///     Show application version
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MiAbout_OnClick(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string programmName = assembly.GetName().Name;
            string programmVersion = assembly.GetName().Version.ToString();
            string programmDate = "März 2017";
            await this.ShowMessageAsync($"About {programmName}", $"{programmName} Version: {programmVersion} \n{programmDate}\nAlexander Spindler");
            //MessageBox.Show($"{programmName} Version: {programmVersion} \n{programmDate}\nAlexander Spindler", $"About {programmName}", MessageBoxButton.OK);
        }

        /// <summary>
        ///     Get path to save folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog objDialog = new FolderBrowserDialog
            {
                Description = "Opening save path for pictures",
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            DialogResult objResult = objDialog.ShowDialog();
            if (objResult == System.Windows.Forms.DialogResult.OK)
            {
                TbPath.Text = objDialog.SelectedPath;
            }
        }

        /// <summary>
        ///     Load styles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbStyle_OnLoaded(object sender, RoutedEventArgs e)
        {
            CbStyle.Items.Add(System.Drawing.FontStyle.Regular.ToString());
            _fontStyle.Add(System.Drawing.FontStyle.Regular);
            CbStyle.Items.Add(System.Drawing.FontStyle.Bold.ToString());
            _fontStyle.Add(System.Drawing.FontStyle.Bold);
            CbStyle.Items.Add(System.Drawing.FontStyle.Italic.ToString());
            _fontStyle.Add(System.Drawing.FontStyle.Italic);
            CbStyle.Items.Add(System.Drawing.FontStyle.Strikeout.ToString());
            _fontStyle.Add(System.Drawing.FontStyle.Strikeout);
            CbStyle.Items.Add(System.Drawing.FontStyle.Underline.ToString());
            _fontStyle.Add(System.Drawing.FontStyle.Underline);
            CbStyle.SelectedIndex = 0;
        }

        /// <summary>
        ///     Set fontsize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UdSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (UdSize.Value == null)
            {
                _fontsize = 50;
            }
            else
            {
                _fontsize = (int)UdSize.Value;
            }
        }

        /// <summary>
        ///     Set default fontsize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UdSize_OnLoaded(object sender, RoutedEventArgs e)
        {
            _fontsize = 50;
            UdSize.Value = 50;
        }
    }
}