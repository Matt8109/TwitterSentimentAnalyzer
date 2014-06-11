namespace Oclumen.Crawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMoreAutoSentimentInformation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tweets", "AutoUnigram", c => c.Int(nullable: false));
            AddColumn("dbo.Tweets", "AutoUnigramStemmed", c => c.Int(nullable: false));
            AddColumn("dbo.Tweets", "AutoBigram", c => c.Int(nullable: false));
            AddColumn("dbo.Tweets", "AutoBigramStemmed", c => c.Int(nullable: false));
            DropColumn("dbo.Tweets", "AutoSentiment");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tweets", "AutoSentiment", c => c.Int(nullable: false));
            DropColumn("dbo.Tweets", "AutoBigramStemmed");
            DropColumn("dbo.Tweets", "AutoBigram");
            DropColumn("dbo.Tweets", "AutoUnigramStemmed");
            DropColumn("dbo.Tweets", "AutoUnigram");
        }
    }
}
