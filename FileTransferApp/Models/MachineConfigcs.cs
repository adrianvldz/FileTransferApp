using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferApp.Models
{
    // Modelo para configuración de máquinas en la red
    public class MachineConfig
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string SendFolder { get; set; }
        public string ReceiveFolder { get; set; }
    }
}
