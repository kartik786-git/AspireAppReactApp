namespace AspireAppReactApp.Web
{
    public class TodoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsComplete { get; set; } = false;
        public int Position { get; set; } = 0;
    }
}
