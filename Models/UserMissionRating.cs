namespace MarbleServer.Models
{
    public class UserMissionRating
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }

        public int MissionId { get; set; }

        public int Rating { get; set; }

        public Player Player { get; set; } = null!;
    }
}