namespace Dragonfly.SiteAuditor.Models;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Extensions;


/// <summary>
/// Meta data about a Content Node for Auditing purposes.
/// </summary>
[DataContract]
public class AuditableContentExport
{

	#region Private vars / Methods
	private string _defaultDelimiter = " » ";

	/// <summary>
	/// Default string used for NodePathAsText
	/// ' » ' unless explicitly changed
	/// </summary>
	public void SetDefaultDelimiter(string Delimiter)
	{
		_defaultDelimiter = Delimiter;
	}

	#endregion

	#region Public Props
	public int OverallSort { get; set; }
	public string NodeName { get; set; } = "";

	public IEnumerable<string> NodePath { get; set; } = new List<string>();

	/// <summary>
	/// Full path to node in a single delimited string using object's default delimiter
	/// </summary>
	public string NodePathAsText
	{
		get
		{
			var nodePath = string.Join(_defaultDelimiter, this.NodePath);
			return nodePath;
		}
	}

	public string DocTypeAlias { get; set; } = "";

	public int ParentId { get; set; }

	/// <summary>
	/// Url with domain name. Returns "UNPUBLISHED" if there is no public url.
	/// </summary>
	public string FullUrl { get; set; } = "";

	/// <summary>
	/// Path-only Url. Returns "UNPUBLISHED" if there is no public url.
	/// </summary>
	public string RelativeUrl { get; set; } = "";

	public int Level { get; set; }

	public int SortOrder { get; set; }

	/// <summary>
	/// Alias of the Template assigned to this Content Node. Returns "NONE" if there is no template.
	/// </summary>
	public string TemplateAlias { get; set; } = "";

	public DateTime CreateDate { get; set; }

	public string CreateUser { get; set; } = "";
	public DateTime UpdateDate { get; set; }

	public string UpdateUser { get; set; } = "";

	public bool IsPublished { get; set; }

	public int NodeId { get; set; }

	public Guid NodeGuid { get; set; }

	public string Udi { get; set; } = "";


	#endregion

	public AuditableContentExport() { }

	public AuditableContentExport(AuditableContent Ac, int OverallSortNum)
	{
		this.OverallSort = OverallSortNum;

		this.NodeName = Ac.UmbContentNode.Name != null ? Ac.UmbContentNode.Name : "UNKNOWN";
		this.NodePath = Ac.NodePath;
		this.DocTypeAlias = Ac.UmbContentNode.ContentType.Alias;
		this.ParentId = Ac.UmbContentNode.ParentId;
		this.FullUrl = Ac.FullNiceUrl;
		this.RelativeUrl = Ac.RelativeNiceUrl;
		this.Level = Ac.UmbContentNode.Level;
		this.SortOrder = Ac.UmbContentNode.SortOrder;
		this.TemplateAlias = Ac.TemplateAlias;
		this.CreateDate = Ac.UmbContentNode.CreateDate;
		this.CreateUser = Ac.CreateUser != null ? Ac.CreateUser.Username : "UNKNOWN";
		this.UpdateDate = Ac.UmbContentNode.UpdateDate;
		this.UpdateUser = Ac.UpdateUser != null ? Ac.UpdateUser.Username : "UNKNOWN";
		this.IsPublished = Ac.IsPublished;
		this.NodeId = Ac.UmbContentNode.Id;
		this.NodeGuid = Ac.UmbContentNode.Key;
		this.Udi = Ac.UmbContentNode.GetUdi().ToString();
	}


}

public static class AuditableContentExportExtensions
{
	public static IEnumerable<AuditableContentExport> ConvertToExportable(this IEnumerable<AuditableContent> Items)
	{
		var list = new List<AuditableContentExport>();
		var counter = 1;

		foreach (var ac in Items)
		{
			var eac = new AuditableContentExport(ac, counter);
			list.Add(eac);
			counter++;
		}

		return list;
	}
}



