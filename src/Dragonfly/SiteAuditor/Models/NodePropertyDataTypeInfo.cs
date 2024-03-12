namespace Dragonfly.SiteAuditor.Models
{
    using Umbraco.Cms.Core.Models;

    public class NodePropertyDataTypeInfo
    {
        public int NodeId { get; set; }
        public string DocTypeAlias { get; set; } = "";
        public string? DocTypeCompositionAlias { get; set; }
        public string PropertyEditorAlias { get; set; }= "";
        public string DatabaseType { get; set; }= "";
        public string ErrorMessage { get; set; }= "";
        public IDataType? DataType { get; set; }
        public IProperty? Property { get; set; }
        public object? PropertyData { get; set; }

        public bool HasError
        {
            get
            {
                if (ErrorMessage != "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool UsesComposition
        {
            get
            {
                if (!string.IsNullOrEmpty(DocTypeCompositionAlias))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

     
    }
}
