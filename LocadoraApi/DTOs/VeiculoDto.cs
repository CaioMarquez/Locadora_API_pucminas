using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.Models;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados recebidos para criar/atualizar um veículo.</summary>
    public class VeiculoDto
    {
        [Required(ErrorMessage = "A placa é obrigatória.")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "A placa deve ter entre 7 e 8 caracteres.")]
        public string Placa { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [StringLength(80)]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "O ano de fabricação é obrigatório.")]
        [Range(1950, 2100, ErrorMessage = "O ano de fabricação deve estar entre 1950 e 2100.")]
        public int AnoFabricacao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa.")]
        public int Quilometragem { get; set; }

        [StringLength(30)]
        public string Cor { get; set; }

        [StringLength(17, MinimumLength = 17, ErrorMessage = "O chassi deve ter 17 caracteres.")]
        public string Chassi { get; set; }

        public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

        [Required(ErrorMessage = "O fabricante é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um FabricanteId válido.")]
        public int FabricanteId { get; set; }
    }

    /// <summary>Dados devolvidos pela API para um veículo.</summary>
    public class VeiculoResponseDto
    {
        public int VeiculoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public int AnoFabricacao { get; set; }
        public int Quilometragem { get; set; }
        public string Cor { get; set; }
        public string Chassi { get; set; }
        public string Status { get; set; }
        public int FabricanteId { get; set; }
        public string FabricanteNome { get; set; }
    }
}
