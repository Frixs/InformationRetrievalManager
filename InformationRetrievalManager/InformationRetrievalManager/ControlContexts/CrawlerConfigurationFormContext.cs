﻿using InformationRetrievalManager.Core;
using InformationRetrievalManager.Relational;

namespace InformationRetrievalManager
{
    /// <summary>
    /// Context for control <see cref="CrawlerConfigurationForm"/>.
    /// </summary>
    public class CrawlerConfigurationFormContext : BaseViewModel
    {
        #region Public Properties

        public TextEntryViewModel SiteAddressEntry { get; set; }
        public TextEntryViewModel SiteSuffixEntry { get; set; }
        public IntegerEntryViewModel StartPageNoEntry { get; set; }
        public IntegerEntryViewModel MaxPageNoEntry { get; set; }
        public IntegerEntryViewModel PageNoModifierEntry { get; set; }
        public IntegerEntryViewModel SearchIntervalEntry { get; set; }
        public TextEntryViewModel SiteUrlArticlesXPathEntry { get; set; }
        public TextEntryViewModel SiteArticleContentAreaXPathEntry { get; set; }
        public TextEntryViewModel SiteArticleTitleXPathEntry { get; set; }
        public TextEntryViewModel SiteArticleCategoryXPathEntry { get; set; }
        public TextEntryViewModel SiteArticleDateTimeXPathEntry { get; set; }
        public TextEntryViewModel SiteArticleDateTimeFormatEntry { get; set; }
        public TextEntryViewModel SiteArticleDateTimeCultureInfoEntry { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public CrawlerConfigurationFormContext()
        {
            SiteAddressEntry = new TextEntryViewModel
            {
                Label = "Site Address",
                Description = "The website you want to search data for.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteAddress),
                Value = CrawlerConfigurationDataModel.SiteAddress_DefaultValue,
                Placeholder = "E.g.: https://www.pcgamer.com",
                MaxLength = CrawlerConfigurationDataModel.SiteAddress_MaxLength
            };
            SiteSuffixEntry = new TextEntryViewModel
            {
                Label = "Site Suffix",
                Description = "The site suffix should contain '{0}' to mark the position of the page number.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteSuffix),
                Value = CrawlerConfigurationDataModel.SiteSuffix_DefaultValue,
                Placeholder = "E.g.: /news/page/{0}",
                MaxLength = CrawlerConfigurationDataModel.SiteSuffix_MaxLength
            };
            StartPageNoEntry = new IntegerEntryViewModel
            {
                Label = "Start Page Number",
                Description = "The first page to search.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, int, ValidateIntegerAttribute>(o => o.StartPageNo),
                Value = CrawlerConfigurationDataModel.StartPageNo_DefaultValue,
                MinValue = CrawlerConfigurationDataModel.StartPageNo_MinValue,
                MaxValue = CrawlerConfigurationDataModel.StartPageNo_MaxValue
            };
            MaxPageNoEntry = new IntegerEntryViewModel
            {
                Label = "Max Allowed Page Number",
                Description = "The maximal page number that should not be overpassed during searching.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, int, ValidateIntegerAttribute>(o => o.MaxPageNo),
                Value = CrawlerConfigurationDataModel.MaxPageNo_DefaultValue,
                MinValue = CrawlerConfigurationDataModel.MaxPageNo_MinValue,
                MaxValue = CrawlerConfigurationDataModel.MaxPageNo_MaxValue
            };
            PageNoModifierEntry = new IntegerEntryViewModel
            {
                Label = "Page Number Step",
                Description = "Incremental step between page numbers.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, int, ValidateIntegerAttribute>(o => o.PageNoModifier),
                Value = CrawlerConfigurationDataModel.PageNoModifier_DefaultValue,
                MinValue = CrawlerConfigurationDataModel.PageNoModifier_MinValue,
                MaxValue = CrawlerConfigurationDataModel.PageNoModifier_MaxValue
            };
            SearchIntervalEntry = new IntegerEntryViewModel
            {
                Label = "Search Interval",
                Description = "The delay (stated in milliseconds) between each crawler searches to lighten the load on the searched site.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, int, ValidateIntegerAttribute>(o => o.SearchInterval),
                Value = CrawlerConfigurationDataModel.SearchInterval_DefaultValue,
                MinValue = CrawlerConfigurationDataModel.SearchInterval_MinValue,
                MaxValue = CrawlerConfigurationDataModel.SearchInterval_MaxValue
            };
            SiteUrlArticlesXPathEntry = new TextEntryViewModel
            {
                Label = "XPath: Article URLs",
                Description = "Select links to specific articles.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteUrlArticlesXPath),
                Value = CrawlerConfigurationDataModel.SiteUrlArticlesXPath_DefaultValue,
                Placeholder = "E.g. //div[contains(@class, 'listingResults')]//a[@class='article-link']",
                MaxLength = CrawlerConfigurationDataModel.SiteUrlArticlesXPath_MaxLength
            };
            SiteArticleContentAreaXPathEntry = new TextEntryViewModel
            {
                Label = "XPath: Article Content Area",
                Description = "Select area where the content body is located.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteArticleContentAreaXPath),
                Value = CrawlerConfigurationDataModel.SiteArticleContentAreaXPath_DefaultValue,
                Placeholder = "E.g. //div[@id='article-body']",
                MaxLength = CrawlerConfigurationDataModel.SiteArticleContentAreaXPath_MaxLength
            };
            SiteArticleTitleXPathEntry = new TextEntryViewModel
            {
                Label = "XPath: Article Title",
                Description = "Select the title node.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteArticleTitleXPath),
                Value = CrawlerConfigurationDataModel.SiteArticleTitleXPath_DefaultValue,
                Placeholder = "E.g. //h1",
                MaxLength = CrawlerConfigurationDataModel.SiteArticleTitleXPath_MaxLength
            };
            SiteArticleCategoryXPathEntry = new TextEntryViewModel
            {
                Label = "XPath: Article Category",
                Description = "Select the category node.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteArticleCategoryXPath),
                Value = CrawlerConfigurationDataModel.SiteArticleCategoryXPath_DefaultValue,
                Placeholder = "E.g. (//nav[@id='articleTag']//div[@class='tag'])[1]//a",
                MaxLength = CrawlerConfigurationDataModel.SiteArticleCategoryXPath_MaxLength
            };
            SiteArticleDateTimeXPathEntry = new TextEntryViewModel
            {
                Label = "XPath: Article DateTime",
                Description = "Select the article's timestamp/datetime of publication.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteArticleDateTimeXPath),
                Value = CrawlerConfigurationDataModel.SiteArticleDateTimeXPath_DefaultValue,
                Placeholder = "E.g. //time[@itemprop='datePublished']",
                MaxLength = CrawlerConfigurationDataModel.SiteArticleDateTimeXPath_MaxLength
            };
            SiteArticleDateTimeFormatEntry = new TextEntryViewModel
            {
                Label = "Article DateTime Format",
                Description = "The exact format of the publication datetime.",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteArticleDateTimeFormat),
                Value = CrawlerConfigurationDataModel.SiteArticleDateTimeFormat_DefaultValue,
                Placeholder = "E.g. dd' 'MMM' 'yyyy",
                MaxLength = CrawlerConfigurationDataModel.SiteArticleDateTimeFormat_MaxLength
            };
            SiteArticleDateTimeCultureInfoEntry = new TextEntryViewModel
            {
                Label = "Article DateTime Culture",
                Description = "Culture info for the DateTime (necessary only if the DateTime consist of named months e.g.).",
                Validation = ValidationHelpers.GetPropertyValidateAttribute<CrawlerConfigurationDataModel, string, ValidateStringAttribute>(o => o.SiteArticleDateTimeCultureInfo),
                Value = CrawlerConfigurationDataModel.SiteArticleDateTimeCultureInfo_DefaultValue,
                Placeholder = "E.g. en-US",
                MaxLength = CrawlerConfigurationDataModel.SiteArticleDateTimeCultureInfo_MaxLength
            };
        }

        /// <summary>
        /// Initialize the context with values for each entry in this context.
        /// </summary>
        /// <returns>Return self for chaining.</returns>
        public CrawlerConfigurationFormContext Set(
            string siteAddress, string siteSuffix,
            int startPageNo, int maxPageNo, int pageNoModifier,
            int searchInterval,
            string siteUrlArticlesXPath,
            string siteArticleContentAreaXPath,
            string siteArticleTitleXPath,
            string siteArticleCategoryXPath,
            string siteArticleDateTimeXPath,
            string siteArticleDateTimeFormat,
            string siteArticleDateTimeCultureInfo)
        {
            SiteAddressEntry.Value = siteAddress;
            SiteSuffixEntry.Value = siteSuffix;
            StartPageNoEntry.Value = startPageNo;
            MaxPageNoEntry.Value = maxPageNo;
            PageNoModifierEntry.Value = pageNoModifier;
            SearchIntervalEntry.Value = searchInterval;
            SiteUrlArticlesXPathEntry.Value = siteUrlArticlesXPath;
            SiteArticleContentAreaXPathEntry.Value = siteArticleContentAreaXPath;
            SiteArticleTitleXPathEntry.Value = siteArticleTitleXPath;
            SiteArticleCategoryXPathEntry.Value = siteArticleCategoryXPath;
            SiteArticleDateTimeXPathEntry.Value = siteArticleDateTimeXPath;
            SiteArticleDateTimeFormatEntry.Value = siteArticleDateTimeFormat;
            SiteArticleDateTimeCultureInfoEntry.Value = siteArticleDateTimeCultureInfo;

            return this;
        }

        /// <summary>
        /// Set the context readonly access to each entry in this context.
        /// </summary>
        /// <returns>Return self for chaining.</returns>
        public CrawlerConfigurationFormContext ReadOnly(bool readOnlyInputs)
        {
            SiteAddressEntry.IsReadOnly =
            SiteSuffixEntry.IsReadOnly =
            StartPageNoEntry.IsReadOnly =
            MaxPageNoEntry.IsReadOnly =
            PageNoModifierEntry.IsReadOnly =
            SearchIntervalEntry.IsReadOnly =
            SiteUrlArticlesXPathEntry.IsReadOnly =
            SiteArticleContentAreaXPathEntry.IsReadOnly =
            SiteArticleTitleXPathEntry.IsReadOnly =
            SiteArticleCategoryXPathEntry.IsReadOnly =
            SiteArticleDateTimeXPathEntry.IsReadOnly =
            SiteArticleDateTimeFormatEntry.IsReadOnly =
            SiteArticleDateTimeCultureInfoEntry.IsReadOnly = readOnlyInputs;

            return this;
        }

        #endregion
    }
}
