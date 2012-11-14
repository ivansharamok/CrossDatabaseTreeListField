using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using Sitecore.Web.UI.HtmlControls;

using Sitecore;
using Sitecore.Text;
using Sitecore.Web.UI;
using Sitecore.Web.UI.WebControls;
using Sitecore.Resources;
using Sitecore.Data.Items;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web;

namespace Custom.Shell.Applications.ContentEditor
{
	/// <summary>
	/// Summary description for TreeMultiList.
	/// </summary>
   [Designer(typeof(WebControlDesigner))]
   [ToolboxData("<{0}:TreeList runat=server></{0}:TreeList>")]
   public class TreeList  : Sitecore.Web.UI.HtmlControls.Control, IContentField
   {
      #region variables
      
      string itemID;
      bool readOnly;
      string source;
      Listbox listBox;

      #endregion

      #region property
      
      [Category("Data")]
      [Description("Comma separated list of template names. If this value is set, items based on these template will not be included in the menu.")]
      [Sitecore.Web.UI.WebControlProperty]
      public string ExcludeTemplatesForSelection
      {
         get
         {
            return GetViewStateString("ExcludeTemplatesForSelection");
         }
         set
         {
            SetViewStateString("ExcludeTemplatesForSelection", value);
         }
      }

      [Category("Data")]
      [Description("Comma separated list of template names. If this value is set, items based on these template will not be displayed in the tree.")]
      [Sitecore.Web.UI.WebControlProperty]
      public string ExcludeTemplatesForDisplay
      {
         get
         {
            return GetViewStateString("ExcludeTemplatesForDisplay");
         }
         set
         {
            SetViewStateString("ExcludeTemplatesForDisplay", value);
         }
      }

      [Category("Data")]
      [Description("Comma separated list of template names. If this value is set, only items based on these template can be included in the menu.")]
      [Sitecore.Web.UI.WebControlProperty]
      public string IncludeTemplatesForSelection
      {
         get
         {
            return GetViewStateString("IncludeTemplatesForSelection");
         }
         set
         {
            SetViewStateString("IncludeTemplatesForSelection", value);
         }
      }

      [Category("Data")]
      [Description("Comma separated list of template names. If this value is set, only items based on these template can be displayed in the menu.")]
      [Sitecore.Web.UI.WebControlProperty]
      public string IncludeTemplatesForDisplay
      {
         get
         {
            return GetViewStateString("IncludeTemplatesForDisplay");
         }
         set
         {
            SetViewStateString("IncludeTemplatesForDisplay", value);
         }
      }


      [Category("Data")]
      [Description("If set to Yes, allows the same item to be selected more than once")]
      [Sitecore.Web.UI.WebControlProperty]
      public bool AllowMultipleSelection
      {
         get
         {
            return GetViewStateBool("AllowMultipleSelection");
         }
         set
         {
            SetViewStateBool("AllowMultipleSelection", value);
         }
      }

      [Category("Data")]
      [Description("Source database for query statement. By default is context database.")]
      [Sitecore.Web.UI.WebControlProperty]
      public string SourceDatabaseName
      {
         get
         {
            return GetViewStateString("SourceDatabaseName");
         }
         set
         {
            if (string.IsNullOrEmpty(value))
            {
               value = Sitecore.Context.ContentDatabase.Name;
            }
            SetViewStateString("SourceDatabaseName", value);
         }
      }

      public string ItemID
      {
         get
         {
            return itemID;
         }
         set
         {
            itemID = value;
         }
      }
      public bool ReadOnly 
      {
         get
         {
            return readOnly;
         }
         set
         {
            readOnly = value;
         }         
      }

      public string Source 
      {
         get
         {
            return source;
         }
         set
         {
            source = value;
         }               
      }

      #endregion

      # region constructors

      public TreeList()
      {
         Class = "scContentControl";
         Background = "white";
         Activation = true;
         ReadOnly = false;                 
      }


      #endregion

      #region overrridable
      
      protected override void OnLoad(EventArgs args)
      {             
         if (!Sitecore.Context.ClientPage.IsEvent)
         {
            SetProperties();
            GridPanel gridPanel = new GridPanel();
            Controls.Add(gridPanel);
            gridPanel.Columns = 4;
            GetControlAttributes();
            
            foreach (string text2 in base.Attributes.Keys)
            {
               gridPanel.Attributes.Add(text2, base.Attributes[text2]);
            }

            gridPanel.Style["margin"] = "0px 0px 4px 0px";
            SetViewStateString("ID", ID);
            Sitecore.Web.UI.HtmlControls.Literal literal = new Sitecore.Web.UI.HtmlControls.Literal("All");
            literal.Class = ("scContentControlMultilistCaption");
   
            gridPanel.Controls.Add(literal);
            gridPanel.SetExtensibleProperty(literal, "Width", "50%");
            gridPanel.SetExtensibleProperty(literal, "Row.Height", "20px");
            
            LiteralControl spacer = new LiteralControl(Images.GetSpacer(24, 1));
            gridPanel.Controls.Add(spacer);
            gridPanel.SetExtensibleProperty(spacer, "Width", "24px");

            literal = new Sitecore.Web.UI.HtmlControls.Literal("Selected");
            literal.Class = "scContentControlMultilistCaption";

            gridPanel.Controls.Add(literal);
            gridPanel.SetExtensibleProperty(literal, "Width", "50%");
            
            spacer = new LiteralControl(Images.GetSpacer(24, 1));
            gridPanel.Controls.Add(spacer);
            gridPanel.SetExtensibleProperty(spacer, "Width", "24px");

            Scrollbox scrollbox = new Scrollbox();
            scrollbox.ID = GetUniqueID("S");

            gridPanel.Controls.Add(scrollbox);
            scrollbox.Style["border"] = "3px window-inset";
            gridPanel.SetExtensibleProperty(scrollbox, "rowspan", "2");

            DataTreeview dataTreeView = new DataTreeview();
            dataTreeView.ID = ID + "_all";
            scrollbox.Controls.Add(dataTreeView);
            dataTreeView.DblClick = ID + ".Add";

            ImageBuilder builderRight = new ImageBuilder();
            builderRight.Src = "Applications/16x16/nav_right_blue.png";
            builderRight.ID = ID + "_right";
            builderRight.Width = 0x10;
            builderRight.Height = 0x10;
            builderRight.Margin = "2";
            builderRight.OnClick = Sitecore.Context.ClientPage.GetClientEvent(ID + ".Add");

            ImageBuilder builderLeft = new ImageBuilder();
            builderLeft.Src = "Applications/16x16/nav_left_blue.png";
            builderLeft.ID = ID + "_left";
            builderLeft.Width = 0x10;
            builderLeft.Height = 0x10;
            builderLeft.Margin = "2";
            builderLeft.OnClick = Sitecore.Context.ClientPage.GetClientEvent(ID + ".Remove");
            
            LiteralControl literalControl = new LiteralControl(builderRight.ToString() + "<br/>" + builderLeft.ToString());
            gridPanel.Controls.Add(literalControl);
            gridPanel.SetExtensibleProperty(literalControl, "Width", "30");
            gridPanel.SetExtensibleProperty(literalControl, "Align", "center");
            gridPanel.SetExtensibleProperty(literalControl, "VAlign", "top");
            gridPanel.SetExtensibleProperty(literalControl, "rowspan", "2");

            Sitecore.Web.UI.HtmlControls.Listbox listbox = new Listbox();
				listBox = listbox;
            gridPanel.SetExtensibleProperty(listbox, "VAlign", "top");
            gridPanel.SetExtensibleProperty(listbox, "Height", "100%");

            gridPanel.Controls.Add(listbox);
            listbox.ID = ID + "_selected";
            listbox.DblClick = ID + ".Remove";
            listbox.Style["width"] = "100%";
            listbox.Size = "10";
            listbox.Attributes["onchange"] = "javascript:document.getElementById('" + this.ID + "_help').innerHTML=this.selectedIndex>=0?this.options[this.selectedIndex].innerHTML:''";
            listbox.Attributes["class"] = "scContentControlMultilistBox";

            dataTreeView.Disabled = ReadOnly;
            listbox.Disabled = ReadOnly;
            
            ImageBuilder builderUp = new ImageBuilder();
            builderUp.Src = "Applications/16x16/nav_up_blue.png";
            builderUp.ID = ID + "_up";
            builderUp.Width = 0x10;
            builderUp.Height = 0x10;
            builderUp.Margin = "2px";
            builderUp.OnClick = Sitecore.Context.ClientPage.GetClientEvent(ID + ".Up");

            ImageBuilder builderDown = new ImageBuilder();
            builderDown.Src = "Applications/16x16/nav_down_blue.png";
            builderDown.ID = ID + "_down";
            builderDown.Width = 0x10;
            builderDown.Height = 0x10;
            builderDown.Margin = "2";
            builderDown.OnClick = Sitecore.Context.ClientPage.GetClientEvent(ID + ".Down");
            
            literalControl = new LiteralControl(builderUp.ToString() + "<br/>" + builderDown.ToString());
            gridPanel.Controls.Add(literalControl);
            gridPanel.SetExtensibleProperty(literalControl, "Width", "30");
            gridPanel.SetExtensibleProperty(literalControl, "Align", "center");
            gridPanel.SetExtensibleProperty(literalControl, "VAlign", "top");
            gridPanel.SetExtensibleProperty(literalControl, "rowspan", "2");
            gridPanel.Controls.Add(new LiteralControl("<div style=\"border:1px solid #999999;font:8pt tahoma;padding:2px;margin:4px 0px 4px 0px;height:14px\" id=\"" + this.ID + "_help\"></div>"));
            
            DataContext treeContext = new DataContext();            
            gridPanel.Controls.Add(treeContext);
            treeContext.ID = GetUniqueID("D");
            treeContext.Filter = FormTemplateFilterForDisplay();
            dataTreeView.DataContext = treeContext.ID;
            treeContext.DataViewName = "Master";
            if (!string.IsNullOrEmpty(this.SourceDatabaseName))
            {
               treeContext.Parameters = "databasename=" + this.SourceDatabaseName;
            }
            treeContext.Root = DataSource;
            dataTreeView.EnableEvents();
            
            listbox.Size = "10";
            gridPanel.Fixed = true;
            dataTreeView.AutoUpdateDataContext = true;
            dataTreeView.ShowRoot = true;
            listbox.Attributes["class"] = "scContentControlMultilistBox";
            
            gridPanel.SetExtensibleProperty(scrollbox, "Height", "100%");
            RestoreState();
         }
         base.OnLoad(args);
      }
      

      #endregion

      # region message handlers
      
      protected void Add()
      {         
         if (!Disabled)
         {
            string id = GetViewStateString("ID");
            DataTreeview dataTreeView = FindControl(id + "_all") as DataTreeview;
            Listbox listbox = FindControl(id + "_selected") as Listbox;

            Item item = dataTreeView.GetSelectionItem();
            if(item == null)
            {
               Sitecore.Context.ClientPage.ClientResponse.Alert("Please choose item to select from tree.");
               return;
            }

            if(HasExcludeTemplateForSelection(item))
            {
               return;
            }
				if(IsDeniedMultipleSelection(item, listbox))
				{
               Sitecore.Context.ClientPage.ClientResponse.Alert("It is not allowed to select the same item twice.");
					return;
				}
            if(!HasIncludeTemplateForSelection(item))
            {
               return;
            }
            
            Sitecore.Context.ClientPage.ClientResponse.Eval("scForm.browser.getControl('" + id + "_selected').selectedIndex=-1");
            
            if(item == null)
            {
               return;
            }

            Sitecore.Web.UI.HtmlControls.ListItem listItem = new Sitecore.Web.UI.HtmlControls.ListItem();
            listItem.ID = GetUniqueID("L");
            Sitecore.Context.ClientPage.AddControl(listbox, listItem);
            listItem.Header = dataTreeView.Selected[0].Header;
            listItem.Value = listItem.ID + "|" + item.ID.ToString();

            ClientCommand insertCommand = Sitecore.Context.ClientPage.ClientResponse.Insert(id + "_selected", "append", listItem);
            insertCommand.Attributes["tag"] = "select";            
            
            
            Sitecore.Context.ClientPage.ClientResponse.Refresh(listbox);
            
            SetModified();
         }         
      }
      
      protected void Down()
      {
         if (!Disabled)
         {
            string id = GetViewStateString("ID");
            Listbox listbox = FindControl(id + "_selected") as Listbox;
            int i = -1;
				Sitecore.Web.UI.HtmlControls.ListItem item;
            for (int j = listbox.Controls.Count - 1; j >= 0; j--)
            {
               item = listbox.Controls[j] as Sitecore.Web.UI.HtmlControls.ListItem;
               if (!item.Selected)
               {
                  i = j - 1;
                  break;
               }
            }
            for (int k = i; k >= 0; k--)
            {
               item = listbox.Controls[k] as Sitecore.Web.UI.HtmlControls.ListItem;
               if (item.Selected)
               {
                  string[] textArray = new string[] { "scForm.browser.getControl('", item.ID, "').swapNode(scForm.browser.getControl('", item.ID, "').nextSibling)"} ;
                  Sitecore.Context.ClientPage.ClientResponse.Eval(string.Concat(textArray));
                  listbox.Controls.Remove(item);
                  listbox.Controls.AddAt(k + 1, item);
               }
            }
            SetModified();           
         }
      }

      protected void Remove()
      {         
         if (!Disabled)
         {
            string id = GetViewStateString("ID");            
            Sitecore.Web.UI.HtmlControls.Listbox listbox = FindControl(id + "_selected") as Listbox;
            Sitecore.Context.ClientPage.ClientResponse.Eval("scForm.browser.getControl('" + id + "_all').selectedIndex=-1");
            Sitecore.Web.UI.HtmlControls.ListItem[] itemArray = listbox.Selected;

            for (int num = 0; num < itemArray.Length; num++)
            {
               Sitecore.Web.UI.HtmlControls.ListItem li = itemArray[num];
               Sitecore.Context.ClientPage.ClientResponse.Remove(li.ID);
               listbox.Controls.Remove(li);
            }

            Sitecore.Context.ClientPage.ClientResponse.Refresh(listbox);
            SetModified();
         }
         
      }

      protected void Up()
      {
         if(Disabled)
         {
            return;
         }
         string id = GetViewStateString("ID");
         Sitecore.Web.UI.HtmlControls.Listbox listbox = FindControl(id + "_selected") as Listbox;
         
         Sitecore.Web.UI.HtmlControls.ListItem listItem = listbox.SelectedItem;

         if(listItem == null)
         {
            return;
         }
         
         int index = listbox.Controls.IndexOf(listItem);
         if(index == 0)
         {
            return;
         }

         string[] updateString = new string[] { "scForm.browser.getControl('", listItem.ID, "').swapNode(scForm.browser.getControl('", listItem.ID, "').previousSibling)" } ;
         Sitecore.Context.ClientPage.ClientResponse.Eval(string.Concat(updateString));
         
         listbox.Controls.Remove(listItem);
         listbox.Controls.AddAt(index-1, listItem);

         SetModified();
      }


      #endregion 

      # region IContentField realization

      public void SetValue(string text)
      {
         Value = text;
      }

      public string GetValue()
      {         
         ListString list = new ListString();
         string id = GetViewStateString("ID");
         Sitecore.Web.UI.HtmlControls.Listbox listbox = FindControl(id + "_selected") as Sitecore.Web.UI.HtmlControls.Listbox;
         Sitecore.Web.UI.HtmlControls.ListItem[] itemArray = listbox.Items;
         for (int i = 0; i < itemArray.Length; i++)
         {
            Sitecore.Web.UI.HtmlControls.ListItem item = itemArray[i];
            string[]pair  = item.Value.Split(new char[]{'|'});
            if(pair.Length <= 1)
            {
               continue;
            }
            list.Add(pair[1]);
         }
         return list.ToString();
      }

      
      # endregion
  
      #region private methods
      /// <summary>
      /// Can be used after OnLoad() is called.
      /// Fulfills parsing Of Value and restores Listbox state
      /// </summary>
      string FormTemplateFilterForDisplay()
      {
         if(IncludeTemplatesForDisplay == "" && ExcludeTemplatesForDisplay == "")
         {
            return "";
         }
         string includeList = "," + IncludeTemplatesForDisplay + ",";
         string exlcludeList = "," + ExcludeTemplatesForDisplay + ",";
         if(IncludeTemplatesForDisplay != "" && ExcludeTemplatesForDisplay != "")
         {
            string ret = string.Format("(contains('{0}', ',' + @@templateid + ',') or contains('{0}', ',' + @@templatekey + ',')) and " + 
               " not (contains('{1}', ',' + @@templateid + ',') or contains('{1}', ',' + @@templatekey + ','))",
               includeList.ToLower(), exlcludeList.ToLower());
            return ret;
         }
         else
         {
            if(IncludeTemplatesForDisplay != "")
            {
               string ret = string.Format("(contains('{0}', ',' + @@templateid + ',') or contains('{0}', ',' + @@templatekey + ','))", includeList.ToLower());
               return ret;            
            }
            else
            {
               string ret = string.Format("not (contains('{0}', ',' + @@templateid + ',') or contains('{0}', ',' + @@templatekey + ','))", exlcludeList.ToLower());
               return ret;               
            }
         }
      }

      void RestoreState()
      {
         string[] pair = Value.Split(new char[]{'|'});			

         if(pair.Length <= 0)
         {
            return;
         }
         
         for(int i = 0; i < pair.Length; i++)
         {
            string idItem = pair[i];            
            Item item = Sitecore.Configuration.Factory.GetDatabase(SourceDatabaseName).Items[idItem];
            if(item == null)
            {
               continue;
            }
            Sitecore.Web.UI.HtmlControls.ListItem listItem = new Sitecore.Web.UI.HtmlControls.ListItem();
            listItem.ID = GetUniqueID("I");
            listBox.Controls.Add(listItem);
            listItem.Value = listItem.ID + "|" + idItem;
            listItem.Header = item.Name; 
         }	
         Sitecore.Context.ClientPage.ClientResponse.Refresh(listBox);
      }

      protected void SetModified()
      {
         Sitecore.Context.ClientPage.Modified = true;
      }           

      bool HasExcludeTemplateForSelection(Item item)
      {
         if(item == null)
         {
            return true;//not to include
         }
         return HasItemTemplate(item, ExcludeTemplatesForSelection);
      }

      bool HasIncludeTemplateForSelection(Item item)
      {
         if(IncludeTemplatesForSelection == "")
         {
            return true;
         }
         return HasItemTemplate(item, IncludeTemplatesForSelection);
      }

      bool HasItemTemplate(Item item, string templateList)
      {
         if(item == null)
         {
            return false;
         }
         if(templateList == "")
         {
            return false;
         }
         string[] templates = templateList.Split(new char[]{','});
         ArrayList list = new ArrayList(templates.Length);
         
         for(int i = 0; i < templates.Length; i++)
         {
            list.Add(templates[i].Trim().ToLower());
         }

         if(list.Contains(item.TemplateName.Trim().ToLower()))
         {
            return true;
         }
         return false;      
      }

      bool IsDeniedMultipleSelection(Item item, Listbox listbox)
      {
         if(item == null)
         {
            return true;
         }
         if(AllowMultipleSelection)
         {
            return false;
         }
         foreach(Sitecore.Web.UI.HtmlControls.ListItem li in listbox.Controls)
         {
				string[] pair = li.Value.Split(new char[]{'|'});
				if(pair.Length < 2) continue;
            if(pair[1] == item.ID.ToString())
            {
               return true;
            }
         }
         return false;
      }

      void SetProperties()
      {
         if(!Source.Trim().ToLower().StartsWith("/"))
         {
            ExcludeTemplatesForSelection = StringUtil.ExtractParameter("ExcludeTemplatesForSelection", Source).Trim();
            IncludeTemplatesForSelection = StringUtil.ExtractParameter("IncludeTemplatesForSelection", Source).Trim();
            IncludeTemplatesForDisplay = StringUtil.ExtractParameter("IncludeTemplatesForDisplay", Source).Trim();
            ExcludeTemplatesForDisplay = StringUtil.ExtractParameter("ExcludeTemplatesForDisplay", Source).Trim();
            SourceDatabaseName = StringUtil.ExtractParameter("SourceDatabaseName", Source).Trim();

            string temp = StringUtil.ExtractParameter("AllowMultipleSelection", Source).Trim().ToLower();         
            if(temp.ToLower() == "yes" )
            {
               AllowMultipleSelection = true;
            }
            else
            {
               AllowMultipleSelection = false;
            }
         
            DataSource = StringUtil.ExtractParameter("DataSource", Source).Trim().ToLower();
         }
         else
         {
            DataSource = Source;
         }
      }
      
      #endregion
   }
}
