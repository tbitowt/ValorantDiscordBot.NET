using System.Linq;
using Microsoft.EntityFrameworkCore;

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
                {
                    context.Set<T>().Add(entity);
                }
                else
                {
                    context.Set<T>().Update(entity);
                }
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
    }
}