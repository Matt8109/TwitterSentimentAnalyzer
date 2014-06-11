namespace Oclumen.Crawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BasicNgrams",
                c => new
                    {
                        Text = c.String(nullable: false, maxLength: 140),
                        Cardinality = c.Int(nullable: false),
                        PositiveCount = c.Int(nullable: false),
                        NeutralCount = c.Int(nullable: false),
                        NegativeCount = c.Int(nullable: false),
                        RtPositiveCount = c.Int(nullable: false),
                        RtNeutralCount = c.Int(nullable: false),
                        RtNegativeCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Text);
            
            CreateTable(
                "dbo.Hashtags",
                c => new
                    {
                        Tag = c.String(nullable: false, maxLength: 511),
                        Count = c.Long(nullable: false),
                        FirstSeen = c.DateTime(nullable: false, storeType: "datetime2"),
                        PositiveCount = c.Int(nullable: false),
                        NeutralCount = c.Int(nullable: false),
                        NegativeCount = c.Int(nullable: false),
                        RtPositiveCount = c.Int(nullable: false),
                        RtNeutralCount = c.Int(nullable: false),
                        RtNegativeCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Tag);
            
            CreateTable(
                "dbo.HashtagUseRecords",
                c => new
                    {
                        Tag = c.String(nullable: false, maxLength: 511),
                        Timestamp = c.DateTime(nullable: false, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Tag);
            
            CreateTable(
                "dbo.IgnoredNgrams",
                c => new
                    {
                        Text = c.String(nullable: false, maxLength: 140),
                        Cardinality = c.Int(nullable: false),
                        PositiveCount = c.Int(nullable: false),
                        NeutralCount = c.Int(nullable: false),
                        NegativeCount = c.Int(nullable: false),
                        RtPositiveCount = c.Int(nullable: false),
                        RtNeutralCount = c.Int(nullable: false),
                        RtNegativeCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Text);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        id = c.Long(nullable: false),
                        Latitute = c.String(maxLength: 15),
                        Longitude = c.String(maxLength: 15),
                        type = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Tweets", t => t.id)
                .Index(t => t.id);
            
            CreateTable(
                "dbo.Tweets",
                c => new
                    {
                        id = c.Long(nullable: false, identity: true),
                        contributors = c.String(),
                        created_at_dt = c.DateTimeOffset(nullable: false),
                        favorited = c.String(),
                        in_reply_to_screen_name = c.String(),
                        in_reply_to_status_id = c.String(),
                        in_reply_to_user_id = c.String(),
                        source = c.String(),
                        text = c.String(maxLength: 1024),
                        truncated = c.String(),
                        created_at = c.String(),
                        CorpusSentiment = c.Int(nullable: false),
                        CorpusSentimentTimestamp = c.DateTime(nullable: false, storeType: "datetime2"),
                        AutoSentiment = c.Int(nullable: false),
                        AutoSentimentTimestamp = c.DateTime(nullable: false, storeType: "datetime2"),
                        user_id = c.Long(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.TwitterAccounts", t => t.user_id)
                .Index(t => t.user_id);
            
            CreateTable(
                "dbo.TwitterAccounts",
                c => new
                    {
                        id = c.Long(nullable: false, identity: true),
                        name = c.String(),
                        screen_name = c.String(),
                        location = c.String(),
                        description = c.String(),
                        profile_image_url = c.String(),
                        url = c.String(),
                        lang = c.String(),
                        _protected = c.String(name: "protected"),
                        followers_count = c.String(),
                        profile_background_color = c.String(),
                        profile_text_color = c.String(),
                        profile_link_color = c.String(),
                        profile_sidebar_fill_color = c.String(),
                        profile_sidebar_border_color = c.String(),
                        friends_count = c.String(),
                        created_at_dt = c.DateTimeOffset(nullable: false),
                        favourites_count = c.String(),
                        utc_offset = c.String(),
                        time_zone = c.String(),
                        profile_background_image_url = c.String(),
                        profile_background_tile = c.String(),
                        statuses_count = c.String(),
                        notifications = c.String(),
                        following = c.String(),
                        verified = c.String(),
                        contributors_enabled = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.StemmedNgrams",
                c => new
                    {
                        Text = c.String(nullable: false, maxLength: 140),
                        Cardinality = c.Int(nullable: false),
                        PositiveCount = c.Int(nullable: false),
                        NeutralCount = c.Int(nullable: false),
                        NegativeCount = c.Int(nullable: false),
                        RtPositiveCount = c.Int(nullable: false),
                        RtNeutralCount = c.Int(nullable: false),
                        RtNegativeCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Text);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Tweets", new[] { "user_id" });
            DropIndex("dbo.Locations", new[] { "id" });
            DropForeignKey("dbo.Tweets", "user_id", "dbo.TwitterAccounts");
            DropForeignKey("dbo.Locations", "id", "dbo.Tweets");
            DropTable("dbo.StemmedNgrams");
            DropTable("dbo.TwitterAccounts");
            DropTable("dbo.Tweets");
            DropTable("dbo.Locations");
            DropTable("dbo.IgnoredNgrams");
            DropTable("dbo.HashtagUseRecords");
            DropTable("dbo.Hashtags");
            DropTable("dbo.BasicNgrams");
        }
    }
}
