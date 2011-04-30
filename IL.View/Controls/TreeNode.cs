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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using IL.View.Decompiler;
using IL.View.Model;
using Mono.Cecil;

namespace IL.View.Controls
{
  public abstract class TreeNode : TreeViewItem
  {
    public abstract AssemblyDefinition DeclaringAssembly { get; }
    public virtual bool IsLoaded { get; private set; }
    public object Component { get; private set; }

    [Import]
    public ContextMenuBuilder ContextMenuBuilder { get; set; }

    [Import]
    public DecompilerManager DecompilerManager { get; set; }

    protected TreeNode(object component)
    {
      CompositionInitializer.SatisfyImports(this);
      Component = component;
      Tag = component;
    }

    public virtual void LoadData()
    {
      if (IsLoaded) return;

      IsLoaded = true;
    }

    protected override void OnExpanded(RoutedEventArgs e)
    {
      LoadData();
      base.OnExpanded(e);
    }

    protected virtual FrameworkElement CreateHeaderCore(string icon, IEnumerable<string> overlays, string header, bool isPublic)
    {
      var panel = new StackPanel { Orientation = Orientation.Horizontal };

      var icons = new Grid();
      var mainIcon = DefaultImages.AssemblyBrowser.GetDefaultImage(icon);
      mainIcon.Width = 16;
      mainIcon.Height = 16;

      icons.Children.Add(mainIcon);

      if (overlays != null)
      {
        foreach (var overlay in overlays)
          icons.Children.Add(DefaultImages.AssemblyBrowser.GetDefaultImage(overlay));
      }

      panel.Children.Add(icons);

      var caption = new TextBlock
      {
        Text = header,
        Margin = new Thickness(5, 0, 0, 0),
        FontWeight = FontWeights.Normal
      };

      if (!isPublic) caption.FontStyle = FontStyles.Italic;

      panel.Children.Add(caption);

      panel.MouseRightButtonDown += (s, e) => e.Handled = true;
      panel.MouseRightButtonUp += (s, e) =>
        {
          IsSelected = true;
          var menu = CreateContextMenu();
          if (menu == null || menu.Items.Count == 0) return;

          GeneralTransform gt = panel.TransformToVisual(Application.Current.RootVisual as UIElement);
          Point offset = gt.Transform(new Point(0, 0));
          menu.HorizontalOffset = offset.X + e.GetPosition(panel).X;
          menu.VerticalOffset = offset.Y + e.GetPosition(panel).Y;
          menu.IsOpen = true;
        };

      return panel;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.Key == Key.Space)
      {
        DecompilerManager.RequestCodeDisassembly(DeclaringAssembly, Component);
        e.Handled = true;
        return;
      }

      base.OnKeyDown(e);
    }

    protected ContextMenu CreateContextMenu()
    {
      ContextMenu menu = new ContextMenu();

      if (ContextMenuBuilder != null)
      {
        var items = ContextMenuBuilder.GetMenuItems(Component);
        foreach (var item in items)
          menu.Items.Add(item);
      }
      return menu;
    }
  }


  public abstract class TreeNode<T> : TreeNode where T : class
  {
    private readonly TreeViewItem _stubItem = new TreeViewItem { Header = "Loading data..." };

    public T AssociatedObject { get; private set; }

    private Action<TreeNode<T>, T> _dataProvider;
    protected Action<TreeNode<T>, T> DataProvider
    {
      get { return _dataProvider; }
      set
      {
        if (_dataProvider == value) return;
        _dataProvider = value;
        if (_dataProvider != null)
        {
          Items.Clear();
          Items.Add(_stubItem);
        }
      }
    }

    protected TreeNode(T component = null, Action<TreeNode<T>, T> loadHandler = null)
      : base(component)
    {
      AssociatedObject = component;
      DataProvider = loadHandler;
    }

    public override void LoadData()
    {
      if (IsLoaded) return;

      if (DataProvider != null)
      {
        Items.Clear();
        DataProvider(this, AssociatedObject);
      }
      else if (Items.Contains(_stubItem))
        Items.Remove(_stubItem);

      base.LoadData();
    }

    protected virtual IEnumerable<string> GetOverlays(T component)
    {
      return Enumerable.Empty<string>();
    }
  }
}
