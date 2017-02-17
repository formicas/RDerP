using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RDerP.Annotations;

namespace RDerP
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddDialog
    {
        public string NewName { get; set; }
        public string Host { get; set; }

        private bool _autoGenHost = true;
        private readonly string _path;

        public AddDialog(string path)
        {
            _path = path;
            InitializeComponent();
            nameInput.Focus();

            nameInput.TextChanged += (sender, e) =>
            {
                if (_autoGenHost)
                {
                    hostInput.Text = nameInput.Text;
                }
            };

            hostInput.GotFocus += (sender, e) =>
            {
                _autoGenHost = false;
            };
            
            //PreviewKeyDown += (sender, e) =>
            //{
            //    var canClose = false;
            //    switch (e.Key)
            //    {
            //        case Key.Escape:
            //            DialogResult = false;
            //            canClose = true;
            //            break;
            //        case Key.Enter:
            //            e.SuppressKeyPress = true;
            //            if (Validate())
            //            {
            //                DialogResult = true;
            //                canClose = true;
            //            }
            //            break;
                        
            //    }

            //    if (canClose)
            //    {
            //        Close();
            //    }
            //};
        }

        private bool Validate()
        {
            var errorBuilder = new StringBuilder();
            if (string.IsNullOrWhiteSpace(NewName))
            {
                errorBuilder.AppendLine("Please enter a name.");
            }

            if (string.IsNullOrWhiteSpace(Host))
            {
                errorBuilder.AppendLine("Please enter a host.");
            }
            var error = errorBuilder.ToString();
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(this, error, "I'm afraid I can't do that");
                return false;
            }

            var filePath = Path.Combine(_path, $"{NewName}.rdp");
            if (File.Exists(filePath))
            {
                MessageBox.Show(this, $"{NewName} already exists", "I'm afraid I can't do that");
                return false;
            }

            return true;
        }

        private void  btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
                DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
