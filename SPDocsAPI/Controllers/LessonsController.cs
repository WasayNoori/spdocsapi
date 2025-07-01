using Microsoft.AspNetCore.Mvc;
using SPDocsAPI.Interfaces;
using Microsoft.Extensions.Configuration;

namespace SPDocsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<LessonsController> _logger;
        private readonly IConfiguration _configuration;

        public LessonsController(IDocumentService documentService, ILogger<LessonsController> logger, IConfiguration configuration)
        {
            _documentService = documentService;
            _logger = logger;
            _configuration = configuration;
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

        [HttpGet("GetMultipleLessonIDs")]

        public async Task<ActionResult<IEnumerable<string>>> GetMultipleLessonIDs([FromQuery] string[] categories)
        {
            try
            {
                if (categories == null || categories.Length == 0)
                {
                    return BadRequest("Categories parameter is required and cannot be empty");
                }
                var lessonIds = new List<string>();
                foreach (var category in categories)
                {
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        var lessonId = await _documentService.GetLessonIdAsync(category);
                        lessonIds.Add(lessonId);
                    }
                }
                return Ok(lessonIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple lesson IDs for categories: {Categories}", string.Join(", ", categories));
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("GetBulkLessonIDs")]
        public async Task<ActionResult<IEnumerable<string>>> GetBulkLessonIDs([FromQuery] string category, int count = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest("Category parameter is required and cannot be empty");
                }

                if (count <= 0 || count > 1000)
                {
                    return BadRequest("Count must be between 1 and 1000");
                }

                var lessonIds = await _documentService.GetBulkLessonIdsAsync(category, count);
                
                return Ok(lessonIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bulk lesson IDs for category: {Category}, count: {Count}", category, count);
                return StatusCode(500, "Internal server error");
            }
        }

       
    }
} 