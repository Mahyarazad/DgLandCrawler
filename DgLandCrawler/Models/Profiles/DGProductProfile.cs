

using AutoMapper;
using DgLandCrawler.Models.DTO;

namespace DgLandCrawler.Models.Profiles
{
    public class DGProductProfile : Profile
    {
        public DGProductProfile()
        {
            CreateMap<DGView, DGProductData>();
        }
    }
}
