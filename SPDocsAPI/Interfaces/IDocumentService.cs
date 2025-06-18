using SPDocsAPI.DTOs;
using SPDocsAPI.Models;

namespace SPDocsAPI.Interfaces
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto);
        Task<DocumentDto?> UpdateDocumentAsync(int id, UpdateDocumentDto updateDocumentDto);
        Task<bool> DeleteDocumentAsync(int id);
        
        // Stored procedure specific methods
        Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(string documentType);
        Task<IEnumerable<DocumentDto>> GetDocumentsByUserAsync(string userName);
        Task<bool> ActivateDeactivateDocumentAsync(int id, bool isActive);
        
        // Lesson-specific method
        Task<string> GetLessonIdAsync(string category);
    }
} 