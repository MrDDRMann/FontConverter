using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using FontStyle = System.Drawing.FontStyle;
using MessageBox = System.Windows.MessageBox;

namespace FontConverter
{
    public partial class MainWindow : Window
    {
        private readonly Methods _methods = new Methods();
        private readonly List<FontStyle> _fontStyle = new List<FontStyle>();
        private List<FontFamily> _fonts;
        private Color _forground;
        private Color _background;
        private int _fontsize;

        public MainWindow()
        {
            InitializeComponent();
        }

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

        private void tbPath_Loaded(object sender, RoutedEventArgs e)
        {
            TbPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void btOpen_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show("Can't open folder. Please check your path.\n" + exc.Message, "FontConverter Error", MessageBoxButton.OK);
            }
        }

        private void btConvert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                string path = _methods.CreatePath(_fonts, CbFonts.SelectedIndex, TbPath.Text, _fontsize, _fontStyle[CbStyle.SelectedIndex]);
                char[] allChar = _methods.GetAllChar(_fonts, CbFonts.SelectedIndex);
                _methods.DrawImage(_fonts, CbFonts.SelectedIndex, path, _fontsize, _fontStyle[CbStyle.SelectedIndex], _background, _forground, allChar);
                Mouse.OverrideCursor = null;
            }
            catch (Exception exc)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("Can't convert font to picture\n" + exc.Message, "FontConverter Error", MessageBoxButton.OK);
            }
        }

        private
            void MiClose_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown
                ();
        }

        private void CpBackground_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            _background
                =
                Color.FromArgb
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

        private void CpForeground_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            _forground
                =
                Color.FromArgb
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

        private void UdSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (
                UdSize.Value == null)
            {
                _fontsize
                    = 50;
            }
            else
            {
                _fontsize
                    = (
                        int
                        )
                        UdSize.Value;
            }
        }

        private void CpForeground_OnLoaded(object sender, RoutedEventArgs e)
        {
            _forground = Color.FromArgb(CpForeground.SelectedColor.Value.A, CpForeground.SelectedColor.Value.R,
                CpForeground.SelectedColor.Value.G, CpForeground.SelectedColor.Value.B);
        }

        private void CpBackground_OnLoaded(object sender, RoutedEventArgs e)
        {
            _background = Color.FromArgb(CpBackground.SelectedColor.Value.A, CpBackground.SelectedColor.Value.R,
                CpBackground.SelectedColor.Value.G, CpBackground.SelectedColor.Value.B);
        }

        private void UdSize_OnLoaded(object sender, RoutedEventArgs e)
        {
            _fontsize = 50;
        }

        private void MiAbout_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("FontConverter Version 1.2 \nNovember 2016\nAlexander Spindler", " About FontConverter",
                MessageBoxButton.OK);
        }

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
    }
}