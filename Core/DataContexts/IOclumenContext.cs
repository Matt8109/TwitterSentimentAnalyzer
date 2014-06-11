using System.Data.Entity;
using Oclumen.Core.Entities;

namespace Oclumen.Core.DataContexts
{
    public interface IOclumenContext
    {
        DbSet<BasicNgram> BasicNgrams { get; set; }
        DbSet<HashtagNgram> Hashtags { get; set; }
        DbSet<HashtagUseRecord> HashtagUseRecords { get; set; }
        DbSet<RetweetUseRecord> RetweetUseRecords { get; set; }
        DbSet<IgnoredNgram> IgnoredNgrams { get; set; }
        DbSet<Location> Locations { get; set; }
        DbSet<StemmedNgram> StemmedNgrams { get; set; }
        DbSet<Tweet> RawTweets { get; set; }
        DbSet<TwitterAccount> TwitterAccounts { get; set; }

        // base class types
        int SaveChanges();
    }
}