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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Mono.Cecil;
using System.Windows.Input;
using IL.View.Model;

namespace IL.View.Controls
{
  public class AssemblyNode : TreeNode<AssemblyDefinition>
  {    
    public bool IsSilverlight
    {
      get { return AssociatedObject.IsSilverlight(); }
    }

    public AssemblyNode(AssemblyDefinition component)
      : base(component)
    {
      DefaultStyleKey = typeof(AssemblyNode);
      var header = CreateHeaderCore(DefaultImages.AssemblyBrowser.Assembly, null, component.Name.Name, true);            
      Header = header;
      InitializeNode();
    }    

    private void InitializeNode()
    {
      foreach (var module in AssociatedObject.Modules.OrderBy(m => m.Name))
      {
        var moduleView = new ModuleNode(module);

        // References
        if (module.HasAssemblyReferences)
        {
          var referencesView = new SimpleNode(DefaultImages.AssemblyBrowser.Reference, "References");
          foreach (var reference in module.AssemblyReferences)
            referencesView.Items.Add(new AssemblyReferenceNode(reference));

          moduleView.Items.Add(referencesView);
        }

        // Namespace cache
        var namespaceCache = new Dictionary<string, NamespaceNode>();

        // Types
        foreach (var type in module.Types.OrderBy(t => t.Namespace).ThenBy(t => t.Name))
        {
          NamespaceNode namespaceView;

          if (!namespaceCache.TryGetValue(type.Namespace, out namespaceView))
          {
            namespaceView = new NamespaceNode(type.Namespace);
            namespaceCache.Add(type.Namespace, namespaceView);
            moduleView.Items.Add(namespaceView);
          }

          namespaceView.AssociatedObject.Types.Add(type);
        }

        Items.Add(moduleView);
      }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.Key == Key.Delete)
      {
        ApplicationModel.Current.AssemblyCache.RemoveAssembly(AssociatedObject);
        e.Handled = true;
        return;
      }

      base.OnKeyDown(e);
    }
  }
}
