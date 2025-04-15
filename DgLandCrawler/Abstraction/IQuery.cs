
using MediatR;

namespace DgLandCrawler.Abstraction
{
    public interface IQuery<TReponse> : IRequest<TReponse>
    {

    }

    public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery;


    public interface ICachedQuery
    {
        string Key { get; }
        TimeSpan? Expiration { get; }
    }
}
