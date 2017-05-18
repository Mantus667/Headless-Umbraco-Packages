using Semver;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;

namespace HeadlessUmbracoPackages.Package
{
    /// <summary>
    /// This class is called by Umbraco when the website starts we handle installation here
    /// </summary>
    /// <seealso cref="Umbraco.Core.ApplicationEventHandler" />
    public class Startup : ApplicationEventHandler
    {
        /// <summary>
        /// Overridable method to execute when Bootup is completed, this allows you to perform any other bootup logic required for the application.
        /// Resolution is frozen so now they can be used to resolve instances.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            InstalledPackage.BeforeDelete += InstalledPackage_BeforeDelete;

            var installer = new Installer();
            installer.Run(new SemVersion(1));
        }

        private void InstalledPackage_BeforeDelete(InstalledPackage sender, System.EventArgs e)
        {
            //Check which package is being uninstalled
            if (sender.Data.Name != "DemoPackage")
            {
                return;
            }

            var uninstaller = new Uninstaller();
            uninstaller.Run();
        }
    }
}