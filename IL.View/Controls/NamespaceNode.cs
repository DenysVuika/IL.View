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

using System.Linq;
using Mono.Cecil;

namespace IL.View.Controls
{
  public class NamespaceNode : TreeNode<NamespaceDefinition>
  {
    public override AssemblyDefinition DeclaringAssembly
    {
      get { return AssociatedObject.DeclaringAssembly; }
    }

    public NamespaceNode(AssemblyDefinition declaringAssembly, string name)
      : this(new NamespaceDefinition(declaringAssembly, name))
    {
    }

    public NamespaceNode(NamespaceDefinition component)
      : base(component)
    {
      DefaultStyleKey = typeof(NamespaceNode);
      Header = CreateHeaderCore(DefaultImages.AssemblyBrowser.Namespace, null, component.Name, true);
      DataProvider = DoLoadNamespaceTypes;
    }

    private static void DoLoadNamespaceTypes(TreeNode<NamespaceDefinition> view, NamespaceDefinition definition)
    {
      foreach (var type in definition.Types.OrderBy(t => t.Name))
      {
        var typeView = new TypeNode(type);
        view.Items.Add(typeView);
      }
    }
  }
}
