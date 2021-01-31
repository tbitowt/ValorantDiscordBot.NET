using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Models.Database;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Services
{
    public class ValorantDbService
    {
        public async Task<bool> LinkAccountToDiscordUser(ValorantAccount accountToAdd, DiscordUser user)
        {
            bool result = true;
            using (var db = new DatabaseDbContext())
            {
                var dbUser = db.DiscordUsers.Include(u => u.ValorantAccounts).SingleOrDefault(u => u.ID == user.Id);
                if (dbUser == null)
                {
                    dbUser = new DbDiscordUser()
                    {
                        ID = user.Id,
                        Name = user.Username
                    };
                    await db.DiscordUsers.AddAsync(dbUser);
                    await db.SaveChangesAsync();
                }

                var valorantAccount = dbUser.ValorantAccounts.FirstOrDefault(a => a.Subject == accountToAdd.Subject);
                if (valorantAccount == null)
                {
                    dbUser.ValorantAccounts.Add(accountToAdd);
                    db.Update(dbUser);
                }
                else
                {
                    accountToAdd.ID = valorantAccount.ID;
                    result = false;
                    db.Update(accountToAdd);
                }

                await db.SaveChangesAsync();
            }
            return result;
        }

        public async Task AsignAccountToGuild(ValorantAccount accountToAssign, ulong guildId)
        {
            using (var db = new DatabaseDbContext())
            {

                var guild = accountToAssign.RegisteredGuilds.FirstOrDefault(g => g.GuildID == guildId);
                if (guild == null)
                {
                    accountToAssign.RegisteredGuilds.Add(new RegisteredGuild()
                    {
                        GuildID = guildId,
                        ValorantAccount = accountToAssign
                    });
                    db.Update(accountToAssign);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task AddLastAccountMatch(ValorantAccount account, List<RankInfo> history)
        {
            using (var db = new DatabaseDbContext())
            {
                foreach (var rankInfo in history)
                {
                    if (account.RankInfos.Any(info => info.DateTime == rankInfo.DateTime) == false)
                    {
                        account.RankInfos.Add(rankInfo);
                    }
                }
                db.Update(account);
                await db.SaveChangesAsync();
            }
        }
    }
}