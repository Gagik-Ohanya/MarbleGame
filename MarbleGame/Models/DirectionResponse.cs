namespace MarbleGame.Models
{
    public class DirectionResponse
    {
        public bool Success { get; set; }
        public bool Finished { get; set; }
        public Board BoardState { get; set; }
    }
}