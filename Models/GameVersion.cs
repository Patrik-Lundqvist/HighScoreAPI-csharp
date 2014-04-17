namespace HighscoreAPI.Models
{
    public class GameVersion
    {
        public string Latest { get; set; }
        public string Required { get; set; }
        public bool UpdateExists { get; set; }
        public bool UpdateRequired { get; set; }
    }
}
