using BusinessLogic.Dto;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Presentation.Controllers
{
    public class categoryController : ODataController
    {
        private readonly ICategoryService _categoryService;
        public categoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            try
            {
                var categories = _categoryService.GetCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [EnableQuery]
        public IActionResult Get([FromRoute] short key)
        {
            try
            {
                var category = _categoryService.GetCategoryById(key);

                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        public IActionResult Post([FromBody] CategoryCreateRequestDto category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            try
            {
                _categoryService.CreateCategory(category);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [EnableQuery]
        public IActionResult Put([FromRoute] short key, [FromBody] CategoryUpdateRequestDto category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (key != category.CategoryId)
                    return BadRequest("Category ID mismatch");

                _categoryService.UpdateCategory(category);
                return Ok(category);
            }
            catch
            {
                return BadRequest();
            }
        }

        public IActionResult Delete([FromRoute] short key)
        {
            try
            {
                _categoryService.DeleteCategory(key);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();

            }
        }
       
        [HttpPatch]
        public IActionResult Patch([FromRoute] short key)
        {
            try
            {
                _categoryService.ToggleCategory(key);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();
            }

        }

       


    }
}
