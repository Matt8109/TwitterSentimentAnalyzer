using System.Data.Entity;
using Oclumen.Core.DataContexts;
using Oclumen.Core.Entities;
using Oclumen.Crawler.Helpers;

namespace Oclumen.Crawler.DataContexts
{
    public class OclumenContext : DbContext, IOclumenContext
    {
        public OclumenContext()
            : base(AppConfigSettings.ContextName)
        {
        }

        public DbSet<BasicNgram> BasicNgrams { get; set; }
        public DbSet<HashtagNgram> Hashtags { get; set; }
        public DbSet<HashtagUseRecord> HashtagUseRecords { get; set; }
        public DbSet<RetweetUseRecord> RetweetUseRecords { get; set; }
        public DbSet<IgnoredNgram> IgnoredNgrams { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<StemmedNgram> StemmedNgrams { get; set; }
        public DbSet<Tweet> RawTweets { get; set; }
        public DbSet<TwitterAccount> TwitterAccounts { get; set; }
    }
}