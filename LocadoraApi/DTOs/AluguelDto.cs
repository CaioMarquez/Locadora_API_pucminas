using System;
using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.Models;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados recebidos para abrir um novo aluguel.</summary>
    public class AluguelCreateDto
    {
        [Required(ErrorMessage = "O cliente é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um ClienteId válido.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um VeiculoId válido.")]
        public int VeiculoId { get; set; }

        [Required(ErrorMessage = "A data de retirada é obrigatória.")]
        public DateTime DataRetirada { get; set; }

        [Required(ErrorMessage = "A data prevista de devolução é obrigatória.")]
        public DateTime DataPrevistaDevolucao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem inicial não pode ser negativa.")]
        public int QuilometragemInicial { get; set; }

        [Required(ErrorMessage = "O valor da diária é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da diária deve ser maior que zero.")]
        public decimal ValorDiaria { get; set; }

        [StringLength(300)]
        public string Observacoes { get; set; }
    }

    /// <summary>Dados recebidos para registrar a devolução de um aluguel.</summary>
    public class AluguelDevolucaoDto
    {
        [Required(ErrorMessage = "A quilometragem final é obrigatória.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem final não pode ser negativa.")]
        public int QuilometragemFinal { get; set; }

        public DateTime? DataDevolucao { get; set; }
    }

    /// <summary>Dados devolvidos pela API para um aluguel.</summary>
    public class AluguelResponseDto
    {
        public int AluguelId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public int VeiculoId { get; set; }
        public string VeiculoPlaca { get; set; }
        public string VeiculoModelo { get; set; }
        public DateTime DataRetirada { get; set; }
        public DateTime DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public int QuilometragemInicial { get; set; }
        public int? QuilometragemFinal { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }
        public string Observacoes { get; set; }
    }
}
