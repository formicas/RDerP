using System;
using System.IO;
using System.Text;
using System.Windows;
using RDerP.IO;

namespace RDerP
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddDialog
    {
        public string NewName { get; set; }
        public string Host { get; set; }
        public string FullPath { get; set; }

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
                    btnOK.IsEnabled = !string.IsNullOrEmpty(nameInput.Text);
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
                MessageBox.Show(this, error, Constants.ErrorMessageTitle);
                return false;
            }

            if (ShowHost)
            {
                FullPath = Path.Combine(_path, $"{NewName}.rdp");
                if (File.Exists(FullPath))
                {
                    MessageBox.Show(this, $"{NewName} already exists.", Constants.ErrorMessageTitle);
                    return false;
                }
            }
            else
            {
                FullPath = Path.Combine(_path, NewName);
                if (Directory.Exists(FullPath))
                {
                    MessageBox.Show(this, $"{NewName} already exists.", Constants.ErrorMessageTitle);
                    return false;
                }
            }
            return true;
        }

        private void  btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                try
                {
                    if (ShowHost)
                    {
                        FileHelper.GenerateRdpFile(FullPath, Host);
                    }
                    else
                    {
                        Directory.CreateDirectory(FullPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error creating {(ShowHost ? "file" : "directory")}. See Event Logs for details.", Constants.ErrorMessageTitle);
                    Logger.LogError($"Error creating {(ShowHost ? "file" : "directory")}", ex);
                    return;
                }
                DialogResult = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
