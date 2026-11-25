using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados recebidos para criar/atualizar um fabricante.</summary>
    public class FabricanteDto
    {
        [Required(ErrorMessage = "O nome do fabricante é obrigatório.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 80 caracteres.")]
        public string Nome { get; set; }

        [StringLength(60)]
        public string PaisOrigem { get; set; }

        public bool Ativo { get; set; } = true;
    }

    /// <summary>Dados devolvidos pela API para um fabricante.</summary>
    public class FabricanteResponseDto
    {
        public int FabricanteId { get; set; }
        public string Nome { get; set; }
        public string PaisOrigem { get; set; }
        public bool Ativo { get; set; }
        public int QuantidadeVeiculos { get; set; }
    }
}
