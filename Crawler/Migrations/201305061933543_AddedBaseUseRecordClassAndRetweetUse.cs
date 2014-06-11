namespace Oclumen.Crawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBaseUseRecordClassAndRetweetUse : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RetweetUseRecords",
                c => new
                    {
                        Tag = c.String(nullable: false, maxLength: 1024),
                        Timestamp = c.DateTime(nullable: false, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Tag);
            
            AlterColumn("dbo.HashtagUseRecords", "Tag", c => c.String(nullable: false, maxLength: 1024));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.HashtagUseRecords", "Tag", c => c.String(nullable: false, maxLength: 511));
            DropTable("dbo.RetweetUseRecords");
        }
    }
}
