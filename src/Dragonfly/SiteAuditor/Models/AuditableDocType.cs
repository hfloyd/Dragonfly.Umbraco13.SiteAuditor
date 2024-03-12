namespace Dragonfly.SiteAuditor.Models
{
    using System;
    using System.Collections.Generic;
   
    using Umbraco.Cms.Core.Models;
    using Umbraco.Cms.Core.Models.PublishedContent;

    public class AuditableDocType
    {
     
        #region Public Props

        public IContentType ContentType { get; set; }

        public string Name { get; set; }
        public string Alias { get; set; }
        public IEnumerable<string> FolderPath { get; internal set; }
        public Guid Guid { get; set; }
        public int Id { get; set; }
        public string DefaultTemplateName { get; set; }
        public Dictionary<int,string> AllowedTemplates { get; set; }
        public bool HasContentNodes { get; set; }
        public bool IsElement { get; set; }
        public bool IsComposition { get; set; }
        public Dictionary<int, string> CompositionsUsed { get; set; }
        

        //TODO: Add Info about compositions/parents/folders: IsComposition, HasCompositions, etc.

        #endregion
     

        public AuditableDocType()
        {
            
        }

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
}
