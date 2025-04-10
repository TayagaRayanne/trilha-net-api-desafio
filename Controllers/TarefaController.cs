using Microsoft.AspNetCore.Mvc;
using TrilhaApiDesafio.Context;
using TrilhaApiDesafio.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TrilhaApiDesafio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TarefaController : ControllerBase
    {
        private readonly OrganizadorContext _context;

        public TarefaController(OrganizadorContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult ObterPorId(int id)
        {
            var tarefa = _context.Tarefas.Find(id);

            if (tarefa == null)
            {
                return NotFound();
            }

            return Ok(tarefa);
        }

        [HttpGet("ObterTodos")]
        public async Task<IActionResult> ObterTodos()
        {
            var tarefas = await _context.Tarefas.ToListAsync();
            return Ok(tarefas);
        }

        [HttpGet("ObterPorTitulo")]
        public async Task<IActionResult> ObterPorTitulo(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                return BadRequest("O título não pode ser vazio.");
            }

            var tarefas = await _context.Tarefas.Where(t => t.Titulo.Contains(titulo)).ToListAsync();
            return Ok(tarefas);
        }

        [HttpGet("ObterPorData")]
        public IActionResult ObterPorData(DateTime data)
        {
            var tarefa = _context.Tarefas.Where(x => x.Data.Date == data.Date);
            return Ok(tarefa);
        }

        [HttpGet("ObterPorStatus")]
        public async Task<IActionResult> ObterPorStatus(EnumStatusTarefa status)
        {
            var tarefas = await _context.Tarefas.Where(t => t.Status == status).ToListAsync();
            return Ok(tarefas);
        }

        [HttpPost]
        public async Task<IActionResult> CriarTarefa(Tarefa tarefa)
        {
            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = tarefa.Id }, tarefa);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, Tarefa tarefa)
        {
            // Verifica se o ID da rota corresponde ao ID da tarefa no corpo
            if (id != tarefa.Id)
            {
                return BadRequest("O ID da rota não corresponde ao ID da tarefa no corpo da requisição.");
            }

            // Verifica se a tarefa com o ID existe no banco de dados
            var tarefaBanco = await _context.Tarefas.FindAsync(id);
            if (tarefaBanco == null)
            {
                return NotFound();
            }

            // Atualiza as propriedades da tarefa existente com os valores da tarefa recebida
            tarefaBanco.Titulo = tarefa.Titulo;
            tarefaBanco.Descricao = tarefa.Descricao;
            tarefaBanco.Data = tarefa.Data;
            tarefaBanco.Status = tarefa.Status;

            // Marca a entidade como modificada (embora o EF Core geralmente detecte isso automaticamente)
            _context.Entry(tarefaBanco).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // Retorna 204 No Content para indicar sucesso na atualização
            }
            catch (DbUpdateConcurrencyException)
            {
                // Trata erros de concorrência, se necessário
                if (!TarefaExiste(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool TarefaExiste(int id)
        {
            return _context.Tarefas.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var tarefaBanco = await _context.Tarefas.FindAsync(id);

            if (tarefaBanco == null)
            {
                return NotFound();
            }

            _context.Tarefas.Remove(tarefaBanco);
            await _context.SaveChangesAsync();

            return NoContent(); // Retorna 204 No Content para indicar sucesso na exclusão
        }
    }
}
