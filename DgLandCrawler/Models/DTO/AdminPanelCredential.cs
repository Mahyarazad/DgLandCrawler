
namespace DgLandCrawler.Models.DTO
{
    public record struct AdminPanelCredential
    {
        public AdminPanelCredential(string useranme, string password)
        {
            Useranme = useranme;
            Password = password;
        }

        public string Useranme { get; set; }
        public string Password { get; set; }
    }
}
