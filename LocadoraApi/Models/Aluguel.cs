using System;

namespace LocadoraVeiculos.Models
{
    public enum StatusAluguel
    {
        Aberto = 1,
        Finalizado = 2,
        Cancelado = 3
    }

    public class Aluguel
    {
        public int AluguelId { get; set; }            
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; }

        public DateTime DataRetirada { get; set; }
        public DateTime DataPrevistaDevolucao { get; set; }

        public DateTime? DataDevolucao { get; set; }

        public int QuilometragemInicial { get; set; }
        public int? QuilometragemFinal { get; set; }

        public decimal ValorDiaria { get; set; }     
        public decimal ValorTotal { get; set; }       

        public StatusAluguel Status { get; set; } = StatusAluguel.Aberto;
        public string Observacoes { get; set; }
    }
}