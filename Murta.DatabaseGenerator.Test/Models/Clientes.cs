using Murta.DatabaseGenerator.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Test.Models
{
    [Table("CLIENTES")]
    public class Clientes
    {
        [PrimaryKey]
        [Column("ID", "INTEGER")]
        public int Id { get; set; }

        [Column("NOME", "VARCHAR(100)")]
        public string Nome { get; set; }

        [Column("NASCIMENTO", "VARCHAR(10)")]
        public string Nascimento { get; set; }

        [Column("CPF", "VARCHAR(20)")]
        public string CPF { get; set; }

        public Produtos Produto { get; set; }
    }
}
