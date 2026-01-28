using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Presentation.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class CountNewsByCategoryController : ODataController
    {
        private readonly ICategoryService _categoryService;
        public CountNewsByCategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        //[HttpGet("CountNewsByCategory")]
        [EnableQuery]
        public IActionResult Get()
        {
            try
            {
                var result = _categoryService.GetCategoryArticleCounts();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

            }
        }
    }
}
