﻿using Almacen.DTOs;
using Almacen.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Almacen.Controllers
{
    [Route("api/area")]
    [ApiController]
    public class AreasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AreasController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("obtener-por-id/{id:int}")]
        public async Task<ActionResult<AreaDTO>> GetById(int id)
        {
            var area = await context.Areas
                .FirstOrDefaultAsync(u => u.Id == id);

            if (area == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<AreaDTO>(area));
        }

        [HttpGet("obtener-todos")]
        public async Task<ActionResult<List<AreaDTO>>> GetAll()
        {
            var area = await context.Areas
                .OrderBy(u => u.Id)
                .ToListAsync();

            if (!area.Any())
            {
                return NotFound();
            }

            return Ok(mapper.Map<List<AreaDTO>>(area));
        }


        [HttpPost("crear")]
        public async Task<ActionResult> Post(AreaDTO dto)
        {
            // Validación del modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificación de la existencia del usuario
            var exiteArea = await context.Areas.AnyAsync(u => u.Nombre == dto.Nombre);

            if (exiteArea)
            {
                // return Conflict(new { error = "El usuario ya existe." });
                return Conflict();
            }

            // Mapeo del DTO a la entidad
            var area = mapper.Map<Area>(dto);

            // Incluir la entidad en el contexto
            context.Add(area);

            try
            {
                // Guardar cambios en la base de datos dentro de una transacción
                await context.SaveChangesAsync();
                return Ok();
                // return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                // Manejar errores de base de datos
                // return StatusCode(500, new { error = "Error interno del servidor.", details = ex.Message });
                return StatusCode(500);
            }
        }

        [HttpDelete("eliminar/{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var area = await context.Areas.FindAsync(id);

            if (area == null)
            {
                return NotFound();
            }

            context.Areas.Remove(area);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("actualizar/{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] AreaDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("El ID de la ruta y el ID del objeto no coinciden");
            }

            var area = await context.Areas.FindAsync(id);

            if (area == null)
            {
                return NotFound();
            }

            // Mapea los datos del DTO al usuario existente
            mapper.Map(dto, area);

            context.Update(area);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AreaExiste(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool AreaExiste(int id)
        {
            return context.Areas.Any(e => e.Id == id);
        }
    }
}
