namespace DgLandCrawler.Models
{
    public class AppConfig
    {
        public SerilogConfig Serilog { get; set; }
        public ConnectionStringsConfig ConnectionStrings { get; set; }
        public ChatGPTConfig ChatGPT { get; set; }
        public string DownloadPath { get; set; }
    }

    public class SerilogConfig
    {
        public MinimalLevelConfig MinimalLevel { get; set; }
    }

    public class MinimalLevelConfig
    {
        public string Default { get; set; }
        public Dictionary<string, string> Override { get; set; }
    }

    public class ConnectionStringsConfig
    {
        public string MSSqlServer { get; set; }
    }

    public class ChatGPTConfig
    {
        public string Key { get; set; }
    }

}
