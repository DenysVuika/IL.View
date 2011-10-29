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
using System.Text;
using Mono.Cecil;

namespace IL.View.Controls
{
  public sealed class TypeNode : TreeNode<TypeDefinition>
  {
    public override AssemblyDefinition DeclaringAssembly
    {
      get { return AssociatedObject.Module.Assembly; }
    }

    public TypeNode(TypeDefinition component)
      : base(component)
    {
      DefaultStyleKey = typeof(TypeNode);
      InitializeNode();
    }

    private void InitializeNode()
    {
      var definition = AssociatedObject;
      var isPublic = definition.IsPublic || definition.IsNestedPublic;

      string icon = DefaultImages.AssemblyBrowser.Class;
      if (definition.IsValueType) icon = DefaultImages.AssemblyBrowser.Structure;
      if (definition.IsEnum) icon = DefaultImages.AssemblyBrowser.Enumeration;
      if (definition.IsInterface) icon = DefaultImages.AssemblyBrowser.Interface;
      if (definition.IsDelegate()) icon = DefaultImages.AssemblyBrowser.Delegate;

      var nameBuilder = new StringBuilder();

      if (definition.HasGenericParameters)
      {
        nameBuilder.Append(definition.Name.Substring(0, definition.Name.IndexOf('`')));

        nameBuilder.Append("<");
        for (int i = 0; i < definition.GenericParameters.Count; i++)
        {
          nameBuilder.Append(definition.GenericParameters[i].Name);
          if (i != definition.GenericParameters.Count - 1)
            nameBuilder.Append(", ");
        }
        nameBuilder.Append(">");
      }
      else
      {
        nameBuilder.Append(definition.Name);
      }

      Header = CreateHeaderCore(icon, OverlayIconProvider.GetOverlays(definition), nameBuilder.ToString(), isPublic);
      DataProvider = DoLoadType;
    }

    private static void DoLoadType(TreeNode<TypeDefinition> view, TypeDefinition definition)
    {
      // Base types
      if (definition.BaseType != null)
      {
        LoadBaseTypeView(view, definition.BaseType);
      }

      // Derived types      
      //typeView.Items.Add(TreeBuilder.CreateView(DefaultIcons.DerivedTypes, "Derived Types"));

      // Nested types          
      foreach (var nestedType in definition.NestedTypes.OrderBy(t => t.Name))
      {
        var nestedTypeView = new TypeNode(nestedType);
        view.Items.Add(nestedTypeView);
      }

      // Methods
      foreach (var method in definition.Methods.OrderBy(m => m.Name))
      {
        // skip processing Property members
        if (method.IsGetter || method.IsSetter) continue;
        // skipt processing Event members
        if (method.IsAddOn || method.IsRemoveOn) continue;
        view.Items.Add(new MethodNode(method));
      }

      // Properties
      foreach (var property in definition.Properties.OrderBy(p => p.Name))
      {
        var propertyView = new PropertyNode(property);

        // Getter
        if (property.GetMethod != null)
          propertyView.Items.Add(new MethodNode(property.GetMethod));

        // Setter
        if (property.SetMethod != null)
          propertyView.Items.Add(new MethodNode(property.SetMethod));

        view.Items.Add(propertyView);
      }

      // Events
      foreach (var eventdef in definition.Events.OrderBy(e => e.Name))
      {
        var eventView = new EventNode(eventdef);

        // Add
        if (eventdef.AddMethod != null)
          eventView.Items.Add(new MethodNode(eventdef.AddMethod));

        // Remove
        if (eventdef.RemoveMethod != null)
          eventView.Items.Add(new MethodNode(eventdef.RemoveMethod));

        view.Items.Add(eventView);
      }

      // Fields
      foreach (var field in definition.Fields.OrderBy(f => f.Name))
        view.Items.Add(new FieldNode(field));
    }

    private static void LoadBaseTypeView(TreeNode typeView, TypeReference typeRef)
    {
      var baseTypesView = new SimpleNode(DefaultImages.AssemblyBrowser.BaseTypes, "Base Types");

      var typeRefView = new SimpleNode(DefaultImages.AssemblyBrowser.Class, typeRef.Name);
      baseTypesView.Items.Add(typeRefView);

      // TODO: Introduce broader search, for now only within the same module
      TypeDefinition type = typeRef as TypeDefinition;
      if (type != null && type.BaseType != null)
      {
        LoadBaseTypeView(typeRefView, type.BaseType);
        if (type.HasInterfaces)
          foreach (var interfaceRef in type.Interfaces)
            typeRefView.Items.Add(new SimpleNode(DefaultImages.AssemblyBrowser.Interface, interfaceRef.Name));
      }

      typeView.Items.Add(baseTypesView);
    }
  }
}
