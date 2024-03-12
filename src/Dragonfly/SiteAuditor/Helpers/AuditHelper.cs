namespace Dragonfly.SiteAuditor.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dragonfly.SiteAuditor.Models;
    using Umbraco.Cms.Core.Models;
    using Umbraco.Cms.Core.Services;
    using Umbraco.Cms.Web.Common;
    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Core.Services;
    using Umbraco.Web;
    using Umbraco.Web.Composing;

    /// <summary>
    /// Helpers for Auditing
    /// </summary>
    public class AuditHelper
    {
        //private static UmbracoHelper umbHelper = Current.UmbracoHelper;
        //private static IContentService umbContentService = Current.Services.ContentService;
        //private static IContentTypeService umbContentTypeService = Current.Services.ContentTypeService;
        //private static IDataTypeService  umbDataTypeService = Current.Services.DataTypeService;
     

        //[Obsolete("Use 'string.Join()' instead")]
        //public static string ConvertToSeparatedString(List<string> ListOfStrings, string Separator)
        //{
        //    string myString = Separator;

        //    foreach (var listItem in ListOfStrings)
        //    {
        //        myString += Separator + listItem;
        //    }

        //    myString = myString.Replace(String.Concat(Separator, Separator), "");
            
        //    return myString;
        //}

        ///// <summary>
        ///// Returns a list of all DocumentType Aliases in the site
        ///// </summary>
        ///// <returns></returns>
        //public static IEnumerable<string> GetAllContentTypesAliases()
        //{
        //    List<string> docTypesList = new List<string>();

        //    foreach (var dt in umbContentTypeService.GetAllContentTypeAliases())
        //    {
        //        docTypesList.Add(dt);
        //    }

        //    return docTypesList;
        //}

        

        ///// <summary>
        ///// Get a DataTypeDefinition for a Datatype by its DataTypeDefinitionId
        ///// </summary>
        ///// <param name="DtDefinitionId"></param>
        ///// <returns></returns>
        //public static IDataType GetDataTypeDefinition(int DtDefinitionId)
        //{
        //    return umbDataTypeService.GetDataType(DtDefinitionId);
        //}



      
        ///// <summary>
        ///// Get the Alias of a template from its ID. If the Id is null or zero, "NONE" will be returned.
        ///// </summary>
        ///// <param name="TemplateId">
        ///// The template id.
        ///// </param>
        ///// <returns>
        ///// The <see cref="string"/>.
        ///// </returns>
        //public static string GetTemplateAlias(int? TemplateId)
        //{
        //    var umbFileService = Current.Services.FileService;
        //    string templateAlias;

        //    if (TemplateId == 0 | TemplateId == null)
        //    {
        //        templateAlias = "NONE";
        //    }
        //    else
        //    {
        //        var lookupTemplate = umbFileService.GetTemplate(Convert.ToInt32(TemplateId));
        //        templateAlias = lookupTemplate.Alias;
        //    }
            
        //    return templateAlias;
        //}



       

        /// <summary>
        /// Get a NodePropertyDataTypeInfo model for a specified Node and Property Alias
        /// (Includes information about the Property, Datatype, and the node's property Value)
        /// </summary>
        /// <param name="PropertyAlias"></param>
        /// <param name="Node"></param>
        /// <returns></returns>
        public static NodePropertyDataTypeInfo GetPropertyDataTypeInfo(ServiceContext Services, string PropertyAlias, AuditableContent Node)
        {
            var umbContentTypeService = Services.ContentTypeService;
            var umbDataTypeService = Services.DataTypeService;

            var dtInfo = new NodePropertyDataTypeInfo();
            dtInfo.NodeId = Node.UmbContentNode.Id;

            //if (Node.UmbPublishedNode != null)
            //{
            //    dtInfo.Property = Node.UmbPublishedNode.GetProperty(PropertyAlias);
            //}
            //else
            //{
            dtInfo.Property = Node.UmbContentNode.Properties[PropertyAlias];
            //}

            //Find datatype of property
            IDataType dataType = null;

            var docType = umbContentTypeService.Get(Node.UmbContentNode.ContentTypeId);
            var matchingProperties = docType.PropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();

            if (matchingProperties.Any())
            {
                var propertyType = matchingProperties.First();
                dataType = umbDataTypeService.GetDataType(propertyType.DataTypeId);

                dtInfo.DataType = dataType;
                dtInfo.PropertyEditorAlias = dataType.EditorAlias;
                dtInfo.DatabaseType = dataType.DatabaseType.ToString();
                dtInfo.DocTypeAlias = docType.Alias;
            }
            else
            {
                //Look at Compositions for prop data
                var matchingCompProperties = docType.CompositionPropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
                if (matchingCompProperties.Any())
                {
                    var propertyType = matchingCompProperties.First();
                    dataType = umbDataTypeService.GetDataType(propertyType.DataTypeId);

                    dtInfo.DataType = dataType;
                    dtInfo.PropertyEditorAlias = dataType.EditorAlias;
                    dtInfo.DatabaseType = dataType.DatabaseType.ToString();

                    if (docType.ContentTypeComposition.Any())
                    {
                        var compsList = docType.ContentTypeComposition.Where(n => n.PropertyTypeExists(PropertyAlias)).ToList();
                        if (compsList.Any())
                        {
                            dtInfo.DocTypeAlias = docType.Alias;
                            dtInfo.DocTypeCompositionAlias = compsList.First().Alias;
                        }
                        else
                        {
                            dtInfo.DocTypeAlias = docType.Alias;
                            dtInfo.DocTypeCompositionAlias = "Unknown Composition";
                        }
                    }
                }
                else
                {
                    dtInfo.ErrorMessage = $"No property found for alias '{PropertyAlias}' in DocType '{docType.Name}'";
                }
            }

            return dtInfo;
        }
    }
}
