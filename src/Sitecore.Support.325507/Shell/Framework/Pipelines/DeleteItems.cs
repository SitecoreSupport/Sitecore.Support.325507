using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using System.Collections.Generic;

namespace Sitecore.Support.Shell.Framework.Pipelines
{
  public class DeleteItems : Sitecore.Shell.Framework.Pipelines.DeleteItems
  {
    public override void Confirm([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      if (args.IsPostBack)
      {
        if (args.Result == "yes")
        {
          args.Result = string.Empty;
        }
        else if (args.Result == "no")
        {
          Database database = GetDatabase(args);
          ListString list = new ListString(args.Parameters["items"], '|');

          if (list.Count == 1)
          {
            var language = args.Parameters["language"];
            Item item = GetItem(database, list, Language.Parse(language));

            if (Context.Item == null || item.ID != Context.Item.ID)
            {
              Context.ClientPage.ClientResponse.Eval("if(this.Content && this.Content.loadSearchedItem){this.Content.loadSearchedItem('" + item.ID + "')}");
            }
          }

          args.AbortPipeline();
        }
      }
      else
      {
        Database database = GetDatabase(args);
        var items = GetItems(args);

        Context.ClientPage.ClientResponse.Confirm(DeleteItems.GetConfirmDeleteItemsMessage(items, database, args.Parameters["language"]));

        args.WaitForPostBack();
      }
    }

    private static Database GetDatabase([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Database database = Factory.GetDatabase(args.Parameters["database"]);
      Assert.IsNotNull(database, typeof(Database), "Name: {0}", args.Parameters["database"]);
      return Assert.ResultNotNull(database);
    }

    private static List<Item> GetItems([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Database database = GetDatabase(args);
      var result = new List<Item>();
      var list = new ListString(args.Parameters["items"], '|');
      foreach (string id in list)
      {
        Item item = database.GetItem(id, Language.Parse(args.Parameters["language"]));
        if (item != null)
        {
          result.Add(item);
        }
      }
      return Assert.ResultNotNull(result);
    }

    private static Item GetItem([NotNull] Database database, [NotNull] ListString list, Language language)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(list, "list");
      Item item = database.GetItem(list[0], language);
      Assert.IsNotNull(item, typeof(Item), "ID: {0}", list[0]);
      return Assert.ResultNotNull(item);
    }
  }
}