

using MediatR;

namespace DgLandCrawler.Abstraction.Behaviour
{
    // Cache aside pattern
    public class QueryCachingPipelineBehaviour<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse> where TRequest : ICachedQuery<TResponse>
    {
        private readonly ICacheService _cacheService;

        public QueryCachingPipelineBehaviour(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return await _cacheService.GetOrCreate(request.Key, _ => next(), request.Expiration);
        }
    }

    public interface ICacheService
    {
        Task<T> GetOrCreate<T>(
            string key,
            Func<CancellationToken,Task<T>> factory,
            TimeSpan? expiration, CancellationToken cancellationToken = default);
    }
}
