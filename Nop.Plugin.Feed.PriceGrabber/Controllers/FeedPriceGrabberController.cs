using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Feed.PriceGrabber.Models;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Feed.PriceGrabber.Controllers
{
    [AdminAuthorize]
    public class FeedPriceGrabberController : BasePluginController
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public FeedPriceGrabberController(CurrencySettings currencySettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            ILogger logger,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IProductService productService,
            IStoreService storeService,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            this._currencySettings = currencySettings;
            this._categoryService = categoryService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._storeService = storeService;
            this._webHelper = webHelper;
            this._workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Replace some special characters
        /// </summary>
        /// <param name="inputString">Input string</param>
        /// <returns>Output string</returns>
        protected string ReplaceSpecChars(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return inputString;

            return inputString.Replace(';', ',').Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new FeedPriceGrabberModel
            {
                ProductPictureSize = 125,
                CurrencyId = _currencySettings.PrimaryStoreCurrencyId,
                AvailableCurrencies = _currencyService.GetAllCurrencies().ToSelectList(x => (x as Currency).Name)
            };
            
            return View("~/Plugins/Feed.PriceGrabber/Views/FeedPriceGrabber/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [ChildActionOnly]
        [FormValueRequired("generate")]
        public ActionResult GenerateFeed(FeedPriceGrabberModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var storeUrl = _webHelper.GetStoreLocation();

            try
            {
                var fileName = string.Format("priceGrabber_{0}_{1}.csv", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                using (var writer = new StreamWriter(Path.Combine(Request.PhysicalApplicationPath, "content\\files\\exportimport", fileName)))
                {
                    //write header
                    writer.WriteLine("Unique Retailer SKU;Manufacturer Name;Manufacturer Part Number;Product Title;Categorization;" +
                        "Product URL;Image URL;Detailed Description;Selling Price;Condition;Availability");

                    foreach (var parentProduct in _productService.SearchProducts(storeId: storeScope, visibleIndividuallyOnly: true))
                    {
                        //get single products from all of these
                        var singleProducts = new List<Product>();
                        switch (parentProduct.ProductType)
                        {
                            case ProductType.SimpleProduct:
                                {
                                    //simple product doesn't have child products
                                    singleProducts.Add(parentProduct);
                                }
                                break;
                            case ProductType.GroupedProduct:
                                {
                                    //grouped products could have several child products
                                    singleProducts.AddRange(_productService.GetAssociatedProducts(parentProduct.Id, storeScope));
                                }
                                break;
                            default:
                                continue;
                        }

                        //get line for the each product
                        foreach (var product in singleProducts)
                        {
                            //sku
                            var sku = !string.IsNullOrEmpty(product.Sku) ? product.Sku : product.Id.ToString();
                            sku = ReplaceSpecChars(sku);

                            //manufacturer name
                            var productManufacturer = _manufacturerService.GetProductManufacturersByProductId(product.Id).FirstOrDefault();
                            var manufacturerName = productManufacturer != null && productManufacturer.Manufacturer != null 
                                ? productManufacturer.Manufacturer.Name : string.Empty;
                            manufacturerName = ReplaceSpecChars(manufacturerName);

                            //manufacturer part number
                            var manufacturerPartNumber = ReplaceSpecChars(product.ManufacturerPartNumber);

                            //product title
                            var productTitle = ReplaceSpecChars(product.Name);

                            //category
                            var productCategory = _categoryService.GetProductCategoriesByProductId(product.Id).FirstOrDefault();
                            var categorization = productCategory != null && productCategory.Category != null
                                ? productCategory.Category.GetFormattedBreadCrumb(_categoryService, ">") : "no category";
                            categorization = ReplaceSpecChars(categorization);

                            //product URL
                            var productUrl = string.Format("{0}{1}", storeUrl, product.GetSeName());

                            //image Url
                            var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                            var storeUrlNotSsl = _webHelper.GetStoreLocation(false); //always use HTTP when getting image URL
                            var imageUrl = picture != null
                                ? _pictureService.GetPictureUrl(picture, model.ProductPictureSize, storeLocation: storeUrlNotSsl)
                                : _pictureService.GetDefaultPictureUrl(model.ProductPictureSize, storeLocation: storeUrlNotSsl);

                            //description
                            var description = !string.IsNullOrEmpty(product.FullDescription) ? product.FullDescription
                                : !string.IsNullOrEmpty(product.ShortDescription) ? product.ShortDescription : product.Name;
                            description = ReplaceSpecChars(Core.Html.HtmlHelper.StripTags(description));

                            //price
                            var currency = _currencyService.GetCurrencyById(model.CurrencyId);
                            var priceAmount = currency != null ? _currencyService.ConvertFromPrimaryStoreCurrency(product.Price, currency) : product.Price;
                            var price = priceAmount.ToString(new CultureInfo("en-US", false).NumberFormat);

                            //condition
                            var condition = "New";

                            //availability
                            var availability = product.StockQuantity > 0 ? "Yes" : "No";

                            //write line
                            writer.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}",
                                sku, manufacturerName, manufacturerPartNumber, productTitle, categorization,
                                productUrl, imageUrl, description, price, condition, availability);
                        }
                    }
                }

                //link for the result
                model.GenerateFeedResult = string.Format("<a href=\"{0}content/files/exportimport/{1}\" target=\"_blank\">{2}</a>",
                    storeUrl, fileName, _localizationService.GetResource("Plugins.Feed.PriceGrabber.ClickHere"));

                SuccessNotification(_localizationService.GetResource("Plugins.Feed.PriceGrabber.Success"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                _logger.Error(exc.Message, exc);
            }

            //prepare currencies
            model.AvailableCurrencies = _currencyService.GetAllCurrencies().ToSelectList(x => (x as Currency).Name);

            return View("~/Plugins/Feed.PriceGrabber/Views/FeedPriceGrabber/Configure.cshtml", model);
        }

        #endregion
    }
}
