using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Polly;
using RestSharp;

namespace DiscordBot
{
    public static class DBContextExtensions
    {
        public static void AddOrUpdate<T>(this DbContext context, T entity) where T : class
        {
            if (context.Entry(entity).State == EntityState.Detached)
            {
                var key = context.GetKey(entity);
                if (key == null)
                    context.Set<T>().Add(entity);
                else
                    context.Set<T>().Update(entity);
            }
            else
            {
                context.Set<T>().Update(entity);
            }
        }

        public static object? GetKey<T>(this DbContext context, T entity)
        {
            var keyName = context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();

            return entity.GetType().GetProperty(keyName).GetValue(entity, null);
        }

        public static async Task<IRestResponse> ExecuteTaskAsyncWithPolicy(this IRestClient client,
            IRestRequest request, CancellationToken cancellationToken, AsyncPolicy<IRestResponse> policy)
        {
            var policyResult =
                await policy.ExecuteAndCaptureAsync(ct => client.ExecuteTaskAsync(request, ct), cancellationToken);

            return policyResult.Outcome == OutcomeType.Successful
                ? policyResult.Result
                : new RestResponse
                {
                    Request = request,
                    ErrorException = policyResult.FinalException
                };
        }

        public static async Task<IRestResponse<T>> ExecuteTaskAsyncWithPolicy<T>(this IRestClient client,
            IRestRequest request, CancellationToken cancellationToken, AsyncPolicy<IRestResponse<T>> policy)
        {
            var policyResult =
                await policy.ExecuteAndCaptureAsync(ct => client.ExecuteTaskAsync<T>(request, ct), cancellationToken);

            return policyResult.Outcome == OutcomeType.Successful
                ? policyResult.Result
                : new RestResponse<T>
                {
                    Request = request,
                    ErrorException = policyResult.FinalException
                };
        }
    }
}