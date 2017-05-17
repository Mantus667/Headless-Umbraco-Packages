using System.Linq;
using HeadlessUmbracoPackages.Package.Helper;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace HeadlessUmbracoPackages.Package
{
    /// <summary>
    /// Installer used to install stuff that the package needs like DocumentTypes, Migrations and so on
    /// </summary>
    internal class Installer
    {
        /// <summary>
        /// Runs this installer and checks if an install is necessary.
        /// </summary>
        public void Run()
        {
            InstallSection();
            InstallSectionDashboard();
        }

        /// <summary>
        /// Installs the custom section created by the package.
        /// </summary>
        private void InstallSection()
        {
            LogHelper.Info<Installer>("Try to install section for DemoPackage");
            var services = ApplicationContext.Current.Services;

            var sectionService = services.SectionService;

            //Try & find a section with the alias of "nuget"
            var ecSection = sectionService.GetSections().SingleOrDefault(x => x.Alias == "demoPackage");

            //If we can't find the section - doesn't exist
            if (ecSection != null) return;

            //So let's create it the section
            sectionService.MakeNew("DemoPackage", "demoPackage", "icon-calendar-alt");
            LogHelper.Info<Installer>("Done creating the section.");

            //Add the section to the allowed sections for the superadmin user
            var userService = services.UserService;
            var admin = userService.GetUserById(0);
            if (!admin.AllowedSections.Contains("demoPackage"))
            {
                admin.AddAllowedSection("demoPackage");
                userService.Save(admin);
            }
            LogHelper.Info<Installer>("Added Admin to custom section.");
        }

        /// <summary>
        /// Installs the custom section dashboard.
        /// </summary>
        private void InstallSectionDashboard()
        {
            LogHelper.Info<Installer>("Try to install section dashboard for DemoPackage");

            TransformationHelper.Transform("~/config/dashboard.config", "~/App_Plugins/DemoPackage/Transformations/dashboard.install.xdt");

            LogHelper.Info<Installer>("Done installing section dashboard for DemoPackage");
        }
    }
}