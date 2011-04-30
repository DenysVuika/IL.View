/*
 * The MIT License
 * 
 * Copyright © 2011, Denys Vuika
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * */

using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Hosting;

namespace IL.View.Repository
{
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
  public class AssemblyRepository : IAssemblyRepository
  {
    public bool VerifyAssembly(string architecture, string runtime, string name, string version, string token, string specificVersion)
    {
      bool isSpecificVersion = false;
      if (!bool.TryParse(specificVersion, out isSpecificVersion)) isSpecificVersion = false;
      string path;
      bool result = TryGetAssemblyPath(architecture, runtime, name, version, token, isSpecificVersion, out path);
      WebOperationContext.Current.OutgoingResponse.StatusCode = result ? HttpStatusCode.OK : HttpStatusCode.NotFound;
      return result;
    }

    private static bool TryGetAssemblyPath(string architecture, string runtime, string name, string version, string token, bool specificVersion, out string path)
    {
      string assemblyFolder = string.Format("{0}Repository\\{1}\\{2}\\{3}",
        HostingEnvironment.ApplicationPhysicalPath,
        architecture,
        runtime,
        name);

      path = null;

      if (!Directory.Exists(assemblyFolder)) return false;

      string versionFolder = Path.Combine(assemblyFolder, string.Format("{0}__{1}", version, token));
      string directPath = Path.Combine(versionFolder, string.Format("{0}.dll", name));
      if (File.Exists(directPath))
      {
        path = directPath;
        return true;
      }

      if (!specificVersion)
      {
        string lastestVersionFolder = Directory.GetDirectories(assemblyFolder).Max();
        if (string.Compare(lastestVersionFolder, versionFolder) > 0)
        {
          string latestAssemblyPath = Path.Combine(lastestVersionFolder, string.Format("{0}.dll", name));
          if (File.Exists(latestAssemblyPath))
          {
            path = latestAssemblyPath;
            return true;
          }
        }
      }

      return false;
    }


    public Stream GetAssembly(string architecture, string runtime, string name, string version, string token, string specificVersion)
    {
      bool isSpecificVersion = false;
      if (!bool.TryParse(specificVersion, out isSpecificVersion)) isSpecificVersion = false;

      string path;
      if (!TryGetAssemblyPath(architecture, runtime, name, version, token, isSpecificVersion, out path))
      {
        WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound(string.Format("Assembly '{0}' not found.", name));
        return null;
      }

      WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
      var stream = File.OpenRead(path);
      return stream;
    }
  }
}
