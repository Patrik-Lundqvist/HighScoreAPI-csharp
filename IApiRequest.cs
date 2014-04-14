namespace HighscoreAPI
{
    public interface IApiRequest
    {
        string Key { get; }
        string Secret { get;  }
        string URL { get; }
        string GameVersion { get;  }
    }
}
