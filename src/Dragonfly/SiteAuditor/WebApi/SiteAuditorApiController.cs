namespace Dragonfly.SiteAuditor.WebApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Net;
	

	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.ViewFeatures;
	using Microsoft.Extensions.Logging;

	using System.Text.Json;
	using Umbraco.Cms.Infrastructure.PublishedCache;
	using Umbraco.Cms.Web.Common.Attributes;
	using Umbraco.Cms.Web.Common.UmbracoContext;
	using Umbraco.Cms.Core.Services;
	using Umbraco.Cms.Web.BackOffice;
	using Umbraco.Cms.Web.BackOffice.Controllers;
	using Umbraco.Cms.Web.Common.Attributes;
	using Umbraco.Extensions;

	using Dragonfly.NetHelperServices;
	using Dragonfly.NetModels;
	using Dragonfly.UmbracoHelpers;

	using Dragonfly.SiteAuditor.Models;
	using Dragonfly.SiteAuditor.Services;


	//  /umbraco/backoffice/Dragonfly/SiteAuditor/
	[PluginController("Dragonfly")]
	[IsBackOffice]
	public class SiteAuditorController : UmbracoAuthorizedApiController
	{
		#region ctor & DI
		private readonly ILogger<SiteAuditorController> _logger;
		private readonly SiteAuditorService _siteAuditorService;
		private readonly IViewRenderService _viewRenderService;

		public SiteAuditorController(
			ILogger<SiteAuditorController> logger,
			SiteAuditorService siteAuditorService,
			IViewRenderService viewRenderService
			)
		{
			_logger = logger;
			_siteAuditorService = siteAuditorService;
			_viewRenderService = viewRenderService;

		}

		#endregion
		private string RazorFilesPath()
		{
			return SiteAuditorService.PluginPath() + "RazorViews/";
		}

		private SiteAuditorService GetSiteAuditorService()
		{
			return _siteAuditorService;
			//return new SiteAuditorService(Umbraco, UmbracoContext, Services, Logger<>);
		}

		internal StandardViewInfo GetStandardViewInfo()
		{
			var info = new StandardViewInfo();

			info.CurrentToolVersion = PackageInfo.Version;

			return info;
		}


		#region Content Nodes

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsJson
		[HttpGet]
		public IActionResult GetAllContentAsJson()
		{
			var saService = GetSiteAuditorService();
			var allNodes = saService.GetContentNodes();
			var exportable = allNodes.ConvertToExportable();

			//Return JSON
			return new JsonResult(exportable);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsHtmlTable
		[HttpGet]
		public IActionResult GetAllContentAsHtmlTable()
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllContentAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			var contentNodes = saService.GetContentNodes();

			//VIEW DATA 
			var model = contentNodes;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsCsv
		[HttpGet]
		public IActionResult GetAllContentAsCsv()
		{
			var saService = GetSiteAuditorService();
			var returnSB = new StringBuilder();

			var allNodes = saService.GetContentNodes();

			var tableData = new StringBuilder();

			tableData.AppendLine(
				"\"Node Name\"" +
				",\"NodeID\"" +
				",\"Node Path\"" +
				",\"DocType\"" +
				",\"ParentID\"" +
				",\"Full URL\"" +
				",\"Level\"" +
				",\"Sort Order\"" +
				",\"Template Name\"" +
				",\"Udi\"" +
				",\"Create Date\"" +
				",\"Update Date\"");

			foreach (var auditNode in allNodes)
			{
				var nodeLine = $"\"{auditNode.UmbContentNode.Name}\"" +
							   $",{auditNode.UmbContentNode.Id}" +
							   $",\"{auditNode.NodePathAsCustomText(" > ")}\"" +
							   $",\"{auditNode.UmbContentNode.ContentType.Alias}\"" +
							   $",{auditNode.UmbContentNode.ParentId}" +
							   $",\"{auditNode.FullNiceUrl}\"" +
							   $",{auditNode.UmbContentNode.Level}" +
							   $",{auditNode.UmbContentNode.SortOrder}" +
							   $",\"{auditNode.TemplateAlias}\"" +
							   $",\"{auditNode.UmbContentNode.GetUdi()}\"" +
							   $",{auditNode.UmbContentNode.CreateDate}" +
							   $",{auditNode.UmbContentNode.UpdateDate}" +
							   $"{Environment.NewLine}";

				tableData.Append(nodeLine);
			}
			returnSB.Append(tableData);


			//RETURN AS CSV FILE
			var fileName = "AllContentNodes.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

			//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
			//{
			//	FileName = "AllContentNodes.csv",
			//	DispositionType = "attachment"
			//};
			//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
			//return Content(
			//	returnSB.ToString(),
			//	"text/csv",
			//	Encoding.UTF8
			//);
			
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetContentForDoctypeHtml?DocTypeAlias=X
		[HttpGet]
		public IActionResult GetContentForDoctypeHtml(string DocTypeAlias = "")
		{
			if (DocTypeAlias != "")
			{
				return ContentForDoctypeHtml(DocTypeAlias);
			}
			else
			{
				return DoctypesForContentForDoctypeHtml();
			}
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetContentForDoctypeHtml?DocTypeAlias=X
		[HttpGet]
		public IActionResult GetContentForElementHtml(string DocTypeAlias = "")
		{
			if (DocTypeAlias != "")
			{
				return ContentForElementHtml(DocTypeAlias);
			}
			else
			{
				return DoctypesForContentForDoctypeHtml();
			}
		}

		private IActionResult DoctypesForContentForDoctypeHtml()
		{
			//Setup
			//  var pvPath = RazorFilesPath() + "Start.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			var allSiteDocTypes = saService.GetAllDocTypes().ToList();
			var allNodeTypes = allSiteDocTypes.Where(n => n.IsElement == false).OrderBy(n => n.Alias).ToList();
			var allElementTypes = allSiteDocTypes.Where(n => n.IsElement == true).OrderBy(n => n.Alias).ToList();


			//BUILD HTML
			var returnSB = new StringBuilder();
			returnSB.AppendLine($"<h1>Get Content for a Selected Document Type</h1>");
			returnSB.AppendLine($"<p id=\"Nav\"><a href=\"#ContentNodes\">Content Node Document Types</a> | <a href=\"#Elements\">Element Types</a></p>");


			returnSB.AppendLine($"<h3 id=\"ContentNodes\">Available Content Node Document Types</h3>");
			returnSB.AppendLine("<ul>");
			foreach (var docType in allNodeTypes)
			{
				var type = docType.IsElement ? "Nodes Using Element" : "Content Nodes of Type";
				var api = docType.IsElement ? "GetContentForElementHtml" : "GetContentForDoctypeHtml";
				var url = $"/umbraco/backoffice/Dragonfly/SiteAuditor/{api}?DocTypeAlias={docType.Alias}";

				returnSB.AppendLine($"<li>{docType.Alias} <a target=\"_blank\" href=\"{url}\">View</a></li>");
			}
			returnSB.AppendLine("</ul>");
			returnSB.AppendLine($"<p><a href=\"#Nav\">Top Nav</a></p>");

			returnSB.AppendLine($"<h3 id=\"Elements\">Available Element Document Types</h3>");
			returnSB.AppendLine("<p>Note: These options will take longer to load because they have to recursively check the content property values.</p>");
			returnSB.AppendLine("<ul>");
			foreach (var docType in allElementTypes)
			{
				var type = docType.IsElement ? "Nodes Using Element" : "Content Nodes of Type";
				var api = docType.IsElement ? "GetContentForElementHtml" : "GetContentForDoctypeHtml";
				var url = $"/umbraco/backoffice/Dragonfly/SiteAuditor/{api}?DocTypeAlias={docType.Alias}";

				returnSB.AppendLine($"<li>{docType.Alias} <a target=\"_blank\" href=\"{url}\">View</a></li>");
			}
			returnSB.AppendLine("</ul>");
			returnSB.AppendLine($"<p><a href=\"#Nav\">Top Nav</a></p>");

			var displayHtml = returnSB.ToString();

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};

		}

		private IActionResult ContentForDoctypeHtml(string DocTypeAlias)
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllContentAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			var contentNodes = saService.GetContentNodes(DocTypeAlias);

			//VIEW DATA 
			var model = contentNodes;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};

		}

		private IActionResult ContentForElementHtml(string DocTypeAlias)
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllElementContentAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			var contentNodes = saService.GetContentNodesUsingElement(DocTypeAlias);

			//VIEW DATA 
			var model = contentNodes;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}


		#endregion

		#region GetContentWithValues

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues?PropertyAlias=xxx
		[HttpGet]
		public IActionResult GetContentWithValues(string PropertyAlias = "")
		{
			//Setup
			var pvPath = RazorFilesPath() + "ContentWithValuesTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			var displayHtml = "";

			if (PropertyAlias == "")
			{
				//Get list of properties
				displayHtml = HtmlListOfProperties();
			}
			else
			{
				//Get matching Property data
				var contentNodes = saService.GetContentWithProperty(PropertyAlias);

				//VIEW DATA 
				var model = contentNodes;
				var viewData = new Dictionary<string, object>();
				viewData.Add("StandardInfo", GetStandardViewInfo());
				viewData.Add("Status", status);
				viewData.Add("PropertyAlias", PropertyAlias);
				// viewData.Add("IncludeUnpublished", IncludeUnpublished);

				//RENDER
				var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
				displayHtml = htmlTask.Result;
			}

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}

		private string HtmlListOfProperties()
		{
			//Setup
			//    var pvPath = RazorFilesPath() + "Start.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			var allSiteDocTypes = saService.GetAllDocTypes();
			var allProps = allSiteDocTypes.SelectMany(n => n.PropertyTypes);
			var allPropsAliases = allProps.Select(n => n.Alias).Distinct();

			//BUILD HTML
			var returnSB = new StringBuilder();
			returnSB.AppendLine($"<h1>Get Content with Values</h1>");
			returnSB.AppendLine($"<h3>Available Properties</h3>");
			//returnSB.AppendLine("<p>Note: Choosing the 'All' option will take significantly longer to load than the 'Published' option because we need to bypass the cache and query the database directly.</p>");

			returnSB.AppendLine("<ol>");
			foreach (var propAlias in allPropsAliases.OrderBy(n => n))
			{
				//var url1 =
				//    $"/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues?PropertyAlias={propAlias}&IncludeUnpublished=false";
				var url2 = $"/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues?PropertyAlias={propAlias}";

				returnSB.AppendLine($"<li>{propAlias} <a target=\"_blank\" href=\"{url2}\">View</a></li>");
			}
			returnSB.AppendLine("</ol>");

			return returnSB.ToString();
		}

		#endregion

		#region Properties Info

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsJson
		[HttpGet]
		public IActionResult GetAllPropertiesAsJson()
		{
			var saService = GetSiteAuditorService();

			var siteProps = saService.AllProperties();
			var propertiesList = siteProps.AllProperties;

			//Return JSON
			return new JsonResult(propertiesList);
		}

		// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsHtml
		[HttpGet]
		public IActionResult GetAllPropertiesAsHtmlTable()
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllPropertiesAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			var siteProps = saService.AllProperties();
			var propertiesList = siteProps.AllProperties;

			//VIEW DATA 
			var model = propertiesList;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsCsv
		[HttpGet]
		public IActionResult GetAllPropertiesAsCsv()
		{
			var saService = GetSiteAuditorService();
			var returnSB = new StringBuilder();

			var siteProps = saService.AllProperties();
			var propertiesList = siteProps.AllProperties;

			var tableData = new StringBuilder();

			tableData.AppendLine(
				"\"Property Name\",\"Property Alias\",\"DataType Name\",\"DataType Property Editor\",\"DataType Database Type\",\"DocumentTypes Used In\",\"Qty of DocumentTypes\"");

			foreach (var prop in propertiesList)
			{
				tableData.AppendFormat(
					"\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",{6}{7}",
					prop.UmbPropertyType.Name,
					prop.UmbPropertyType.Alias,
					prop.DataType.Name,
					prop.DataType.EditorAlias,
					prop.DataType.DatabaseType,
					string.Join(", ", prop.AllDocTypes.Select(n => n.DocTypeAlias)),
					prop.AllDocTypes.Count(),
					Environment.NewLine);
			}

			returnSB.Append(tableData);


			//RETURN AS CSV FILE
			var fileName = "AllProperties.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

			//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
			//{
			//	FileName = "AllProperties.csv",
			//	DispositionType = "attachment"
			//};
			//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
			//return new ContentResult()
			//{
			//	Content = returnSB.ToString(),
			//	StatusCode = (int)HttpStatusCode.OK,
			//	ContentType = "text/csv; charset=utf-8",
			//};

		}

		// /umbraco/backoffice/Dragonfly/SiteAuditor/GetPropertiesForDoctypeHtml?DocTypeAlias=xxx
		[HttpGet]
		public IActionResult GetPropertiesForDoctypeHtml(string DocTypeAlias = "")
		{
			if (DocTypeAlias != "")
			{
				return PropsForDoctypeHtml(DocTypeAlias);
			}
			else
			{
				return DoctypesForPropertiesForDoctypeHtml();
			}
		}

		private IActionResult PropsForDoctypeHtml(string DocTypeAlias)
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllPropertiesAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			var siteProps = saService.AllPropertiesForDocType(DocTypeAlias);
			var propertiesList = siteProps.AllProperties;


			//VIEW DATA 
			var model = propertiesList;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);
			viewData.Add("Title", $"Properties for Document Type '{DocTypeAlias}'");

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);

		}

		private IActionResult DoctypesForPropertiesForDoctypeHtml()
		{
			//Setup
			var pvPath = RazorFilesPath() + "Start.cshtml";

			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			var allSiteDocTypes = saService.GetAllDocTypes();
			var allAliases = allSiteDocTypes.Select(n => n.Alias).OrderBy(n => n).ToList();


			//BUILD HTML
			var returnSB = new StringBuilder();

			returnSB.AppendLine($"<h1>Get Properties for a Selected Document Type</h1>");
			returnSB.AppendLine($"<h3>Available Document Types</h3>");
			//returnSB.AppendLine("<p>Note: Choosing the 'All' option will take significantly longer to load than the 'Published' option because we need to bypass the cache and query the database directly.</p>");

			returnSB.AppendLine("<ul>");
			foreach (var alias in allAliases)
			{
				var url = $"/umbraco/backoffice/Dragonfly/SiteAuditor/GetPropertiesForDoctypeHtml?DocTypeAlias={alias}";

				returnSB.AppendLine($"<li>{alias} <a target=\"_blank\" href=\"{url}\">View</a></li>");
			}
			returnSB.AppendLine("</ul>");
			var displayHtml = returnSB.ToString();

			//VIEW DATA 
			// var model = XXX;
			//var viewData = new Dictionary<string, object>();
			//viewData.Add("StandardInfo", GetStandardViewInfo());
			//viewData.Add("Status", status);

			////RENDER
			//var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			//var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}

		#endregion

		#region DataType Info

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsJson
		[HttpGet]
		public IActionResult GetAllDataTypesAsJson()
		{
			var saService = GetSiteAuditorService();

			var dataTypes = saService.AllDataTypes();

			//Return JSON
			return new JsonResult(dataTypes);
		}

		// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsHtmlTable
		[HttpGet]
		public IActionResult GetAllDataTypesAsHtmlTable()
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllDataTypesAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			var dataTypes = saService.AllDataTypes();

			//VIEW DATA 
			var model = dataTypes;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsCsv
		[HttpGet]
		public IActionResult GetAllDataTypesAsCsv()
		{
			var saService = GetSiteAuditorService();
			var returnSB = new StringBuilder();

			var dataTypes = saService.AllDataTypes();

			var tableData = new StringBuilder();

			tableData.AppendLine(
				"\"DataType Name\",\"Property Editor Alias\",\"Id\",\"Guid Key\",\"Used On Properties\",\"Qty of Properties\"");

			foreach (var dt in dataTypes)
			{
				tableData.AppendFormat(
					"\"{0}\",\"{1}\",{2},\"{3}\",\"{4}\",{5}{6}",
					dt.Name,
					dt.EditorAlias,
					dt.Id,
					dt.Guid.ToString(),
					string.Join(" | ", dt.UsedOnProperties.Select(n => $"{n.Value} [{n.Key.Alias}]")),
					dt.UsedOnProperties.Count(),
					Environment.NewLine);
			}

			returnSB.Append(tableData);

			//RETURN AS CSV FILE
			var fileName = "AllDataTypes.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

			//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
			//{
			//	FileName = "AllDataTypes.csv",
			//	DispositionType = "attachment"
			//};
			//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
			//return new ContentResult()
			//{
			//	Content = returnSB.ToString(),
			//	StatusCode = (int)HttpStatusCode.OK,
			//	ContentType = "text/csv; charset=utf-8",
			//};

		}


		#endregion

		#region DocTypes Info

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsJson
		[HttpGet]
		public IActionResult GetAllDocTypesAsJson()
		{
			var saService = GetSiteAuditorService();
			var allDts = saService.GetAuditableDocTypes();

			//Return JSON
			return new JsonResult(allDts);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsHtmlTable
		[HttpGet]
		public IActionResult GetAllDocTypesAsHtmlTable()
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllDocTypesAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			var allDts = saService.GetAuditableDocTypes();

			//VIEW DATA 
			var model = allDts;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsCsv
		[HttpGet]
		public IActionResult GetAllDocTypesAsCsv()
		{
			var saService = GetSiteAuditorService();
			var returnSB = new StringBuilder();

			var allDts = saService.GetAuditableDocTypes();

			var tableData = new StringBuilder();

			tableData.AppendLine(
				"\"Doctype Name\",\"Alias\",\"Default Template\",\"GUID\",\"Create Date\",\"Update Date\"");

			foreach (var item in allDts)
			{
				tableData.AppendFormat(
					"\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5}{6}",
					item.Name,
					item.Alias,
					item.DefaultTemplateName,
					item.Guid,
					item.ContentType.CreateDate,
					item.ContentType.UpdateDate,
					Environment.NewLine);
			}
			returnSB.Append(tableData);

			//RETURN AS CSV FILE
			var fileName = "AllDoctypes.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

			//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
			//{
			//	FileName = "AllDoctypes.csv",
			//	DispositionType = "attachment"
			//};
			//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
			//return new ContentResult()
			//{
			//	Content = returnSB.ToString(),
			//	StatusCode = (int)HttpStatusCode.OK,
			//	ContentType = "text/csv; charset=utf-8",
			//};

		}

		#endregion

		#region Templates Info

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsJson
		[HttpGet]
		public IActionResult GetAllTemplatesAsJson()
		{
			var saService = GetSiteAuditorService();
			var allTemps = saService.GetAuditableTemplates();

			//Return JSON
			return new JsonResult(allTemps);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsHtmlTable
		[HttpGet]
		public IActionResult GetAllTemplatesAsHtmlTable()
		{
			//Setup
			var pvPath = RazorFilesPath() + "AllTemplatesAsHtmlTable.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			var allTemps = saService.GetAuditableTemplates();

			//VIEW DATA 
			var model = allTemps;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsCsv
		[HttpGet]
		public IActionResult GetAllTemplatesAsCsv()
		{
			var saService = GetSiteAuditorService();
			var returnSB = new StringBuilder();

			var allTemplates = saService.GetAuditableTemplates();

			var tableData = new StringBuilder();

			tableData.AppendLine(
				"\"Template Name\",\"Alias\",\"Code Length\",\"GUID\",\"Create Date\",\"Update Date\"");

			foreach (var item in allTemplates)
			{
				tableData.AppendFormat(
					"\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5}{6}",
					item.Name,
					item.Alias,
					item.CodeLength,
					item.Guid,
					item.CreateDate,
					item.UpdateDate,
					Environment.NewLine);
			}
			returnSB.Append(tableData);

			//RETURN AS CSV FILE
			var fileName = "AllTemplates.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

		}

		#endregion

		#region Special Queries
		/// /umbraco/backoffice/Dragonfly/SiteAuditor/TemplateUsageReport
		[HttpGet]
		public IActionResult TemplateUsageReport()
		{
			//Setup
			var pvPath = RazorFilesPath() + "TemplateUsageReport.cshtml";
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			//VIEW DATA 
			var model = status;
			var viewData = new Dictionary<string, object>();
			viewData.Add("StandardInfo", GetStandardViewInfo());
			viewData.Add("Status", status);
			viewData.Add("TemplatesUsedOnContent", saService.TemplatesUsedOnContent());
			viewData.Add("TemplatesNotUsedOnContent", saService.TemplatesNotUsedOnContent());

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					displayHtml,
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}
		#endregion


		#region Obsolete - Remove in v13

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/Help
		[HttpGet]
		[Obsolete("Use the dashboard in the 'Settings' section. 2023-11-15")]
		public IActionResult Help()
		{
			//Setup
			// var pvPath = RazorFilesPath() + "Start.cshtml";

			//BUILD HTML
			//TODO: HLF - Convert to Razor View?
			var returnSB = new StringBuilder();
			returnSB.AppendLine("<h1>Site Auditor</h1>");
			returnSB.AppendLine("<h2>Content</h2>");
			returnSB.AppendLine("<h3>All Content Nodes</h3>");
			returnSB.AppendLine("<p>These will take a long time to run for large sites. Please be patient.</p>");
			returnSB.AppendLine("<ul>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsXml\">Get All Content As Xml</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsJson\">Get All Content As Json</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsHtml\">Get All Content As HtmlTable</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsCsv\">Get All Content As Csv</a> [no parameters]</li>");
			returnSB.AppendLine("</ul>");
			returnSB.AppendLine("<h3>Content Nodes with Property Data</h3>");
			//returnSB.AppendLine("<p>Note</p>");
			returnSB.AppendLine("<ul>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues\">Get Content With Values</a></li>");
			returnSB.AppendLine("</ul>");
			returnSB.AppendLine("<h2>Document Types</h2>");
			returnSB.AppendLine("<h3>All DocTypes</h3>");
			//returnSB.AppendLine("<p>Note</p>");
			returnSB.AppendLine("<ul>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsXml\">Get All Doctypes As Xml</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsJson\">Get All Doctypes As Json</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsHtml\">Get All Doctypes As Html</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsCsv\">Get All Doctypes As Csv</a> [no parameters]</li>");
			returnSB.AppendLine("</ul>");
			returnSB.AppendLine("<h2>Document Type Properties</h2>");
			returnSB.AppendLine("<h3>All Properties</h3>");
			//returnSB.AppendLine("<p>Note</p>");
			returnSB.AppendLine("<ul>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsXml\">Get All Properties As Xml</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsJson\">Get All Properties As Json</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsHtml\">Get All Properties As Html</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsCsv\">Get All Properties As Csv</a> [no parameters]</li>");
			returnSB.AppendLine("</ul>");

			returnSB.AppendLine("<h2>Data Types</h2>");

			returnSB.AppendLine("<h3>All DataTypes</h3>");
			//returnSB.AppendLine("<p>Note</p>");
			returnSB.AppendLine("<ul>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsXml\">Get All DataTypes As Xml</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsJson\">Get All DataTypes As Json</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsHtml\">Get All DataTypes As Html</a> [no parameters]</li>");
			returnSB.AppendLine("<li><a target=\"_blank\" href=\"/umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsCsv\">Get All DataTypes As Csv</a> [no parameters]</li>");
			returnSB.AppendLine("</ul>");

			//returnSB.AppendLine("<h3>All Content Nodes</h3>");
			//returnSB.AppendLine("<p>Note</p>");
			//returnSB.AppendLine("<ul>");
			//returnSB.AppendLine("<li><a target=\"_blank\" href=\"\"></a></li>");
			//returnSB.AppendLine("</ul>");

			//RETURN AS HTML
			var result = new HttpResponseMessage()
			{
				Content = new StringContent(
					returnSB.ToString(),
					Encoding.UTF8,
					"text/html"
				)
			};

			return new HttpResponseMessageResult(result);
		}


		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsXml
		[HttpGet]
		[Obsolete("This hasn't worked in the v10+ version. 2023-11-15")]
		public List<AuditableContent> GetAllContentAsXml()
		{
			var saService = GetSiteAuditorService();
			var allNodes = saService.GetContentNodes();

			return allNodes;
		}

		// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsXml
		[HttpGet]
		[Obsolete("This hasn't worked in the v10+ version. 2023-11-15")]
		public IEnumerable<AuditableProperty> GetAllPropertiesAsXml()
		{
			var saService = GetSiteAuditorService();
			var siteProps = saService.AllProperties();
			var propertiesList = siteProps.AllProperties;
			return propertiesList;
		}

		// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsXml
		[HttpGet]
		[Obsolete("This hasn't worked in the v10+ version. 2023-11-15")]
		public IEnumerable<AuditableDataType> GetAllDataTypesAsXml()
		{
			var saService = GetSiteAuditorService();
			var dataTypes = saService.AllDataTypes();

			return dataTypes;
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsXml
		[HttpGet]
		[Obsolete("This hasn't worked in the v10+ version. 2023-11-15")]
		public IEnumerable<AuditableDocType> GetAllDocTypesAsXml()
		{
			var saService = GetSiteAuditorService();
			var allDts = saService.GetAuditableDocTypes();

			return allDts;
		}


		/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsXml
		[HttpGet]
		[Obsolete("This hasn't worked in the v10+ version. 2023-11-15")]
		public IEnumerable<AuditableTemplate> GetAllTemplatesAsXml()
		{
			var saService = GetSiteAuditorService();
			var allTemps = saService.GetAuditableTemplates();

			return allTemps;
		}

		#endregion


		#region Tests & Examples

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/TestJson?DocTypeAlias=xxx
		[HttpGet]
		public IActionResult TestJson(string DocTypeAlias)
		{
			//Setup
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);
			try
			{
				var contentNodes = saService.GetContentNodesUsingElement(DocTypeAlias);
				status.RelatedObject = contentNodes;
			}
			catch (Exception e)
			{
				status.Success = false;
				status.SetRelatedException(e);
			}


			//Return JSON
			return new JsonResult(status);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/TestData?DocTypeAlias=xxx
		[HttpGet]
		public List<KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>> TestData(string DocTypeAlias)
		{
			_logger.LogInformation($"SiteAuditor.TestData STARTING...");

			//Setup
			var saService = GetSiteAuditorService();

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			var contentNodes = saService.GetContentNodesUsingElement(DocTypeAlias);
			status.RelatedObject = contentNodes;

			_logger.LogInformation($"SiteAuditor.TestData COMPLETED");
			return contentNodes;

		}



		/// /umbraco/backoffice/Dragonfly/AuthorizedApi/Test
		[HttpGet]
		public bool Test()
		{
			//LogHelper.Info<PublicApiController>("Test STARTED/ENDED");
			return true;
		}

		/// /umbraco/backoffice/Dragonfly/AuthorizedApi/ExampleReturnHtml
		[HttpGet]
		public IActionResult ExampleReturnHtml()
		{
			//Setup
			// var pvPath = RazorFilesPath() + "Start.cshtml";

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			//BUILD HTML
			var returnSB = new StringBuilder();
			returnSB.AppendLine("<h1>Hello! This is HTML</h1>");
			returnSB.AppendLine("<p>Use this type of return when you want to exclude &lt;XML&gt;&lt;/XML&gt; tags from your output and don\'t want it to be encoded automatically.</p>");
			var displayHtml = returnSB.ToString();

			////VIEW DATA 
			//var model = XXX;
			// var viewData = new Dictionary<string, object>();
			// viewData.Add("StandardInfo", GetStandardViewInfo());
			// viewData.Add("Status", status);

			// //RENDER
			// var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			// var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};

		}

		/// /umbraco/backoffice/Dragonfly/AuthorizedApi/ExampleReturnJson
		[HttpGet]
		public IActionResult ExampleReturnJson()
		{
			//var testData1 = new TimeSpan(1, 1, 1, 1);
			var testData2 = new StatusMessage(true, "This is a test object so you can see JSON!");

			//Return JSON
			return new JsonResult(testData2);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/ExampleReturnCsv
		[HttpGet]
		public IActionResult ExampleReturnCsv()
		{
			var returnSB = new StringBuilder();
			var tableData = new StringBuilder();

			for (int i = 0; i < 10; i++)
			{
				tableData.AppendFormat(
					"\"{0}\",{1},\"{2}\",{3}{4}",
					"Name " + i,
					i,
					string.Format("Some text about item #{0} for demo.", i),
					DateTime.Now,
					Environment.NewLine);
			}
			returnSB.Append(tableData);

			//RETURN AS CSV FILE
			var fileName = "Example.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);
			
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/TestCSV
		[HttpGet]
		public IActionResult TestCsv()
		{
			var returnSB = new StringBuilder();

			var tableData = new StringBuilder();

			for (int i = 0; i < 10; i++)
			{
				tableData.AppendFormat(
					"\"{0}\",{1},\"{2}\",{3}{4}",
					"Name " + i,
					i,
					string.Format("Some text about item #{0} for demo.", i),
					DateTime.Now,
					Environment.NewLine);
			}
			returnSB.Append(tableData);

			
			//RETURN AS CSV FILE
			var fileName = "Test.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

		}

		#endregion
	}
}
