namespace Dragonfly.SiteAuditor.Models;

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;


public class AuditableDataType
{
	public string Name { get; set; } = "";
	public string EditorAlias { get; set; } = "";
	public Guid Guid { get; set; }
	public int Id { get; set; }

	public IEnumerable<KeyValuePair<IPropertyType, string>> UsedOnProperties { get; set; } = new List<KeyValuePair<IPropertyType, string>>();
	public string ConfigurationJson { get; set; } = "";
	public List<string> FolderPath { get; set; } = new List<string>();

	public IEnumerable<string> UsesElementsDirectly { get; set; } = new List<string>();

	public IEnumerable<string> UsesElementsAll { get; set; } = new List<string>();

	/// <summary>
	/// Default string used for NodePathAsText
	/// ' » ' unless explicitly changed
	/// </summary>
	public string DefaultDelimiter
	{
		get { return _defaultDelimiter; }
		internal set { _defaultDelimiter = value; }
	}
	private string _defaultDelimiter = " » ";

	/// <summary>
	/// Full path to node in a single delimited string using object's default delimiter
	/// </summary>
	public string PathAsText
	{
		get
		{
			var path = string.Join(this.DefaultDelimiter, this.FolderPath);
			return path;
		}
	}

}

