using Microsoft.Extensions.DependencyInjection;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.XA.Foundation.Abstractions;
using Sitecore.XA.Foundation.Editing.Service;
using Sitecore.XA.Foundation.SitecoreExtensions.Utils;
using System;
using System.Collections;
using System.Linq;

namespace Sitecore.Support.XA.Foundation.LocalDatasources.Processors
{
  public class DeleteItems: Sitecore.Support.Shell.Framework.Pipelines.DeleteItems
  {
    private static Func<ClientPage, ArrayList> _commandsFieldGetter;

    protected IContext Context { get; } = Sitecore.DependencyInjection.ServiceLocator.ServiceProvider.GetService<IContext>();

    public void Reload(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      string message = args.Parameters["message"];
      if (message != null && message.StartsWith("item:deleted", StringComparison.OrdinalIgnoreCase))
      {
        SheerResponse.Eval("window.top.location.reload(true)");
      }
    }

    public override void Confirm(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      if (args.Result == "no" && Context.Item == null)
      {
        args.AbortPipeline();
      }
      else
      {
        base.Confirm(args);
      }
    }

    public override void Execute(ClientPipelineArgs args)
    {
      base.Execute(args);

      ArrayList commands = InternalInvoker.GetField(ref _commandsFieldGetter, "_commands")(Context.ClientPage);
      ClientCommand command = commands[commands.Count - 1] as ClientCommand;
      if (command != null && command.Command == "Eval" && command.InnerText == "this.Content.searchWithSameRoot()")
      {
        command.InnerHtml = "if (typeof this.Content !== \"undefined\" && typeof this.Content.searchWithSameRoot !== \"undefined\") this.Content.searchWithSameRoot()";
      }
    }

    public void CheckInboundLinks(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      if (string.IsNullOrEmpty(args.Result) && !HasLinks(args))
      {
        return;
      }
      CheckLinks(args);
    }

    protected virtual bool HasLinks(ClientPipelineArgs args)
    {
      ILinksService linksService = Sitecore.DependencyInjection.ServiceLocator.ServiceProvider.GetService<ILinksService>();
      ListString listString = new ListString(args.Parameters["items"], '|');
      Database database = Factory.GetDatabase(args.Parameters["database"]);

      if (database == null)
      {
        return false;
      }

      return listString.Select(id => database.GetItem(id)).Any(item => linksService.HasLinks(item));
    }
  }
}