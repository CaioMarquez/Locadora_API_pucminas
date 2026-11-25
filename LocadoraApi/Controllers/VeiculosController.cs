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
    [Route("api/veiculos")]
    public class VeiculosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public VeiculosController(ApplicationContext context)
        {
            _context = context;
        }

        private static VeiculoResponseDto ParaDto(Veiculo v) => new VeiculoResponseDto
        {
            VeiculoId = v.VeiculoId,
            Placa = v.Placa,
            Modelo = v.Modelo,
            AnoFabricacao = v.AnoFabricacao,
            Quilometragem = v.Quilometragem,
            Cor = v.Cor,
            Chassi = v.Chassi,
            Status = v.Status.ToString(),
            FabricanteId = v.FabricanteId,
            FabricanteNome = v.Fabricante != null ? v.Fabricante.Nome : null
        };

        // GET api/veiculos
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var veiculos = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Select(v => new VeiculoResponseDto
                {
                    VeiculoId = v.VeiculoId,
                    Placa = v.Placa,
                    Modelo = v.Modelo,
                    AnoFabricacao = v.AnoFabricacao,
                    Quilometragem = v.Quilometragem,
                    Cor = v.Cor,
                    Chassi = v.Chassi,
                    Status = v.Status.ToString(),
                    FabricanteId = v.FabricanteId,
                    FabricanteNome = v.Fabricante.Nome
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        // GET api/veiculos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .FirstOrDefaultAsync(v => v.VeiculoId == id);

            if (veiculo == null)
                return NotFound(new { sucesso = false, mensagem = $"Veículo com id {id} não encontrado." });

            return Ok(ParaDto(veiculo));
        }

        // POST api/veiculos
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] VeiculoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.FabricanteId == dto.FabricanteId);
            if (!fabricanteExiste)
                return BadRequest(new { sucesso = false, mensagem = $"Fabricante com id {dto.FabricanteId} não existe." });

            var placaEmUso = await _context.Veiculos.AnyAsync(v => v.Placa == dto.Placa);
            if (placaEmUso)
                return Conflict(new { sucesso = false, mensagem = $"Já existe um veículo com a placa '{dto.Placa}'." });

            var veiculo = new Veiculo
            {
                Placa = dto.Placa,
                Modelo = dto.Modelo,
                AnoFabricacao = dto.AnoFabricacao,
                Quilometragem = dto.Quilometragem,
                Cor = dto.Cor,
                Chassi = dto.Chassi,
                Status = dto.Status,
                FabricanteId = dto.FabricanteId
            };

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = veiculo.VeiculoId },
                new { veiculo.VeiculoId, veiculo.Placa });
        }

        // PUT api/veiculos/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] VeiculoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null)
                return NotFound(new { sucesso = false, mensagem = $"Veículo com id {id} não encontrado." });

            var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.FabricanteId == dto.FabricanteId);
            if (!fabricanteExiste)
                return BadRequest(new { sucesso = false, mensagem = $"Fabricante com id {dto.FabricanteId} não existe." });

            var placaEmUso = await _context.Veiculos
                .AnyAsync(v => v.Placa == dto.Placa && v.VeiculoId != id);
            if (placaEmUso)
                return Conflict(new { sucesso = false, mensagem = $"Já existe outro veículo com a placa '{dto.Placa}'." });

            veiculo.Placa = dto.Placa;
            veiculo.Modelo = dto.Modelo;
            veiculo.AnoFabricacao = dto.AnoFabricacao;
            veiculo.Quilometragem = dto.Quilometragem;
            veiculo.Cor = dto.Cor;
            veiculo.Chassi = dto.Chassi;
            veiculo.Status = dto.Status;
            veiculo.FabricanteId = dto.FabricanteId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/veiculos/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Alugueis)
                .FirstOrDefaultAsync(v => v.VeiculoId == id);

            if (veiculo == null)
                return NotFound(new { sucesso = false, mensagem = $"Veículo com id {id} não encontrado." });

            if (veiculo.Alugueis.Any())
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = $"Não é possível excluir: existem {veiculo.Alugueis.Count} aluguel(éis) vinculado(s) a este veículo."
                });

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
