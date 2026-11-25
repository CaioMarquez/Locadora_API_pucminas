using System;
using System.Collections.Generic;

namespace LocadoraVeiculos.Models
{
    public class Cliente
    {
        public int ClienteId { get; set; }         

        public string Nome { get; set; }        
        public string Cpf { get; set; }              
        public string Email { get; set; }             
        public string Telefone { get; set; }
        public string NumeroCnh { get; set; }
        public DateTime? DataNascimento { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Propriedade de navegação (1:N)
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}