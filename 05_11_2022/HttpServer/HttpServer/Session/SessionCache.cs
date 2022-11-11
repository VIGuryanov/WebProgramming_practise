﻿using Microsoft.Extensions.Caching.Memory;

namespace HttpServer.Session
{
    internal class SessionCache
    {
        readonly MemoryCache _cache = new(new MemoryCacheOptions());
        static readonly MyORM.MyORM orm = new(@"(localdb)\MSSQLLocalDB", "SteamDB", true);

        public Session? GetOrCreate(Session? item)
        {
            var mem = item;
            if (item != null && !_cache.TryGetValue(item.AccountId, out item))
            {
                item = mem;
                var db_value = orm.Select<Session>()
                                .Where(s => s.Email == item.Email
                                            && s.AccountId == item.AccountId
                                            && s.createDateTime == item.createDateTime)
                                .FirstOrDefault();

                if(db_value == null)
                    return null;

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                     .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(db_value.AccountId, db_value, cacheEntryOptions);
            }
            return item;
        }
    }
}