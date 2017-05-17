using System;
using System.Web;
using Microsoft.Web.XmlTransform;
using Umbraco.Core.Logging;

namespace HeadlessUmbracoPackages.Package.Helper
{
    /// <summary>
    /// Transformation helper which helps doing xdt transformation
    /// </summary>
    internal class TransformationHelper
    {
        /// <summary>
        /// Transforms the specified file with the given transformation.
        /// </summary>
        /// <param name="fileToTransform">The file to transform.</param>
        /// <param name="transformationFile">The transformation file.</param>
        public static void Transform(string fileToTransform, string transformationFile)
        {
            //LogHelper.Info<TransformationHelper>($"Starting tranformation of {fileToTransform} with {transformationFile}");

            var sourceDocFileName = VirtualPathUtility.ToAbsolute(fileToTransform);
            var xdtFileName = VirtualPathUtility.ToAbsolute(transformationFile);

            try
            {
                // The translation at-hand
                using (var xmlDoc = new XmlTransformableDocument())
                {
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(HttpContext.Current.Server.MapPath(sourceDocFileName));

                    using (var xmlTrans = new XmlTransformation(HttpContext.Current.Server.MapPath(xdtFileName)))
                    {
                        if (xmlTrans.Apply(xmlDoc))
                        {
                            // If we made it here, sourceDoc now has transDoc's changes
                            // applied. So, we're going to save the final result off to
                            // destDoc.
                            xmlDoc.Save(HttpContext.Current.Server.MapPath(sourceDocFileName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<TransformationHelper>("There was an error during transformation", ex);
            }

            LogHelper.Info<TransformationHelper>("Transformation finished successfully");
        }
    }
}
