using System;
using System.Linq;
using System.Threading.Tasks;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>
    /// Rotas de filtro exigidas pelo item 2.5 do enunciado.
    /// São 5 filtros ao todo, usando dois tipos diferentes de JOIN:
    ///   - INNER JOIN explícito (join ... on ... equals ...)  -> filtros 1, 2 e 3
    ///   - LEFT JOIN via GroupJoin + DefaultIfEmpty()         -> filtros 4 e 5
    /// </summary>
    [ApiController]
    [Route("api/filtros")]
    public class FiltrosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FiltrosController(ApplicationContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // FILTRO 1 — INNER JOIN entre Veiculo e Fabricante, filtrando por nome do fabricante
        // GET api/filtros/veiculos-por-fabricante?nome=Toyota
        // ---------------------------------------------------------------
        [HttpGet("veiculos-por-fabricante")]
        public async Task<ActionResult> VeiculosPorFabricante([FromQuery] string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return BadRequest(new { sucesso = false, mensagem = "Informe o parâmetro 'nome' do fabricante." });

            var query =
                from v in _context.Veiculos
                join f in _context.Fabricantes on v.FabricanteId equals f.FabricanteId   // INNER JOIN
                where f.Nome.Contains(nome)
                select new
                {
                    v.VeiculoId,
                    v.Placa,
                    v.Modelo,
                    v.AnoFabricacao,
                    v.Status,
                    Fabricante = f.Nome
                };

            var resultado = await query.ToListAsync();
            return Ok(resultado);
        }

        // ---------------------------------------------------------------
        // FILTRO 2 — INNER JOIN entre Aluguel, Cliente e Veiculo, filtrando por período de retirada
        // GET api/filtros/alugueis-por-periodo?inicio=2026-01-01&fim=2026-12-31
        // ---------------------------------------------------------------
        [HttpGet("alugueis-por-periodo")]
        public async Task<ActionResult> AlugueisPorPeriodo([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            if (fim < inicio)
                return BadRequest(new { sucesso = false, mensagem = "A data 'fim' não pode ser anterior à data 'inicio'." });

            var query =
                from a in _context.Alugueis
                join c in _context.Clientes on a.ClienteId equals c.ClienteId   // INNER JOIN
                join v in _context.Veiculos on a.VeiculoId equals v.VeiculoId   // INNER JOIN
                where a.DataRetirada >= inicio && a.DataRetirada <= fim
                orderby a.DataRetirada
                select new
                {
                    a.AluguelId,
                    a.DataRetirada,
                    a.DataPrevistaDevolucao,
                    a.ValorTotal,
                    Cliente = c.Nome,
                    Veiculo = v.Modelo,
                    Placa = v.Placa
                };

            var resultado = await query.ToListAsync();
            return Ok(resultado);
        }

        // ---------------------------------------------------------------
        // FILTRO 3 — INNER JOIN entre Cliente e Aluguel, trazendo só clientes com aluguel aberto
        // GET api/filtros/clientes-com-aluguel-ativo
        // ---------------------------------------------------------------
        [HttpGet("clientes-com-aluguel-ativo")]
        public async Task<ActionResult> ClientesComAluguelAtivo()
        {
            var query =
                from c in _context.Clientes
                join a in _context.Alugueis on c.ClienteId equals a.ClienteId   // INNER JOIN
                where a.Status == StatusAluguel.Aberto
                select new
                {
                    c.ClienteId,
                    c.Nome,
                    c.Telefone,
                    AluguelId = a.AluguelId,
                    a.DataRetirada,
                    a.DataPrevistaDevolucao
                };

            var resultado = await query.ToListAsync();
            return Ok(resultado);
        }

        // ---------------------------------------------------------------
        // FILTRO 4 — LEFT JOIN entre Veiculo e Aluguel, trazendo veículos que NUNCA foram alugados
        // GET api/filtros/veiculos-sem-aluguel
        // ---------------------------------------------------------------
        [HttpGet("veiculos-sem-aluguel")]
        public async Task<ActionResult> VeiculosSemAluguel()
        {
            var query =
                from v in _context.Veiculos
                join a in _context.Alugueis on v.VeiculoId equals a.VeiculoId into alugueisDoVeiculo
                from a in alugueisDoVeiculo.DefaultIfEmpty()                    // LEFT JOIN
                where a == null
                select new
                {
                    v.VeiculoId,
                    v.Placa,
                    v.Modelo,
                    v.AnoFabricacao,
                    v.Status
                };

            var resultado = await query.ToListAsync();
            return Ok(resultado);
        }

        // ---------------------------------------------------------------
        // FILTRO 5 — LEFT JOIN entre Cliente e Aluguel, trazendo todos os clientes
        // com a quantidade de aluguéis (0 para quem nunca alugou)
        // GET api/filtros/clientes-com-total-alugueis
        // ---------------------------------------------------------------
        [HttpGet("clientes-com-total-alugueis")]
        public async Task<ActionResult> ClientesComTotalAlugueis()
        {
            var query =
                from c in _context.Clientes
                join a in _context.Alugueis on c.ClienteId equals a.ClienteId into alugueisDoCliente
                from a in alugueisDoCliente.DefaultIfEmpty()                   // LEFT JOIN
                group a by new { c.ClienteId, c.Nome, c.Email } into g
                select new
                {
                    g.Key.ClienteId,
                    g.Key.Nome,
                    g.Key.Email,
                    TotalAlugueis = g.Count(a => a != null)
                };

            var resultado = await query.OrderByDescending(r => r.TotalAlugueis).ToListAsync();
            return Ok(resultado);
        }
    }
}
