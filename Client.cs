using System;
using System.Collections.Generic;
using HighscoreAPI.Models;
using Newtonsoft.Json;

namespace HighscoreAPI
{
    public class Client : IApiRequest
    {
        public string Key { get; private set; }
        public string Secret { get; private set; }
        public string URL { get; private set; }
        public string GameVersion { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        /// <param name="url"></param>
        /// <param name="gameVersion"></param>
        public Client(string key, string secret, string url, string gameVersion)
        {
            GameVersion = gameVersion;
            URL = url;
            Secret = secret;
            Key = key;
        }

        /// <summary>
        /// Get the top scores
        /// </summary>
        /// <param name="records">Number of scores to return</param>
        /// <param name="callback"></param>
        public void GetHighscores(int records, Action<Response<List<Score>>> callback)
        {
            var communicator = new Communicator(this);
            var data = new Dictionary<string, object> { { "results", records } };
            communicator.Request<List<Score>>("GET", "/highscore/top", data, r =>
            {
                // If request is successful, deserialize the json data
                if (r.isSuccess)
                {
                    var scoreboard = JsonConvert.DeserializeObject<Scoreboard>(r.Data);
                    r.DataObject = scoreboard.Scores;
                }

                callback(r);
            });
        }

        /// <summary>
        /// Post a score to the API
        /// </summary>
        /// <param name="score"></param>
        /// <param name="callback"></param>
        public void PostScore(Score score, Action<Response<bool>> callback)
        {
            var communicator = new Communicator(this);
            var data = new Dictionary<string, object> { { "username", score.Name }, { "score", score.ScorePoints }, { "version", GameVersion } };
            communicator.Request("POST", "/highscore/new", data, callback);
        }

        /// <summary>
        /// Get game version information from the API
        /// </summary>
        /// <param name="callback"></param>
        public void GetVersion(Action<Response<GameVersion>> callback)
        {
            var communicator = new Communicator(this);
            var data = new Dictionary<string, object> { { "version", GameVersion } };
            communicator.Request<GameVersion>("GET", "/version", data, r =>
            {
                // If request is successful, deserialize the json data
                if (r.isSuccess)
                {
                    r.DataObject = JsonConvert.DeserializeObject<GameVersion>(r.Data);
                }

                callback(r);
            });
        }

    }
}
