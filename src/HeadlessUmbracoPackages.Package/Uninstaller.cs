using HeadlessUmbracoPackages.Package.Helper;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace HeadlessUmbracoPackages.Package
{
    /// <summary>
    /// Uninstaller used to uninstall stuff from config files and other places
    /// </summary>
    internal class Uninstaller
    {
        /// <summary>
        /// Runs this installer and checks if an install is necessary.
        /// </summary>
        public void Run()
        {
            UninstallSection();
            UninstallSectionDashboard();
            UninstallVersion();
        }

        private void UninstallVersion()
        {
            LogHelper.Info<Uninstaller>("Try to uninstall version for DemoPackage");

            TransformationHelper.Transform("~/web.config", "~/App_Plugins/DemoPackage/Transformations/web.uninstall.txt");

            LogHelper.Info<Uninstaller>("Done uninstalling version for DemoPackage");
        }

        /// <summary>
        /// Uninstalls the custom section created by the package.
        /// </summary>
        private void UninstallSection()
        {
            LogHelper.Info<Installer>("Try to uninstall section for DemoPackage");
            var services = ApplicationContext.Current.Services;

            var sectionService = services.SectionService;
            var userService = services.UserService;

            userService.DeleteSectionFromAllUsers("demoPackage");
            var section = sectionService.GetByAlias("demoPackage");
            sectionService.DeleteSection(section);

            LogHelper.Info<Installer>("Uninstalled the section.");
        }

        /// <summary>
        /// Installs the custom section dashboard.
        /// </summary>
        private void UninstallSectionDashboard()
        {
            LogHelper.Info<Installer>("Try to install section dashboard for DemoPackage");

            TransformationHelper.Transform("~/config/dashboard.config", "~/App_Plugins/DemoPackage/Transformations/dashboard.uninstall.txt");

            LogHelper.Info<Installer>("Done installing section dashboard for DemoPackage");
        }
    }
}