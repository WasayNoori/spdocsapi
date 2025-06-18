using Microsoft.AspNetCore.Mvc;
using SPDocsAPI.Interfaces;

namespace SPDocsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(IDocumentService documentService, ILogger<LessonsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Get the next lesson ID for a given category
        /// </summary>
        /// <param name="category">The category string to get the lesson ID for</param>
        /// <returns>The lesson ID as a string</returns>
        [HttpGet("GetLessonID")]
        public async Task<ActionResult<string>> GetLessonID([FromQuery] string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest("Category parameter is required and cannot be empty");
                }

                var lessonId = await _documentService.GetLessonIdAsync(category);
                
                return Ok(lessonId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson ID for category: {Category}", category);
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 