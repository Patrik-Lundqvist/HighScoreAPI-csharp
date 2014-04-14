using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HighscoreAPI
{
    public class Score
    {
        [JsonProperty("username")]
        public string Name { get; set; }
        [JsonProperty("score")]
        public decimal ScorePoints { get; set; }
    }

    public class Scoreboard
    {
        public List<Score> Scores { get; set; }
    }
}
