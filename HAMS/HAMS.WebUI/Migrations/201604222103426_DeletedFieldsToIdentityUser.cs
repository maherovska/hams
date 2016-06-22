namespace HAMS.WebUI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeletedFieldsToIdentityUser : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "FirstName");
            DropColumn("dbo.AspNetUsers", "LastName");
            DropColumn("dbo.AspNetUsers", "Specialization");
            DropColumn("dbo.AspNetUsers", "Department");
            DropColumn("dbo.AspNetUsers", "EnterYear");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "EnterYear", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "Department", c => c.String());
            AddColumn("dbo.AspNetUsers", "Specialization", c => c.String());
            AddColumn("dbo.AspNetUsers", "LastName", c => c.String());
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String());
        }
    }
}
