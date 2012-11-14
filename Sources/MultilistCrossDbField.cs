using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore;

namespace Custom.Data.Fields
{
   public class MultilistCrossDbField : DelimitedField
   {
      private Database _database;

      public MultilistCrossDbField(Field innerField)
         : base(innerField, '|')
      {
         _database = base.InnerField.Database;
         string dbName = StringUtil.ExtractParameter("SourceDatabaseName", base.InnerField.Source).Trim();
         if (!string.IsNullOrEmpty(dbName))
         {            
            _database = Sitecore.Configuration.Factory.GetDatabase(dbName);
         }         
      }

      public override void ValidateLinks(Sitecore.Links.LinksValidationResult result)
      {
         foreach (string str in this.Items)
         {
            if (ID.IsID(str))
            {
               ID id = ID.Parse(str);
               if (!ItemUtil.IsNull(id) && !id.IsNull)
               {
                  Item targetItem = SourceDatabase.Items[id];
                  if (targetItem != null)
                  {
                     result.AddValidLink(targetItem, base.InnerField.Value);
                  }
                  else
                  {
                     result.AddBrokenLink(base.InnerField.Value);
                  }
               }
            }
         }
      }

      public Database SourceDatabase
      {
         get
         {
            return _database;
         }
      }
   }
}
