using Microsoft.EntityFrameworkCore;

namespace DiscordBot
{
    public static class DBContextExtensions
    {
        public static void AddOrUpdate<T>(this DbContext context, T entity) where T : class
        {
            if (context.Entry(entity).State == EntityState.Detached)
                context.Set<T>().Add(entity);
            else
            {
                context.Set<T>().Update(entity);
            }
        }
    }
}