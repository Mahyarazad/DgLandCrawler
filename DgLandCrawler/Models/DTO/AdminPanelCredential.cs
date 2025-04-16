using MediatR;

namespace DgLandCrawler.Models.DTO
{
    public record struct AdminPanelCredential : IRequest
    {
        public string Useranme { get; set; }
        public string Password { get; set; }
    }
}
