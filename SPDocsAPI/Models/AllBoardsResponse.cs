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

    public class Board
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Workspace Workspace { get; set; }
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
}
