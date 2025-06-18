using Microsoft.AspNetCore.Mvc;
using SPDocsAPI.DTOs;
using SPDocsAPI.Interfaces;

namespace SPDocsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active documents
        /// </summary>
        /// <returns>List of documents</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
        {
            try
            {
                var documents = await _documentService.GetAllDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific document by ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Document details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(int id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                
                if (document == null)
                {
                    return NotFound($"Document with ID {id} not found");
                }

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID: {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new document
        /// </summary>
        /// <param name="createDocumentDto">Document creation data</param>
        /// <returns>Created document</returns>
        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument(CreateDocumentDto createDocumentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var document = await _documentService.CreateDocumentAsync(createDocumentDto);
                
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing document
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="updateDocumentDto">Document update data</param>
        /// <returns>Updated document</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentDto>> UpdateDocument(int id, UpdateDocumentDto updateDocumentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var document = await _documentService.UpdateDocumentAsync(id, updateDocumentDto);
                
                if (document == null)
                {
                    return NotFound($"Document with ID {id} not found");
                }

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document with ID: {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a document (soft delete)
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(int id)
        {
            try
            {
                var result = await _documentService.DeleteDocumentAsync(id);
                
                if (!result)
                {
                    return NotFound($"Document with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID: {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get documents by type using stored procedure
        /// </summary>
        /// <param name="documentType">Document type to filter by</param>
        /// <returns>List of documents of the specified type</returns>
        [HttpGet("by-type/{documentType}")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocumentsByType(string documentType)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByTypeAsync(documentType);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents by type: {DocumentType}", documentType);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get documents by user using stored procedure
        /// </summary>
        /// <param name="userName">Username to filter by</param>
        /// <returns>List of documents created by the user</returns>
        [HttpGet("by-user/{userName}")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocumentsByUser(string userName)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByUserAsync(userName);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents by user: {UserName}", userName);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Activate or deactivate a document using stored procedure
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="isActive">Active status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateDocumentStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _documentService.ActivateDeactivateDocumentAsync(id, isActive);
                
                if (!result)
                {
                    return NotFound($"Document with ID {id} not found");
                }

                return Ok(new { message = $"Document {(isActive ? "activated" : "deactivated")} successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document status for ID: {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 