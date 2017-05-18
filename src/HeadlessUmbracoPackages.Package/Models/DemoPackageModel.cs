using Umbraco.Core.Persistence;

namespace HeadlessUmbracoPackages.Package.Models
{
    [TableName("DemoPackage")]
    [PrimaryKey("id", autoIncrement = true)]
    public class DemoPackageModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("text")]
        public string Text { get; set; }
    }
}