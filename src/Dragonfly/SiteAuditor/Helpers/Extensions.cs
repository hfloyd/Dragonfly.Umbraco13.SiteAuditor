namespace Dragonfly.SiteAuditor.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Dragonfly.SiteAuditor.Models;
    using Umbraco.Cms.Core.Models;
    
    public static class Extensions
    {
        #region IContent
        public static bool HasPropertyValue(this IContent Content, string PropertyAlias)
        {
            var hasProp = Content.HasProperty(PropertyAlias);

            if (!hasProp)
            {
                return false;
            }

            var valObject = Content.GetValue(PropertyAlias);
            if (valObject == null)
            {
                return false;
            }

            var valString = Content.GetValue<string>(PropertyAlias);
            if (valString == "")
            {
                return false;
            }

            return true;
        }
        
        #endregion
    }
}
