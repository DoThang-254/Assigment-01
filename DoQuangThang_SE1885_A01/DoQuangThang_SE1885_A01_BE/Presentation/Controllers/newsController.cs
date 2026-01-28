using BusinessLogic.Dto;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Models.Dto;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Presentation.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class newsController : ODataController
    {
        private readonly INewsArticleService _newsArticleService;
        public newsController(INewsArticleService newsArticleService) => _newsArticleService = newsArticleService;

        [EnableQuery]
        public IActionResult Get() {
            var rs = _newsArticleService.GetAllNews();
            return Ok(rs);
        }

        [EnableQuery]
        public IActionResult Get(string key)
        {
            try
            {
                var result = _newsArticleService.GetNewsById(key);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Lỗi xử lý",
                    detail = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }



        //[HttpGet("reports")]
        public IActionResult Reports(int? categoryId)
        {
            var response = _newsArticleService.ReportNewsByCategory(categoryId);
            return Ok(response);
        }

        //[HttpPost]
        public IActionResult Post([FromBody] NewsDto request)
        {
            try
            {
                _newsArticleService.AddNews(request);
                return Ok(request);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] NewsDto request)
        {
            try
            {
                _newsArticleService.UpdateNews(request);
                return Ok(request);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            try
            {
                _newsArticleService.DeleteNews(key);
                return Ok(new
                {
                    message = "News article deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [EnableQuery]
        [HttpGet("authors")]
        public IQueryable<AuthorDto> GetAuthors()
        {
            return _newsArticleService.GetAuthorDtos();
        }

    }
}
