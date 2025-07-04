using System.Text.Json.Serialization;

namespace SPDocsAPI.Models
{
    public class AllBoardsResponse
    {
        public bool Success { get; set; }
        public int TotalBoards { get; set; }
        public List<Board> Boards { get; set; }
        public FilterInfo Filter { get; set; }
        public FetchDetails FetchDetails { get; set; }
    }

    public class UserResponse
    {
        public bool Success { get; set; }
        public int TotalUsers { get; set; }
        public List<User> Users { get; set; }
       
    }
    public class Board
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Workspace Workspace { get; set; }
        [JsonPropertyName("activity_logs")]
        public List<ActivityLog> ActivityLog{ get; set; }
    }

    public class Workspace
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class FilterInfo
    {
        public string Workspace { get; set; }
        public bool Applied { get; set; }
    }

    public class FetchDetails
    {
        public int TotalPages { get; set; }
        public int BoardsPerPage { get; set; }
        public DateTime FetchedAt { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string BusinessUnit { get; set; }
    }

    public class ActivityLog
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("user_id")]
        public string UserID { get; set; }
        [JsonPropertyName("event")]
        public string Actiontype { get; set; }
        [JsonPropertyName("created_at")]
        public string ActionDate { get; set; }

    }
    public class ActivityLogBoard
    {
        [JsonPropertyName("activity_logs")]
        public List<ActivityLog> ActivityLogs { get; set; }
    }

    public class ActivityLogData
    {
        [JsonPropertyName("boards")]
        public List<ActivityLogBoard> Boards { get; set; }
    }

    public class ActivityLogRoot
    {
        [JsonPropertyName("data")]
        public ActivityLogData Data { get; set; }
    }

}
