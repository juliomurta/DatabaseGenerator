using Murta.DatabaseGenerator.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Console.Models
{
    [Table("PRODUTOS")]
    public class Produtos
    {
        [Column("ID", "INTEGER")]
        [PrimaryKey]
        public int Id { get; set; }

        [Column("NOME", "VARCHAR(100)")]
        public string Nome { get; set; }

        [Column("PRECO", "DOUBLE")]
        public double Preco { get; set; }

        public Clientes Cliente { get; set; }
    }
}
