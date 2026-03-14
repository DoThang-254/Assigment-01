using BusinessLogic.Dto;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Presentation.Controllers
{
    public class tagController : ODataController
    {
        private readonly ITagService _tagService;

        public tagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: odata/Tag
        [EnableQuery]
        public IActionResult Get()
        {
            try
            {
                return Ok(_tagService.GetAll());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: odata/Tag(1)
        [EnableQuery]
        public IActionResult Get([FromRoute] int key)
        {
            var tag = _tagService.GetById(key);
            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        // GET: odata/Tag(1)/NewsArticles
        // Exposes articles that use the specified tag via the navigation property name "NewsArticles".
        [EnableQuery]
        public IActionResult GetNewsArticles([FromRoute] int key)
        {
            try
            {
                var articles = _tagService.GetNewsArticlesByTagId(key);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: odata/Tag
        public IActionResult Post([FromBody] TagDto tag)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _tagService.AddTag(tag);

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // PUT: odata/Tag(1)
        public IActionResult Put([FromRoute] int key, [FromBody] TagUpdateRequestDto tag)
        {
            if (key != tag.TagId)
                return BadRequest("Tag ID mismatch");

            try
            {
                _tagService.UpdateTag(tag);
                return Updated(tag);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: odata/Tag(1)
        public IActionResult Delete([FromRoute] int key)
        {
            try
            {
                _tagService.DeleteTag(key);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
