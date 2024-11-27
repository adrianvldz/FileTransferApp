using FileTransferApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FileTransferApp.Services
{
    public class FileTransferService
    {
        // Cola de transferencias
        private ConcurrentQueue<FileTransferModel> _transferQueue = new ConcurrentQueue<FileTransferModel>();

        // Semáforo para controlar transferencias simultáneas
        private SemaphoreSlim _transferSemaphore;

        // Lista de máquinas de destino
        private List<MachineConfig> _destinationMachines;

        public FileTransferService(List<MachineConfig> machines)
        {
            _destinationMachines = machines;
            // Permitir 4 transferencias simultáneas
            _transferSemaphore = new SemaphoreSlim(4, 4);
        }

        // Encolar archivos para transferencia
        public void EnqueueFilesForTransfer(List<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                var fileInfo = new FileInfo(filePath);
                var transferModel = new FileTransferModel
                {
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    Status = TransferStatus.Pending
                };

                _transferQueue.Enqueue(transferModel);
            }
        }

        // Iniciar transferencias
        public async Task StartFileTransfersAsync()
        {
            var transferTasks = new List<Task>();

            while (_transferQueue.TryDequeue(out var transferModel))
            {
                transferModel.Status = TransferStatus.Transferring;

                transferTasks.Add(Task.Run(async () => {
                    await _transferSemaphore.WaitAsync();
                    try
                    {
                        await TransferToAllMachinesAsync(transferModel);
                    }
                    catch (Exception ex)
                    {
                        transferModel.Status = TransferStatus.Failed;
                        // Manejar error
                        MessageBox.Show($"Error en transferencia: {ex.Message}");
                    }
                    finally
                    {
                        _transferSemaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(transferTasks);
        }

        // Transferir archivo a todas las máquinas
        private async Task TransferToAllMachinesAsync(FileTransferModel fileModel)
        {
            var transferTasks = new List<Task>();

            foreach (var machine in _destinationMachines)
            {
                transferTasks.Add(TransferFileToMachineAsync(fileModel, machine));
            }

            await Task.WhenAll(transferTasks);
            fileModel.Status = TransferStatus.Completed;
        }

        // Transferir archivo a una máquina específica
        private async Task TransferFileToMachineAsync(FileTransferModel fileModel, MachineConfig destinationMachine)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(destinationMachine.IpAddress, destinationMachine.Port);

                using (var networkStream = client.GetStream())
                {
                    // Lógica de transferencia de archivo
                    string sourceFilePath = Path.Combine(destinationMachine.SendFolder, fileModel.FileName);
                    using (var fileStream = File.OpenRead(sourceFilePath))
                    {
                        // Transferir datos del archivo
                        await fileStream.CopyToAsync(networkStream);

                        // Actualizar progreso
                        fileModel.ProgressPercentage = 100;
                    }
                }
            }
        }
    }
}
