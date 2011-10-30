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

using System.Resources;
using System.Threading;
using System.Windows;
using IL.View.Model;
using Mono.Cecil;

namespace IL.View.Controls
{
  public sealed class ResourceNode : TreeNode<Resource>
  {
    private readonly AssemblyDefinition _declaringAssembly;
    private readonly AssemblyStream _assemblySource;
    
    public override AssemblyDefinition DeclaringAssembly
    {
      get { return _declaringAssembly; }
    }

    public ResourceNode(AssemblyDefinition declaringAssembly, AssemblyStream assemblySource, Resource resource)
      : base(resource)
    {
      DefaultStyleKey = typeof(ResourceNode);

      _declaringAssembly = declaringAssembly;
      _assemblySource = assemblySource;

      Header = CreateHeaderCore(DefaultImages.AssemblyBrowser.FileResource, null, resource.Name, true);
      DataProvider = DoLoadType;
    }

    // TODO: Ask/warn user whether he wants to load assembly! Introduce settings option to enable/disable warning, add "Don't remind" on warning dialog
    private void DoLoadType(TreeNode<Resource> view, Resource definition)
    {
      var assembly = new AssemblyPart().Load(_assemblySource.OpenRead());
      if (assembly == null) return;

      //var resourceEntryName = assembly.GetManifestResourceNames().First();
      //var rm = new ResourceManager(resourceEntryName.Replace(".resources", ""), assembly);
      var rm = new ResourceManager(definition.Name.Replace(".resources", ""), assembly);

      //Seems like some issue here, but without getting any stream next statement doesn't work....
      var dummy = rm.GetStream("app.xaml");

      var rs = rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, false, true);
      if (rs == null) return;
      
      var enumerator = rs.GetEnumerator();
      while (enumerator.MoveNext())
      {
        //view.Items.Add(new SimpleNode(DefaultImages.AssemblyBrowser.FileMisc, (string)enumerator.Key));
        var resourceName = (string)enumerator.Key;
        var resourceStream = new EmbeddedResourceStream(rs, resourceName);
        view.Items.Add(new FileNode(resourceStream, resourceName));
      }
    }
  }
}
