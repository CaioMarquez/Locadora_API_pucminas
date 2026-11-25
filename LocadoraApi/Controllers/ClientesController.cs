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
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientesController(ApplicationContext context)
        {
            _context = context;
        }

        private static ClienteResponseDto ParaDto(Cliente c) => new ClienteResponseDto
        {
            ClienteId = c.ClienteId,
            Nome = c.Nome,
            Cpf = c.Cpf,
            Email = c.Email,
            Telefone = c.Telefone,
            NumeroCnh = c.NumeroCnh,
            DataNascimento = c.DataNascimento,
            DataCadastro = c.DataCadastro
        };

        // GET api/clientes
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return Ok(clientes.Select(ParaDto));
        }

        // GET api/clientes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { sucesso = false, mensagem = $"Cliente com id {id} não encontrado." });

            return Ok(ParaDto(cliente));
        }

        // POST api/clientes
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cpfEmUso = await _context.Clientes.AnyAsync(c => c.Cpf == dto.Cpf);
            if (cpfEmUso)
                return Conflict(new { sucesso = false, mensagem = $"Já existe um cliente com o CPF '{dto.Cpf}'." });

            var emailEmUso = await _context.Clientes.AnyAsync(c => c.Email == dto.Email);
            if (emailEmUso)
                return Conflict(new { sucesso = false, mensagem = $"Já existe um cliente com o e-mail '{dto.Email}'." });

            var cliente = new Cliente
            {
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                Telefone = dto.Telefone,
                NumeroCnh = dto.NumeroCnh,
                DataNascimento = dto.DataNascimento
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cliente.ClienteId },
                new { cliente.ClienteId, cliente.Nome });
        }

        // PUT api/clientes/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] ClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { sucesso = false, mensagem = $"Cliente com id {id} não encontrado." });

            var cpfEmUso = await _context.Clientes.AnyAsync(c => c.Cpf == dto.Cpf && c.ClienteId != id);
            if (cpfEmUso)
                return Conflict(new { sucesso = false, mensagem = $"Já existe outro cliente com o CPF '{dto.Cpf}'." });

            var emailEmUso = await _context.Clientes.AnyAsync(c => c.Email == dto.Email && c.ClienteId != id);
            if (emailEmUso)
                return Conflict(new { sucesso = false, mensagem = $"Já existe outro cliente com o e-mail '{dto.Email}'." });

            cliente.Nome = dto.Nome;
            cliente.Cpf = dto.Cpf;
            cliente.Email = dto.Email;
            cliente.Telefone = dto.Telefone;
            cliente.NumeroCnh = dto.NumeroCnh;
            cliente.DataNascimento = dto.DataNascimento;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/clientes/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Alugueis)
                .FirstOrDefaultAsync(c => c.ClienteId == id);

            if (cliente == null)
                return NotFound(new { sucesso = false, mensagem = $"Cliente com id {id} não encontrado." });

            if (cliente.Alugueis.Any())
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = $"Não é possível excluir: existem {cliente.Alugueis.Count} aluguel(éis) vinculado(s) a este cliente."
                });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
