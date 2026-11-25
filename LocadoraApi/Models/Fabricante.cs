using System.Collections.Generic;

namespace LocadoraVeiculos.Models
{
    public class Fabricante
    {
        public int FabricanteId { get; set; }          // PK

        public string Nome { get; set; }               // obrigatório, único
        public string PaisOrigem { get; set; }
        public bool Ativo { get; set; } = true;

        // Propriedade de navegação (1:N)
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}