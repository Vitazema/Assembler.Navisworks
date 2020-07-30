using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.Api.Controllers
{
  public class DataManagementController : ControllerBase
  {
    /// <summary>
    /// Credentials on this request
    /// </summary>
    private Credentials Credentials { get; set; }

    /// <summary>
    /// GET TreeNode passing the ID
    /// </summary>
    [HttpGet]
    [Route("api/forge/datamanagement")]
    public async Task<IList<JsTreeNode>> GetTreeNodeAsync(string id)
    {
      Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
      if (Credentials == null) { return null; }

      IList<JsTreeNode> nodes = new List<JsTreeNode>();

      if (id == "#") // root
        return await TreeNode.GetHubsAsync(Credentials);
      else
      {
        string[] idParams = id.Split('/');
        string resource = idParams[idParams.Length - 2];
        switch (resource)
        {
          case "hubs": // hubs node selected/expanded, show projects
            return await TreeNode.GetProjectsAsync(id, Credentials);
          case "projects": // projects node selected/expanded, show root folder contents
            return await TreeNode.GetProjectContents(id, Credentials);
          case "folders": // folders node selected/expanded, show folder contents
            return await TreeNode.GetFolderContents(id, Credentials);
          case "items":
            return await TreeNode.GetItemVersions(id, Credentials);
        }
      }

      return nodes;
    }
  }
}
