using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Localization;

namespace Nop.Plugin.Feed.PriceGrabber
{
    public class PriceGrabberService : BasePlugin,  IMiscPlugin
    {

        #region Fields
       
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public PriceGrabberService(IWebHelper webHelper)
        {
            this._webHelper = webHelper;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/FeedPriceGrabber/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.ClickHere", "Click here to see generated feed");
            this.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency", "Currency");
            this.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency.Hint", "Select the currency that will be used to generate the feed.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize", "Product thumbnail image size");
            this.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize.Hint", "The size in pixels for product thumbnail images (longest size).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Generate", "Generate feed");
            this.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Success", "PriceGrabber feed has been successfully generated");
            
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.ClickHere");
            this.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency");
            this.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency.Hint");
            this.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize");
            this.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize.Hint");
            this.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Generate");
            this.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Success");
            
            base.Uninstall();
        }

        #endregion
    }
}
