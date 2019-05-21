using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.Models
{
    public class FichajeDTO
    {
        public string id { get; set; }
        public string dni { get; set; }
        public string nombre { get; set; }
        public string fechaentrada { get; set; }
        public string horaentrada { get; set; }
        public string fechasalida { get; set; }
        public string horasalida { get; set; }
        public string horastrabajadas { get; set; }
    }
}
