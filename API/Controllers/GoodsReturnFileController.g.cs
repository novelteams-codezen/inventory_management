using Microsoft.AspNetCore.Mvc;
using inventory_management.Models;
using inventory_management.Data;
using inventory_management.Filter;
using inventory_management.Entities;
using inventory_management.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;

namespace inventory_management.Controllers
{
    /// <summary>
    /// Controller responsible for managing goodsreturnfile-related operations in the API.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for adding, retrieving, updating, and deleting goodsreturnfile information.
    /// </remarks>
    [Route("api/goodsreturnfile")]
    [Authorize]
    public class GoodsReturnFileController : ControllerBase
    {
        private readonly inventory_managementContext _context;

        public GoodsReturnFileController(inventory_managementContext context)
        {
            _context = context;
        }

        /// <summary>Adds a new goodsreturnfile to the database</summary>
        /// <param name="model">The goodsreturnfile data to be added</param>
        /// <returns>The result of the operation</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("GoodsReturnFile",Entitlements.Create)]
        public IActionResult Post([FromBody] GoodsReturnFile model)
        {
            _context.GoodsReturnFile.Add(model);
            this._context.SaveChanges();
            return Ok(new { model.Id });
        }

        /// <summary>Retrieves a list of goodsreturnfiles based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of goodsreturnfiles</returns>
        [HttpGet]
        [UserAuthorize("GoodsReturnFile",Entitlements.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public IActionResult Get([FromQuery] string filters, string searchTerm, int pageNumber = 1, int pageSize = 10, string sortField = null, string sortOrder = "asc")
        {
            List<FilterCriteria> filterCriteria = null;
            if (pageSize < 1)
            {
                return BadRequest("Page size invalid.");
            }

            if (pageNumber < 1)
            {
                return BadRequest("Page mumber invalid.");
            }

            if (!string.IsNullOrEmpty(filters))
            {
                filterCriteria = JsonHelper.Deserialize<List<FilterCriteria>>(filters);
            }

            var query = _context.GoodsReturnFile.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<GoodsReturnFile>.ApplyFilter(query, filterCriteria, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(GoodsReturnFile), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<GoodsReturnFile, object>>(Expression.Convert(property, typeof(object)), parameter);
                if (sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderBy(lambda);
                }
                else if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderByDescending(lambda);
                }
                else
                {
                    return BadRequest("Invalid sort order. Use 'asc' or 'desc'.");
                }
            }

            var paginatedResult = result.Skip(skip).Take(pageSize).ToList();
            return Ok(paginatedResult);
        }

        /// <summary>Retrieves a specific goodsreturnfile by its primary key</summary>
        /// <param name="id">The primary key of the goodsreturnfile</param>
        /// <returns>The goodsreturnfile data</returns>
        [HttpGet]
        [Route("{id:Guid}")]
        [UserAuthorize("GoodsReturnFile",Entitlements.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var entityData = _context.GoodsReturnFile.IncludeRelated().FirstOrDefault(entity => entity.Id == id);
            return Ok(entityData);
        }

        /// <summary>Deletes a specific goodsreturnfile by its primary key</summary>
        /// <param name="id">The primary key of the goodsreturnfile</param>
        /// <returns>The result of the operation</returns>
        [HttpDelete]
        [UserAuthorize("GoodsReturnFile",Entitlements.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        [Route("{id:Guid}")]
        public IActionResult DeleteById([FromRoute] Guid id)
        {
            var entityData = _context.GoodsReturnFile.IncludeRelated().FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                return NotFound();
            }

            _context.GoodsReturnFile.Remove(entityData);
            var status = this._context.SaveChanges();
            return Ok(new { status });
        }

        /// <summary>Updates a specific goodsreturnfile by its primary key</summary>
        /// <param name="id">The primary key of the goodsreturnfile</param>
        /// <param name="updatedEntity">The goodsreturnfile data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPut]
        [UserAuthorize("GoodsReturnFile",Entitlements.Update)]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public IActionResult UpdateById(Guid id, [FromBody] GoodsReturnFile updatedEntity)
        {
            if (id != updatedEntity.Id)
            {
                return BadRequest("Mismatched Id");
            }

            this._context.GoodsReturnFile.Update(updatedEntity);
            var status = this._context.SaveChanges();
            return Ok(new { status });
        }

        /// <summary>Updates a specific goodsreturnfile by its primary key</summary>
        /// <param name="id">The primary key of the goodsreturnfile</param>
        /// <param name="updatedEntity">The goodsreturnfile data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPatch]
        [UserAuthorize("GoodsReturnFile",Entitlements.Update)]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public IActionResult UpdateById(Guid id, [FromBody] JsonPatchDocument<GoodsReturnFile> updatedEntity)
        {
            if (updatedEntity == null)
                return BadRequest("Patch document is missing.");
            var existingEntity = this._context.GoodsReturnFile.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
                return NotFound();
            updatedEntity.ApplyTo(existingEntity, ModelState);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            this._context.GoodsReturnFile.Update(existingEntity);
            var status = this._context.SaveChanges();
            return Ok(new { status });
        }
    }
}