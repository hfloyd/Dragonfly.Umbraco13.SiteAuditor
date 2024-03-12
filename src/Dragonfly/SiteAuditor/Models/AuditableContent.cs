namespace Dragonfly.SiteAuditor.Models
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Umbraco.Cms.Core.Models;
	using Umbraco.Cms.Core.Models.Membership;
	using Umbraco.Cms.Core.Models.PublishedContent;

	/// <summary>
	/// Meta data about a Content Node for Auditing purposes.
	/// </summary>
	[DataContract]
	public class AuditableContent
	{
		#region Private vars
		//private UmbracoHelper umbHelper = new UmbracoHelper(UmbracoContext.Current);
		// private IContentService umbContentService = ApplicationContext.Current.Services.ContentService;

		private string _defaultDelimiter = " » ";
		#endregion

		#region Public Props
		/// <summary>
		/// Gets or sets the content node as an IContent
		/// </summary>
		public IContent UmbContentNode { get; internal set; }

		/// <summary>
		/// Gets or sets the content node and an IPublishedContent
		/// </summary>
		public IPublishedContent UmbPublishedNode { get; internal set; }

		/// <summary>
		/// The node path.
		/// </summary>
		/// <returns>
		/// The <see cref="string"/>.
		/// </returns>
		public IEnumerable<string> NodePath { get; internal set; }

		/// <summary>
		/// Default string used for NodePathAsText
		/// ' » ' unless explicitly changed
		/// </summary>
		public string DefaultDelimiter
		{
			get { return _defaultDelimiter; }
			internal set { _defaultDelimiter = value; }
		}

		/// <summary>
		/// Full path to node in a single delimited string using object's default delimiter
		/// </summary>
		public string NodePathAsText
		{
			get
			{
				var nodePath = string.Join(this.DefaultDelimiter, this.NodePath);
				return nodePath;
			}
		}

		/// <summary>
		/// Alias of the Template assigned to this Content Node. Returns "NONE" if there is no template.
		/// </summary>
		public string TemplateAlias { get; internal set; }

		/// <summary>
		/// Url with domain name. Returns "UNPUBLISHED" if there is no public url.
		/// </summary>
		public string FullNiceUrl { get; internal set; }

		public bool IsPublished { get; internal set; }

		/// <summary>
		/// Path-only Url. Returns "UNPUBLISHED" if there is no public url.
		/// </summary>
		public string RelativeNiceUrl { get; internal set; }

		public IUser? CreateUser { get; set; }
		public IUser? UpdateUser { get; set; }

		#endregion

		public AuditableContent()
		{

		}

		#region Methods

		public string NodePathAsCustomText(string Separator = " » ")
		{
			var nodePath = string.Join(Separator, this.NodePath);
			return nodePath;
		}

		#endregion

	}
}
