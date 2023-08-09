using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banco.Models
{
    public class CajaDeAhorro
    {
        public int id { get; set; }
        public int? cbu { get; set; }
        public float? saldo { get; set; }
        public int idUsuario { get; set; }
        public string? TitularNombre { get; set; } 
        public string? TitularApellido { get; set; } 
    
        public List<Movimiento>? movimientos { get; set; }
        public ICollection<Usuario>? titulares { get; } = new List<Usuario>();
        public List<UsuarioCaja>? usuarioCajas { get; set; }
        
        public  CajaDeAhorro()
        {
            titulares = new List<Usuario>();
            movimientos = new List<Movimiento>();
        }
        public CajaDeAhorro(int Cbu, Usuario Titular)//Constructor alternativo
        {
            this.cbu = Cbu;
            this.saldo = 0;
            this.titulares.Add(Titular);
            movimientos = new List<Movimiento>();
        }

        public override string ToString()
        {
            return string.Format("CBU: {0}, Saldo: {1}", this.cbu, this.saldo);
        }
    }
}
