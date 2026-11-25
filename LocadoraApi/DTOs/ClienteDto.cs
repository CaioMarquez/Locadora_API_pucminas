using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados recebidos para criar/atualizar um cliente.</summary>
    public class ClienteDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter 11 dígitos numéricos.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter apenas 11 dígitos numéricos, sem pontos ou traços.")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(120)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [StringLength(11)]
        public string NumeroCnh { get; set; }

        public System.DateTime? DataNascimento { get; set; }
    }

    /// <summary>Dados devolvidos pela API para um cliente.</summary>
    public class ClienteResponseDto
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string NumeroCnh { get; set; }
        public System.DateTime? DataNascimento { get; set; }
        public System.DateTime DataCadastro { get; set; }
    }
}
