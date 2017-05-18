using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;
using HeadlessUmbracoPackages.Package.Helper;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web;

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
        public void Run(SemVersion version)
        {
            if (!NeedInstallation(version)) return;
            InstallSection();
            InstallSectionDashboard();
            //InstallDocumentTypes();
            //InstallTemplates();
            RunMigrations(version);
            UpdateVersion();
        }

        /// <summary>
        /// Runs the migrations.
        /// </summary>
        /// <param name="targetVersion">The target version.</param>
        private void RunMigrations(SemVersion targetVersion)
        {
            var migrationsRunner = new MigrationRunner(
                ApplicationContext.Current.Services.MigrationEntryService,
                ApplicationContext.Current.ProfilingLogger.Logger,
                GetCurrentVersion(),
                targetVersion,
                "DemoPackage");

            try
            {
                migrationsRunner.Execute(UmbracoContext.Current.Application.DatabaseContext.Database);
            }
            catch (Exception e)
            {
                LogHelper.Error<Installer>("Error running DemoPackage migration", e);
            }
        }

        private void UpdateVersion()
        {
            LogHelper.Info<Installer>("Try to install version for DemoPackage");

            TransformationHelper.Transform("~/web.config", "~/App_Plugins/DemoPackage/Transformations/web.install.xdt");

            LogHelper.Info<Installer>("Done installing version for DemoPackage");
        }

        /// <summary>
        /// Tests if the package needs to be installed.
        /// </summary>
        /// <param name="newVersion">The new version that should be installed.</param>
        /// <returns>Bool value indicating if an installation has to be made</returns>
        private bool NeedInstallation(SemVersion newVersion)
        {
            var currentVersion = GetCurrentVersion();
            return currentVersion < newVersion;
        }

        /// <summary>
        /// Gets the current version from the web.config AppSettings.
        /// </summary>
        /// <returns>A SemVersion containing the current installed package version</returns>
        private SemVersion GetCurrentVersion()
        {
            return !WebConfigurationManager.AppSettings.AllKeys.Contains("DemoPackage") ? new SemVersion(0) : new SemVersion(new Version(WebConfigurationManager.AppSettings["DemoPackage"]));
        }

        /// <summary>
        /// Installs the templates.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void InstallTemplates()
        {
            LogHelper.Info<Installer>("Trying to install template for DemoPackage");
            try
            {
                var cts = ApplicationContext.Current.Services.ContentTypeService;
                var fs = ApplicationContext.Current.Services.FileService;
                var template = fs.GetTemplate("DemoDocumentType") ?? new Template("DemoDocumentType", "DemoDocumentType");
                template.Content = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/App_Plugins/DemoPackage/TmpViews/DemoDocumentType.txt"));
                fs.SaveTemplate(template);
                var ct = cts.GetContentType("DemoDocumentType");
                ct.AllowedTemplates = new List<ITemplate> { template };
                ct.SetDefaultTemplate(template);
                cts.Save(ct);

            }
            catch (Exception ex)
            {
                LogHelper.Error<Installer>("Failed to install demo templates", ex);
            }
            LogHelper.Info<Installer>("Done installing section for DemoPackage");
        }

        /// <summary>
        /// Installs the document types.
        /// </summary>
        private void InstallDocumentTypes()
        {
            LogHelper.Info<Installer>("Trying to install document types for DemoPackage");
            var ps = ApplicationContext.Current.Services.PackagingService;
            string documentTypes = @"<DocumentTypes>";
            var element = XElement.Parse(documentTypes);
            ps.ImportContentTypes(element);
            LogHelper.Info<Installer>("Done installing document types for DemoPackage");
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