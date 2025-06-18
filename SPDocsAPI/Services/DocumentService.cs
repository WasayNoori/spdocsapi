using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SPDocsAPI.Data;
using SPDocsAPI.DTOs;
using SPDocsAPI.Interfaces;
using SPDocsAPI.Models;
using System.Data;

namespace SPDocsAPI.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(ApplicationDbContext context, ILogger<DocumentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
        {
            try
            {
                var documents = await _context.Set<Document>()
                    .Where(d => d.IsActive)
                    .OrderByDescending(d => d.CreatedDate)
                    .ToListAsync();

                return documents.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all documents");
                throw;
            }
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            try
            {
                var document = await _context.Set<Document>()
                    .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

                return document != null ? MapToDto(document) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID: {DocumentId}", id);
                throw;
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto)
        {
            try
            {
                var document = new Document
                {
                    Title = createDocumentDto.Title,
                    Description = createDocumentDto.Description,
                    DocumentType = createDocumentDto.DocumentType,
                    CreatedBy = createDocumentDto.CreatedBy,
                    FilePath = createDocumentDto.FilePath,
                    FileSize = createDocumentDto.FileSize,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Set<Document>().Add(document);
                await _context.SaveChangesAsync();

                return MapToDto(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document");
                throw;
            }
        }

        public async Task<DocumentDto?> UpdateDocumentAsync(int id, UpdateDocumentDto updateDocumentDto)
        {
            try
            {
                var document = await _context.Set<Document>()
                    .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

                if (document == null)
                    return null;

                document.Title = updateDocumentDto.Title;
                document.Description = updateDocumentDto.Description;
                document.DocumentType = updateDocumentDto.DocumentType;
                document.ModifiedBy = updateDocumentDto.ModifiedBy;
                document.FilePath = updateDocumentDto.FilePath;
                document.FileSize = updateDocumentDto.FileSize;
                document.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return MapToDto(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document with ID: {DocumentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            try
            {
                var document = await _context.Set<Document>()
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                    return false;

                document.IsActive = false;
                document.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID: {DocumentId}", id);
                throw;
            }
        }

        // Stored procedure methods
        public async Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(string documentType)
        {
            try
            {
                var parameter = new SqlParameter("@DocumentType", documentType);
                
                var documents = await _context.Set<Document>()
                    .FromSqlRaw("EXEC GetDocumentsByType @DocumentType", parameter)
                    .ToListAsync();

                return documents.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents by type: {DocumentType}", documentType);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByUserAsync(string userName)
        {
            try
            {
                var parameter = new SqlParameter("@UserName", userName);
                
                var documents = await _context.Set<Document>()
                    .FromSqlRaw("EXEC GetDocumentsByUser @UserName", parameter)
                    .ToListAsync();

                return documents.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents by user: {UserName}", userName);
                throw;
            }
        }

        public async Task<bool> ActivateDeactivateDocumentAsync(int id, bool isActive)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@DocumentId", id),
                    new SqlParameter("@IsActive", isActive)
                };

                var result = await _context.Database
                    .ExecuteSqlRawAsync("EXEC ActivateDeactivateDocument @DocumentId, @IsActive", parameters);

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating/deactivating document with ID: {DocumentId}", id);
                throw;
            }
        }

        public async Task<string> GetLessonIdAsync(string category)
        {

            //stored procedures cant return a string so this is why we are using
            //an output parameter
            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "GetNextCategoryCode";
                command.CommandType = CommandType.StoredProcedure;

                var categoryParam = new SqlParameter("@Category", SqlDbType.NVarChar, 100)
                {
                    Value = category
                };

                var resultParam = new SqlParameter("@Result", SqlDbType.NVarChar, 50)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(categoryParam);
                command.Parameters.Add(resultParam);

                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                return resultParam.Value?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson ID for category: {Category}", category);
                throw;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }


        private static DocumentDto MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                DocumentType = document.DocumentType,
                CreatedDate = document.CreatedDate,
                ModifiedDate = document.ModifiedDate,
                CreatedBy = document.CreatedBy,
                ModifiedBy = document.ModifiedBy,
                IsActive = document.IsActive,
                FilePath = document.FilePath,
                FileSize = document.FileSize
            };
        }
    }
} 