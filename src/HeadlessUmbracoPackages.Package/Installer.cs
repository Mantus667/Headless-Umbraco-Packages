using System;
using System.Linq;
using HeadlessUmbracoPackages.Package.Helper;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web;
using umbraco.cms.businesslogic.packager;
using System.IO;
using Umbraco.Core.IO;
using HeadlessUmbracoPackages.Package.Properties;

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
			if (InstallPackage(version))
			{
				InstallSection();
				InstallSectionDashboard();
				RunMigrations(version);
			}
        }

		private bool InstallPackage(SemVersion version)
		{
			//need to save the package manifest to a temp folder since that is how this package installer logic works
			var tempFile = Path.Combine(IOHelper.MapPath("~/App_Data/TEMP/DemoPackage"), Guid.NewGuid().ToString(), "package.xml");
			var tempDir = Path.GetDirectoryName(tempFile);
			Directory.CreateDirectory(tempDir);

			try
			{
				File.WriteAllText(tempFile, Resources.package);
				var ins = new umbraco.cms.businesslogic.packager.Installer(0);

				ins.LoadConfig(tempDir);

				int packageId;
				bool sameVersion;
				if (IsPackageVersionAlreadyInstalled(ins.Name, ins.Version, out sameVersion, out packageId))
				{
					//if it's the same version, we don't need to install anything
					if (!sameVersion)
					{
						var pckId = ins.CreateManifest(tempDir, Guid.NewGuid().ToString(), "65194810-1f85-11dd-bd0b-0800200c9a66");
						ins.InstallBusinessLogic(pckId, tempDir);
						return true;
					}
					return false;
				}
				else
				{
					var pckId = ins.CreateManifest(tempDir, Guid.NewGuid().ToString(), "65194810-1f85-11dd-bd0b-0800200c9a66");
					ins.InstallBusinessLogic(pckId, tempDir);
					return true;
				}
			}
			finally
			{
				if (File.Exists(tempFile))
				{
					File.Delete(tempFile);
				}
				if (Directory.Exists(tempDir))
				{
					Directory.Delete(tempDir, true);
				}
			}
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
                new SemVersion(1),
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

            TransformationHelper.Transform("~/config/dashboard.config", "~/App_Plugins/DemoPackage/Transformations/dashboard.install.txt");

            LogHelper.Info<Installer>("Done installing section dashboard for DemoPackage");
        }

		/// <summary>
		/// Determines whether [is package version already installed] [the specified name].
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="version">The version.</param>
		/// <param name="sameVersion">if set to <c>true</c> [same version].</param>
		/// <param name="packageId">The package identifier.</param>
		/// <returns>
		///   <c>true</c> if [is package version already installed] [the specified name]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsPackageVersionAlreadyInstalled(string name, string version, out bool sameVersion, out int packageId)
		{
			var allInstalled = InstalledPackage.GetAllInstalledPackages();
			var found = allInstalled.Where(x => x.Data.Name == name).ToArray();
			sameVersion = false;

			if (found.Length > 0)
			{
				var foundVersion = found.FirstOrDefault(x =>
				{
					//match the exact version
					if (x.Data.Version == version)
					{
						return true;
					}
					//now try to compare the versions
					if (Version.TryParse(x.Data.Version, out Version installed) && Version.TryParse(version, out Version selected))
					{
						if (installed >= selected)
							return true;
					}
					return false;
				});

				sameVersion = foundVersion != null;

				//this package is already installed, find the highest package id for this package name that is installed
				packageId = found.Max(x => x.Data.Id);
				return true;
			}

			packageId = -1;
			return false;
		}
	}
}