using HeadlessUmbracoPackages.Package.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace HeadlessUmbracoPackages.Package.Migrations._1._0._0
{
    [Migration("1.0.0", 0, "DemoPackage")]
    public class AddTablesMigration : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        public AddTablesMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, ApplicationContext.Current.ProfilingLogger.Logger, ApplicationContext.Current.DatabaseContext.SqlSyntax);
        }

        public override void Up()
        {
            //Check if the DB table does NOT exist
            if (!_schemaHelper.TableExist("DemoPackage"))
            {
                _schemaHelper.CreateTable<DemoPackageModel>(false);
            }
        }

        public override void Down()
        {
            _schemaHelper.DropTable<DemoPackageModel>();
        }
    }
}
