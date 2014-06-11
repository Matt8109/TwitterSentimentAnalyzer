namespace Oclumen.Crawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedCountColumnFromHashtagAndNowUseBaseNgramObject : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HashtagNgrams",
                c => new
                    {
                        Text = c.String(nullable: false, maxLength: 140),
                        FirstSeen = c.DateTime(nullable: false, storeType: "datetime2"),
                        Cardinality = c.Int(nullable: false),
                        PositiveCount = c.Int(nullable: false),
                        NeutralCount = c.Int(nullable: false),
                        NegativeCount = c.Int(nullable: false),
                        RtPositiveCount = c.Int(nullable: false),
                        RtNeutralCount = c.Int(nullable: false),
                        RtNegativeCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Text);
            
            DropTable("dbo.Hashtags");
        }
        
        public override void Down()
        {
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
            
            DropTable("dbo.HashtagNgrams");
        }
    }
}
