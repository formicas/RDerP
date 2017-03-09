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
        private readonly ItemType _type;

        private bool ShowHost => _type == ItemType.Rdp;

        public AddDialog(string path, ItemType type = ItemType.Rdp)
        {
            _path = path;
            _type = type;
            
            InitializeComponent();
            hostInput.Visibility = ShowHost ? Visibility.Visible : Visibility.Collapsed;
            hostLabel.Visibility = hostInput.Visibility;

            nameInput.Focus();

            if (ShowHost)
            {
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
            }
        }

        private bool Validate()
        {
            var errorBuilder = new StringBuilder();
            if (string.IsNullOrWhiteSpace(NewName))
            {
                errorBuilder.AppendLine("Please enter a name.");
            }

            if (ShowHost && string.IsNullOrWhiteSpace(Host))
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
