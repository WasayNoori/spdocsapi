using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SPDocsAPI.Data;
using SPDocsAPI.DTOs;
using SPDocsAPI.Interfaces;
using SPDocsAPI.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

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

        public async Task<IEnumerable<string>> GetBulkLessonIdsAsync(string category, int count)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Category", category),
                    new SqlParameter("@Count", count)
                };

                // Use raw SQL to get the lesson IDs from the stored procedure
                var lessonIds = new List<string>();
                
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC GetBulkCategoryCodes @Category, @Count";
                command.Parameters.AddRange(parameters);

                await _context.Database.OpenConnectionAsync();
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lessonIds.Add(reader.GetString("LessonID"));
                }

                return lessonIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bulk lesson IDs for category: {Category}, count: {Count}", category, count);
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

        public async Task<IActionResult> SyncBoardsAsync(List<Board> boards)
        {
            var table = CreateBoardDataTable(boards);

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SyncBoards";
            command.CommandType = CommandType.StoredProcedure;
            var param = new SqlParameter
            {
                ParameterName = "@Boards",
                SqlDbType = SqlDbType.Structured,
                TypeName = "BoardListType",
                Value = table
            };


            command.Parameters.Add(param);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
          
            return new OkObjectResult(new { Success = true, Message = "Boards synchronized successfully." });
        }
        private DataTable CreateBoardDataTable(List<Board> boards)
        {
            var table = new DataTable();
            table.Columns.Add("boardID", typeof(long));
            table.Columns.Add("boardName", typeof(string));

            foreach (var board in boards)
            {
                table.Rows.Add(long.Parse(board.Id), board.Name);
            }

            return table;
        }
        private DataTable CreateUsersDataTable(List<User> users)
        {
            var table = new DataTable();
            table.Columns.Add("MondayUserID", typeof(string));
            table.Columns.Add("FullName", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("Role", typeof(string));
            table.Columns.Add("BusinessUnit", typeof(string));
            foreach (var user in users)
            {
                var email = user.Email?.ToLower() ?? string.Empty;
                if (email.EndsWith("solidprofessor.com"))
                {
                    user.BusinessUnit = "Solid Professor";
                }
                else
                {
                    user.BusinessUnit = "HRS";
                }
                table.Rows.Add(user.ID, user.Name, user.Email, user.Role, user.BusinessUnit);
            }
            return table;
        }
        private DataTable CreateActivityTable(List<ActivityLog> activityLogs,long boardId)
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(string));
            table.Columns.Add("boardID", typeof(long));
           
            table.Columns.Add("MondayUserID", typeof(string));
            table.Columns.Add("activityDate", typeof(string));
            table.Columns.Add("activityType", typeof(string));

            foreach (var log in activityLogs)
            {
                try
                {
                    if (!IsValidLogEntry(log))
                    {
                        continue; // Skip invalid log entries
                    }
                    long time = long.Parse(log.ActionDate);
                    var dt = DateTimeOffset.FromUnixTimeMilliseconds(time / 10000).UtcDateTime.Date;
                    table.Rows.Add(log.Id, boardId, log.UserID, dt.ToString("MM-dd-yyyy"), log.Actiontype);
                }

                catch (Exception)
                {


                }
            }
            try
            {
                var top5 = table.AsEnumerable()
                    .OrderByDescending(row => DateTime.Parse(row.Field<string>("activityDate")))
                     .Take(5)
                     .CopyToDataTable();
                return top5;
            }
            catch (Exception ex)
            {

                throw;
            }

         
        }

        private bool IsValidLogEntry(ActivityLog log)
        {
            // Check if UserID is a valid, non-empty, numeric string and greater than 0
            if (string.IsNullOrWhiteSpace(log.UserID))
                return false;

            if (!long.TryParse(log.UserID, out var userIdValue))
                return false;

            if (userIdValue <= 0)
                return false;

            List<string> validActionTypes = new List<string>
            {
                "update_column_value",
                "create_pulse",
                "create_group",
                "update_name"
            };

            if (!validActionTypes.Contains(log.Actiontype))
                return false;

            return true;
        }
        public async Task<IActionResult> SyncUsers(List<User>users)
        {
            var table = CreateUsersDataTable(users);

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "AddUsersIfNotExists";
            command.CommandType = CommandType.StoredProcedure;
            var param = new SqlParameter
            {
                ParameterName = "@Users",
                SqlDbType = SqlDbType.Structured,
                TypeName = "UserType",
                Value = table
            };


            command.Parameters.Add(param);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

                throw;
            }

            return new OkObjectResult(new { Success = true, Message = "Boards synchronized successfully." });
        }

        public async Task<IActionResult> SyncActivityLogs(List<ActivityLog> activityLogs, long boardId)
        {
            var table = CreateActivityTable(activityLogs, boardId);

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "InsertBoardActivities";
            command.CommandType = CommandType.StoredProcedure;
            var param = new SqlParameter
            {
                ParameterName = "@Activities",
                SqlDbType = SqlDbType.Structured,
                TypeName = "activity",
                Value = table
            };

            var paramboardId = new SqlParameter
            {
                ParameterName = "@BoardID",
                SqlDbType = SqlDbType.BigInt,
                Value = boardId
            };

            command.Parameters.Add(param);
            command.Parameters.Add(paramboardId);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

                throw;
            }

            return new OkObjectResult(new { Success = true, Message = "Boards synchronized successfully." });
        }
    }
} 