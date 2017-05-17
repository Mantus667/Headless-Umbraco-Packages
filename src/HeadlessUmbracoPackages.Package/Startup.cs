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
            
        }
    }
}