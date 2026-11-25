using System;
using System.Linq;
using System.Threading.Tasks;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/alugueis")]
    public class AlugueisController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AlugueisController(ApplicationContext context)
        {
            _context = context;
        }

        private static AluguelResponseDto ParaDto(Aluguel a) => new AluguelResponseDto
        {
            AluguelId = a.AluguelId,
            ClienteId = a.ClienteId,
            ClienteNome = a.Cliente?.Nome,
            VeiculoId = a.VeiculoId,
            VeiculoPlaca = a.Veiculo?.Placa,
            VeiculoModelo = a.Veiculo?.Modelo,
            DataRetirada = a.DataRetirada,
            DataPrevistaDevolucao = a.DataPrevistaDevolucao,
            DataDevolucao = a.DataDevolucao,
            QuilometragemInicial = a.QuilometragemInicial,
            QuilometragemFinal = a.QuilometragemFinal,
            ValorDiaria = a.ValorDiaria,
            ValorTotal = a.ValorTotal,
            Status = a.Status.ToString(),
            Observacoes = a.Observacoes
        };

        // GET api/alugueis
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var alugueis = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .OrderByDescending(a => a.DataRetirada)
                .ToListAsync();

            return Ok(alugueis.Select(ParaDto));
        }

        // GET api/alugueis/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.AluguelId == id);

            if (aluguel == null)
                return NotFound(new { sucesso = false, mensagem = $"Aluguel com id {id} não encontrado." });

            return Ok(ParaDto(aluguel));
        }

        // POST api/alugueis
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AluguelCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.DataPrevistaDevolucao < dto.DataRetirada)
                return BadRequest(new { sucesso = false, mensagem = "A data prevista de devolução não pode ser anterior à data de retirada." });

            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest(new { sucesso = false, mensagem = $"Cliente com id {dto.ClienteId} não existe." });

            var veiculo = await _context.Veiculos.FindAsync(dto.VeiculoId);
            if (veiculo == null)
                return BadRequest(new { sucesso = false, mensagem = $"Veículo com id {dto.VeiculoId} não existe." });

            // Regra de negócio: não pode alugar um veículo que já está com um aluguel aberto
            var veiculoOcupado = await _context.Alugueis
                .AnyAsync(a => a.VeiculoId == dto.VeiculoId && a.Status == StatusAluguel.Aberto);
            if (veiculoOcupado)
                return BadRequest(new { sucesso = false, mensagem = "Este veículo já está com um aluguel em aberto." });

            var diarias = Math.Max(1, (dto.DataPrevistaDevolucao.Date - dto.DataRetirada.Date).Days);
            var valorTotal = diarias * dto.ValorDiaria;

            var aluguel = new Aluguel
            {
                ClienteId = dto.ClienteId,
                VeiculoId = dto.VeiculoId,
                DataRetirada = dto.DataRetirada,
                DataPrevistaDevolucao = dto.DataPrevistaDevolucao,
                QuilometragemInicial = dto.QuilometragemInicial,
                ValorDiaria = dto.ValorDiaria,
                ValorTotal = valorTotal,
                Status = StatusAluguel.Aberto,
                Observacoes = dto.Observacoes
            };

            veiculo.Status = StatusVeiculo.Alugado;

            _context.Alugueis.Add(aluguel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = aluguel.AluguelId },
                new { aluguel.AluguelId, aluguel.ValorTotal });
        }

        // PUT api/alugueis/5 (edita datas/valor enquanto o aluguel está aberto)
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] AluguelCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var aluguel = await _context.Alugueis.FindAsync(id);
            if (aluguel == null)
                return NotFound(new { sucesso = false, mensagem = $"Aluguel com id {id} não encontrado." });

            if (aluguel.Status != StatusAluguel.Aberto)
                return BadRequest(new { sucesso = false, mensagem = "Só é possível editar um aluguel que ainda está aberto." });

            if (dto.DataPrevistaDevolucao < dto.DataRetirada)
                return BadRequest(new { sucesso = false, mensagem = "A data prevista de devolução não pode ser anterior à data de retirada." });

            var diarias = Math.Max(1, (dto.DataPrevistaDevolucao.Date - dto.DataRetirada.Date).Days);

            aluguel.DataRetirada = dto.DataRetirada;
            aluguel.DataPrevistaDevolucao = dto.DataPrevistaDevolucao;
            aluguel.QuilometragemInicial = dto.QuilometragemInicial;
            aluguel.ValorDiaria = dto.ValorDiaria;
            aluguel.ValorTotal = diarias * dto.ValorDiaria;
            aluguel.Observacoes = dto.Observacoes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/alugueis/5/devolucao — registra a devolução do veículo
        [HttpPatch("{id:int}/devolucao")]
        public async Task<ActionResult> RegistrarDevolucao(int id, [FromBody] AluguelDevolucaoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.AluguelId == id);

            if (aluguel == null)
                return NotFound(new { sucesso = false, mensagem = $"Aluguel com id {id} não encontrado." });

            if (aluguel.Status != StatusAluguel.Aberto)
                return BadRequest(new { sucesso = false, mensagem = "Este aluguel já foi finalizado ou cancelado." });

            if (dto.QuilometragemFinal < aluguel.QuilometragemInicial)
                return BadRequest(new { sucesso = false, mensagem = "A quilometragem final não pode ser menor que a inicial." });

            aluguel.DataDevolucao = dto.DataDevolucao ?? DateTime.Now;
            aluguel.QuilometragemFinal = dto.QuilometragemFinal;
            aluguel.Status = StatusAluguel.Finalizado;

            aluguel.Veiculo.Quilometragem = dto.QuilometragemFinal;
            aluguel.Veiculo.Status = StatusVeiculo.Disponivel;

            await _context.SaveChangesAsync();
            return Ok(ParaDto(aluguel));
        }

        // DELETE api/alugueis/5 (só permite cancelar, não apaga histórico)
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.AluguelId == id);

            if (aluguel == null)
                return NotFound(new { sucesso = false, mensagem = $"Aluguel com id {id} não encontrado." });

            if (aluguel.Status == StatusAluguel.Finalizado)
                return BadRequest(new { sucesso = false, mensagem = "Não é possível excluir um aluguel já finalizado. Ele faz parte do histórico." });

            aluguel.Status = StatusAluguel.Cancelado;
            if (aluguel.Veiculo != null)
                aluguel.Veiculo.Status = StatusVeiculo.Disponivel;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
