using System.Collections.Generic;

namespace LocadoraVeiculos.Models
{
    public enum StatusVeiculo
    {
        Disponivel = 1,
        Alugado = 2,
        Manutencao = 3,
        Inativo = 4
    }
    public class Veiculo
    {
        public int VeiculoId { get; set; }          

        public string Placa { get; set; }            
        public string Modelo { get; set; }          
        public int AnoFabricacao { get; set; }         
        public int Quilometragem { get; set; }        
        public string Cor { get; set; }
        public string Chassi { get; set; }
        public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

        // FK obrigatória -> Fabricante
        public int FabricanteId { get; set; }
        public Fabricante Fabricante { get; set; }

        // Propriedade de navegação (1:N)
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}