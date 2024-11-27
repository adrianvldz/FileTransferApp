using FileTransferApp.Models;
using FileTransferApp.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.CompilerServices;
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

namespace FileTransferApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private FileTransferService _fileTransferService;
        private ObservableCollection<FileTransferModel> _fileTransfers;
        private string _statusMessage;

        public ObservableCollection<FileTransferModel> FileTransfers
        {
            get => _fileTransfers;
            set
            {
                _fileTransfers = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Configuración de TODAS las máquinas
            var destinationMachines = new List<MachineConfig>
    {
        new MachineConfig
        {
            IpAddress = "192.168.1.101",
            Port = 8888,
            SendFolder = @"C:\FileTransfer\Send",
            ReceiveFolder = @"C:\FileTransfer\Received"
        },
        new MachineConfig
        {
            IpAddress = "192.168.1.102",
            Port = 8888,
            SendFolder = @"C:\FileTransfer\Send",
            ReceiveFolder = @"C:\FileTransfer\Received"
        },
        new MachineConfig
        {
            IpAddress = "192.168.1.103",
            Port = 8888,
            SendFolder = @"C:\FileTransfer\Send",
            ReceiveFolder = @"C:\FileTransfer\Received"
        },
        new MachineConfig
        {
            IpAddress = "192.168.1.104",
            Port = 8888,
            SendFolder = @"C:\FileTransfer\Send",
            ReceiveFolder = @"C:\FileTransfer\Received"
        }
    };

            // IMPORTANTE: Filtrar máquinas excluyendo la máquina actual
            var currentMachineIp = GetCurrentMachineIp();
            var filteredMachines = destinationMachines
                .Where(m => m.IpAddress != currentMachineIp)
                .ToList();

            _fileTransferService = new FileTransferService(filteredMachines);
        }

        // Método para obtener IP actual de la máquina
        private string GetCurrentMachineIp()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString();
        }
        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Todos los archivos (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _fileTransferService.EnqueueFilesForTransfer(
                    new List<string>(openFileDialog.FileNames)
                );

                // Actualizar lista de transferencias
                foreach (var fileName in openFileDialog.FileNames)
                {
                    FileTransfers.Add(new FileTransferModel
                    {
                        FileName = System.IO.Path.GetFileName(fileName),
                        FileSize = new System.IO.FileInfo(fileName).Length
                    });
                }
            }
        }

        private async void StartTransfer_Click(object sender, RoutedEventArgs e)
        {
            StatusMessage = "Iniciando transferencias...";
            await _fileTransferService.StartFileTransfersAsync();
            StatusMessage = "Transferencias completadas";
        }

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
