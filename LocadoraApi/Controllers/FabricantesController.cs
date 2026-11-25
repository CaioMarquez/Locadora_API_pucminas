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
    [Route("api/fabricantes")]
    public class FabricantesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FabricantesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET api/fabricantes
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var fabricantes = await _context.Fabricantes
                .Select(f => new FabricanteResponseDto
                {
                    FabricanteId = f.FabricanteId,
                    Nome = f.Nome,
                    PaisOrigem = f.PaisOrigem,
                    Ativo = f.Ativo,
                    QuantidadeVeiculos = f.Veiculos.Count
                })
                .ToListAsync();

            return Ok(fabricantes);
        }

        // GET api/fabricantes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var fabricante = await _context.Fabricantes
                .Where(f => f.FabricanteId == id)
                .Select(f => new FabricanteResponseDto
                {
                    FabricanteId = f.FabricanteId,
                    Nome = f.Nome,
                    PaisOrigem = f.PaisOrigem,
                    Ativo = f.Ativo,
                    QuantidadeVeiculos = f.Veiculos.Count
                })
                .FirstOrDefaultAsync();

            if (fabricante == null)
                return NotFound(new { sucesso = false, mensagem = $"Fabricante com id {id} não encontrado." });

            return Ok(fabricante);
        }

        // POST api/fabricantes
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FabricanteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var jaExiste = await _context.Fabricantes.AnyAsync(f => f.Nome == dto.Nome);
            if (jaExiste)
                return Conflict(new { sucesso = false, mensagem = $"Já existe um fabricante com o nome '{dto.Nome}'." });

            var fabricante = new Fabricante
            {
                Nome = dto.Nome,
                PaisOrigem = dto.PaisOrigem,
                Ativo = dto.Ativo
            };

            _context.Fabricantes.Add(fabricante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = fabricante.FabricanteId },
                new { fabricante.FabricanteId, fabricante.Nome });
        }

        // PUT api/fabricantes/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] FabricanteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fabricante = await _context.Fabricantes.FindAsync(id);
            if (fabricante == null)
                return NotFound(new { sucesso = false, mensagem = $"Fabricante com id {id} não encontrado." });

            var nomeEmUso = await _context.Fabricantes
                .AnyAsync(f => f.Nome == dto.Nome && f.FabricanteId != id);
            if (nomeEmUso)
                return Conflict(new { sucesso = false, mensagem = $"Já existe outro fabricante com o nome '{dto.Nome}'." });

            fabricante.Nome = dto.Nome;
            fabricante.PaisOrigem = dto.PaisOrigem;
            fabricante.Ativo = dto.Ativo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/fabricantes/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var fabricante = await _context.Fabricantes
                .Include(f => f.Veiculos)
                .FirstOrDefaultAsync(f => f.FabricanteId == id);

            if (fabricante == null)
                return NotFound(new { sucesso = false, mensagem = $"Fabricante com id {id} não encontrado." });

            if (fabricante.Veiculos.Any())
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = $"Não é possível excluir: existem {fabricante.Veiculos.Count} veículo(s) vinculado(s) a este fabricante."
                });

            _context.Fabricantes.Remove(fabricante);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
