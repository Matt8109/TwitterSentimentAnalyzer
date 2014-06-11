namespace Oclumen.Crawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTestSentiment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tweets", "TestSentiment", c => c.Int(nullable: false));
            AddColumn("dbo.Tweets", "TestSentimentTimestamp", c => c.DateTime(nullable: false, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tweets", "TestSentimentTimestamp");
            DropColumn("dbo.Tweets", "TestSentiment");
        }
    }
}
