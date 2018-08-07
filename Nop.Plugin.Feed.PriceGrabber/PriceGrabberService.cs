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
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public PriceGrabberService(IWebHelper webHelper,
            ILocalizationService localizationService)
        {
            this._webHelper = webHelper;
            this._localizationService = localizationService;
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
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.ClickHere", "Click here to see generated feed");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency", "Currency");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency.Hint", "Select the currency that will be used to generate the feed.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize", "Product thumbnail image size");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize.Hint", "The size in pixels for product thumbnail images (longest size).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Generate", "Generate feed");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Feed.PriceGrabber.Success", "PriceGrabber feed has been successfully generated");
            
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.ClickHere");
            _localizationService.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency");
            _localizationService.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.Currency.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize");
            _localizationService.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Fields.ProductPictureSize.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Generate");
            _localizationService.DeletePluginLocaleResource("Plugins.Feed.PriceGrabber.Success");
            
            base.Uninstall();
        }

        #endregion
    }
}
