namespace Dragonfly.SiteAuditor.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using Dragonfly.SiteAuditor.Helpers;
	using Newtonsoft.Json;
	using Umbraco.Cms.Core.Models;
	using Umbraco.Cms.Core.Models.PublishedContent;
	using Umbraco.Cms.Core.PropertyEditors;

	[DataContract]
	public class AuditableProperty
	{
		private List<PropertyDoctypeInfo> _docTypes = new List<PropertyDoctypeInfo>();

		#region Public Props
		[DataMember]
		public IPropertyType UmbPropertyType { get; internal set; }

		[DataMember]
		public string InDocType { get; internal set; }

		[DataMember]
		public string InDocTypeGroup { get; internal set; }

		[DataMember]
		public List<PropertyDoctypeInfo> AllDocTypes
		{
			get { return _docTypes; }
			internal set { _docTypes = value; }
		}

		[DataMember]
		public IDataType DataType { get; internal set; }

		[DataMember]
		public Dictionary<string, string> DataTypeConfigDictionary { get; internal set; }

		[DataMember]
		public Type DataTypeConfigType { get; internal set; }

		//     [DataMember]
		//      public bool IsNestedContent { get; internal set; }
		//   [DataMember]
		//  public NestedContentConfiguration.ContentType[] NestedContentDocTypesConfig { get; internal set; }

		public string InComposition { get; set; }
		public string GroupName { get; set; }

		[DataMember]
		public IEnumerable<string> DataTypeElementTypes { get; set; }

		#endregion

		#region Methods


		#endregion

	}

	public class SiteAuditableProperties
	{
		public string PropsForDoctype { get; internal set; }
		public IEnumerable<AuditableProperty> AllProperties { get; internal set; }
	}

	public class PropertyDoctypeInfo
	{
		public int Id { get; set; }
		public string DocTypeAlias { get; set; }
		public string GroupName { get; set; }

	}
}
