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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using IL.View.Controls;
using IL.View.Controls.CodeView;
using IL.View.Decompiler;
using IL.View.Model;
using IL.View.Services;
using IL.View.Views;
using Mono.Cecil;

namespace IL.View
{
  public partial class Home : Page
  {
    private static readonly string[] KnownExtensions = new[] { ".dll", ".exe", ".xap", ".nupkg" };

    private readonly Queue<FileInfo> _pendingDownloads = new Queue<FileInfo>();
    private readonly BusyIndicatorContext _busyContext = new BusyIndicatorContext();

    [Obsolete("Temporary solution until Mono.Cecil enhancements are applied")]
    // TODO: this approach is not thread-safe!
    private DecompileTask _decompileTask;

    [Obsolete("Temporary solution until Mono.Cecil enhancements are applied")]
    private void SetDecompileTask(DecompileTask task)
    {
      _decompileTask = task;
    }

    [Import]
    public DecompilerManager DecompilerManager { get; set; }

    [Import]
    public IContentViewerService ContentViewerService { get; set; }

    private static BaseAssemblyResolver AssemblyResolver
    {
      get { return (GlobalAssemblyResolver.Instance as BaseAssemblyResolver); }
    }

    public Home()
    {
      InitializeComponent();
      CompositionInitializer.SatisfyImports(this);
      LayoutRoot.DataContext = ApplicationModel.Current;
    }

    // Executes when the user navigates to this page.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      // enable "Code Uri" usage
      if (!UriParser.IsKnownScheme("code"))
        UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "code", -1);

      // Register a handler for Drop event
      LayoutRoot.Drop += LayoutRoot_Drop;
      DownloadIndicator.DataContext = _busyContext;

      ApplicationModel.Current.AssemblyCache.AssemblyAdded += AssemblyCache_AssemblyAdded;
      ApplicationModel.Current.AssemblyCache.AssemblyRemoved += AssemblyCache_AssemblyRemoved;
      DecompilerManager.CodeDisassemblyRequested += OnCodeDisassemblyRequested;
      ApplicationModel.Current.CurrentLanguageChanged += OnCurrentLanguageChanged;
      
      ContentViewerService.SourceCodeViewRequested += OnSourceCodeViewRequested;
      ContentViewerService.ImageViewRequested += OnImageViewRequested;
    }
    
    // Executes when the user navigates from this page.
    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      LayoutRoot.Drop -= LayoutRoot_Drop;
      ApplicationModel.Current.AssemblyCache.AssemblyAdded -= AssemblyCache_AssemblyAdded;
      ApplicationModel.Current.AssemblyCache.AssemblyRemoved -= AssemblyCache_AssemblyRemoved;
      DecompilerManager.CodeDisassemblyRequested -= OnCodeDisassemblyRequested;
      ApplicationModel.Current.CurrentLanguageChanged -= OnCurrentLanguageChanged;
      
      ContentViewerService.SourceCodeViewRequested -= OnSourceCodeViewRequested;
      ContentViewerService.ImageViewRequested -= OnImageViewRequested;
      
      base.OnNavigatingFrom(e);
    }

    private void AssemblyCache_AssemblyAdded(object sender, AssemblyDefinitionEventArgs e)
    {
      LoadAssembly(e.Definition, e.Source);
    }

    private void AssemblyCache_AssemblyRemoved(object sender, AssemblyDefinitionEventArgs e)
    {
      // TODO: Physically remove cached assembly
      UnloadAssemblyView(e.Definition);
    }

    private void OnCodeDisassemblyRequested(object sender, DecompileRequestEventArgs e)
    {
      if (_decompileTask != null)
      {
        // new thread will corrupt previous task info!
        if (Debugger.IsAttached) Debugger.Break();
      }

      DisassembleProgress.IsBusy = true;
      var task = new DecompileTask(SourceView, e.CallingAssembly, e.Target);
      var thread = new Thread(DoShowCode);
      thread.Start(task);
    }

    private void OnCurrentLanguageChanged(object sender, EventArgs e)
    {
      if (SourceView.CurrentTask != null)
        DecompilerManager.RequestCodeDisassembly(SourceView.CurrentTask.CallingAssembly, SourceView.CurrentTask.Source);
    }

    // Queue the FileInfo objects representing dropped files
    private void LayoutRoot_Drop(object sender, DragEventArgs e)
    {
      var files = e.Data.GetData(DataFormats.FileDrop) as FileInfo[];
      if (files == null || files.Length == 0) return;

      foreach (var fi in files)
      {
        var ext = Path.GetExtension(fi.Name);
        if (KnownExtensions.Contains(ext))
          _pendingDownloads.Enqueue(fi);
      }

      DownloadPendingAssemblies();
    }

    private void DownloadPendingAssemblies()
    {
      if (_pendingDownloads.Count == 0) return;

      _busyContext.IsBusy = true;

      ThreadPool.QueueUserWorkItem(o =>
      {
        while (_pendingDownloads.Count > 0)
        {
          var fileInfo = _pendingDownloads.Dequeue();

          Dispatcher.BeginInvoke(() => { _busyContext.ItemLabel = fileInfo.Name; });
                    
          try
          {
            string extension = Path.GetExtension(fileInfo.Name);

            if (extension == ".xap")
            {
              Dispatcher.BeginInvoke(() => LoadZiPackage(DefaultImages.AssemblyBrowser.XapPackage, fileInfo));
            }
            else if (extension == ".nupkg")
            {
              Dispatcher.BeginInvoke(() => LoadZiPackage(DefaultImages.AssemblyBrowser.NugetPackage, fileInfo));
            }
            else
            {
              var definition = AssemblyDefinition.ReadAssembly(fileInfo.OpenRead());

              string assemblyPath = definition.IsSilverlight()
                ? StorageService.CacheSilverlightAssembly(fileInfo.Name, fileInfo.OpenRead())
                : StorageService.CacheNetAssembly(fileInfo.Name, fileInfo.OpenRead());

              Dispatcher.BeginInvoke(() =>
              {
                var assemblyStream = new AssemblyFileStream(fileInfo);
                ApplicationModel.Current.AssemblyCache.LoadAssembly(assemblyStream, definition, false);
                LoadOrReplaceAssembly(definition, assemblyStream);
              });
            }
          }
          catch (Exception ex)
          {
            Debug.WriteLine(ex.Message);
            Dispatcher.BeginInvoke(() => new ErrorWindow(ex).Show());
          }
        }

        Dispatcher.BeginInvoke(() => _busyContext.IsBusy = false);
      });
    }

    private void LoadZiPackage(string icon, FileInfo fileInfo)
    {
      var node = new ZipFileNode(icon, new AssemblyPackageStream(fileInfo));
      SilverlightAssemblies.Items.Add(node);
    }

    private void LoadAssembly(AssemblyDefinition definition, AssemblyStream source)
    {
      var node = new AssemblyNode(definition, source);

      if (definition.IsSilverlight())
        SilverlightAssemblies.Items.Add(node);
      else
        NetAssemblies.Items.Add(node);
    }

    private void UnloadAssemblyView(AssemblyDefinition definition)
    {
      var root = definition.IsSilverlight() ? SilverlightAssemblies : NetAssemblies;
      var view = root.Items.OfType<TreeNode>().FirstOrDefault(item => item.Component == definition);
      root.Items.Remove(view);
    }

    private void LoadOrReplaceAssembly(AssemblyDefinition definition, AssemblyStream source)
    {
      var assemblyView = new AssemblyNode(definition, source);
      AddOrReplaceAssemblyView(definition.IsSilverlight() ? SilverlightAssemblies : NetAssemblies, assemblyView);
    }

    // TODO: Maybe check against AssemblyChache instead of visual tree?
    private void AddOrReplaceAssemblyView(TreeView root, TreeViewItem assemblyView)
    {
      var definition = assemblyView.Tag as AssemblyDefinition;

      bool replaced = false;

      foreach (var item in root.Items.OfType<TreeViewItem>().ToArray())
      {
        var existing = item.Tag as AssemblyDefinition;
        if (existing == null) continue;

        if (existing.FullName.Equals(definition.FullName, StringComparison.OrdinalIgnoreCase))
        {
          var index = root.Items.IndexOf(item);
          root.Items.Insert(index, assemblyView);
          root.Items.Remove(item);
          replaced = true;
          break;
        }
      }

      if (replaced) return;
      root.Items.Add(assemblyView);
    }

    /*
    private void OnLoadAssemblyClick(object sender, RoutedEventArgs e)
    {
      var dlg = new OpenFileDialog();
      if (dlg.ShowDialog() == true)
      {
        Dispatcher.BeginInvoke(() =>
        {
          try
          {
            var definition = AssemblyDefinition.ReadAssembly(dlg.File.OpenRead());

            string assemblyPath = definition.IsSilverlight() 
              ? StorageService.CacheSilverlightAssembly(dlg.File.Name, dlg.File.OpenRead()) 
              : StorageService.CacheNetAssembly(dlg.File.Name, dlg.File.OpenRead());

            ApplicationModel.Current.AssemblyCache.AddAssembly(assemblyPath, definition);

            LoadOrReplaceAssembly(definition);
          }
          catch (Exception ex)
          {
            Debug.WriteLine(ex.Message);
          }
        });
      }
    }
    */

    private void OnItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      //ShowCode(e.NewValue);
      //ShowCodeUri(e.NewValue);

      if (AssemblyBrowserSettings.Current.AutoDisassemble)
      {
        var node = e.NewValue as TreeNode;
        if (node == null) return;
        Dispatcher.BeginInvoke(() => DecompilerManager.RequestCodeDisassembly(node.DeclaringAssembly, node.Component));
      }      
    }

    //private void ShowCodeUri(object source)
    //{
    //  var item = source as EntityView;
    //}

    private void DoShowCode(object parameter)
    {
      if (_decompileTask != null)
      {
        // trying access already running task!
        if (Debugger.IsAttached) Debugger.Break();
      }

      var task = parameter as DecompileTask;
      SetDecompileTask(task);

      AssemblyResolver.ResolveFailure += TryResolveAssembly;
      string code = GetCode(task.Source);
      AssemblyResolver.ResolveFailure -= TryResolveAssembly;
      Dispatcher.BeginInvoke(() =>
      {
        // TODO: Needs redesign
        if (ApplicationModel.Current.CurrentLanguage == LanguageInfoList.CSharp)
          SourceView.SourceLanguage = SourceLanguageType.CSharp;
        else
          SourceView.SourceLanguage = SourceLanguageType.IL;

        task.View.SourceCode = code;        
        DisassembleProgress.IsBusy = false;
        SetDecompileTask(null);
      });
    }

    private static AssemblyDefinition TryResolveHigherVersionAssembly(AssemblyDefinition callingAssembly, AssemblyNameReference reference)
    {
      foreach (var assembly in ApplicationModel.Current.AssemblyCache.Assemblies)
      {
        var name = assembly.Name;

        if (!name.Name.Equals(reference.Name, StringComparison.OrdinalIgnoreCase)) continue;
        if (!name.Culture.Equals(reference.Culture, StringComparison.OrdinalIgnoreCase)) continue;
        if (name.HasPublicKey == reference.HasPublicKey)
        {
          if (name.PublicKey != null && !name.PublicKey.SequenceEqual(reference.PublicKey)) continue;
        }

        if (name.PublicKeyToken == null && reference.PublicKeyToken == null) return assembly;
        if (name.PublicKeyToken.Length == 0 && reference.PublicKeyToken.Length == 0) return assembly;
        if (name.PublicKeyToken.SequenceEqual(reference.PublicKeyToken)) return assembly;
      }
      return null;
    }

    private AssemblyDefinition TryResolveAssembly(object sender, AssemblyNameReference reference)
    {      
      if (_decompileTask == null)
      {
        // trying to access missing task/calling assembly
        if (Debugger.IsAttached) Debugger.Break();
      }

      Debug.WriteLine("Trying to resolve assembly: '{0}'", reference.FullName);
      // try to resolve assembly from cache
      var definition = ApplicationModel.Current.AssemblyCache.Assemblies.FirstOrDefault(a => a.FullName == reference.FullName);

      // try to resolve assembly with higher version from cache (often used for "mscorlib")
      if (definition == null)
        definition = TryResolveHigherVersionAssembly(_decompileTask.CallingAssembly, reference);

      // try to resolve assembly from user-defined reference paths
      if (definition == null)
        definition = FileService.FindExternalAssembly(reference, Dispatcher);

      // ask user to resolve assembly manually
      if (definition == null)
      {
        var requestProcessed = new AutoResetEvent(false);

        Dispatcher.BeginInvoke(() =>
        {
          var dialog = new AssemblyFileSelector(_decompileTask.CallingAssembly, reference)
          {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
          };
          dialog.Closed += (s, e) =>
          {
            definition = dialog.Definition;
            if (definition == null)
            {
              if (Debugger.IsAttached) Debugger.Break();
              // TODO: decide what to do here...
              // The corresponding IL generator should catch an exception and skip writing the code for failed section
            }
            requestProcessed.Set();
          };
          dialog.Show();
        });

        requestProcessed.WaitOne();
      }

      return definition;
    }

    // TODO: needs rewriting
    private static string GetCode(object source)
    {
      if (ApplicationModel.Current.CurrentLanguage == LanguageInfoList.CSharp)
      {       
        return GetCSharpCode(source);
      }      
      
      return GetILCode(source);
    }

    private static string GetCSharpCode(object source)
    {
      var output = new ICSharpCode.Decompiler.PlainTextOutput();
      var language = Languages.GetLanguage("C#");
      var shortDecompile = new DecompilationOptions { FullDecompilation = false };
      var fullDecompile = new DecompilationOptions { FullDecompilation = true };

      var assembly = source as AssemblyDefinition;
      if (assembly != null)
      {
        language.DecompileAssembly(assembly, "", output, shortDecompile);
        return output.ToString();
      }

      var ns = source as NamespaceDefinition;
      if (ns != null)
      {
        //writer.WriteNamespace(ns, output, null);
        //return output.ToString();
        return "NOT IMPLEMENTED YET";
      }

      var module = source as ModuleDefinition;
      if (module != null)
      {
        //writer.WriteModule(module, output, null);
        //return output.ToString();
        return "NOT IMPLEMENTED YET";
      }

      var type = source as TypeDefinition;
      if (type != null)
      {
        language.DecompileType(type, output, fullDecompile);        
        return output.ToString();
      }

      var method = source as MethodDefinition;
      if (method != null)
      {
        language.DecompileMethod(method, output, fullDecompile);
        return output.ToString();
      }

      var property = source as PropertyDefinition;
      if (property != null)
      {
        language.DecompileProperty(property, output, fullDecompile);
        return output.ToString();
      }

      var field = source as FieldDefinition;
      if (field != null)
      {
        language.DecompileField(field, output, fullDecompile);
        return output.ToString();
      }

      var @event = source as EventDefinition;
      if (@event != null)
      {
        language.DecompileEvent(@event, output, fullDecompile);
        return output.ToString();
      }

      return "NOT IMPLEMENTED YET";
    }

    // TODO: needs rewriting
    private static string GetILCode(object source)
    {
      var output = new PlainTextCodeOutput();
      var writer = new ILCodeWriter();
     
      var assembly = source as AssemblyDefinition;
      if (assembly != null)
      {
        writer.WriteAssembly(assembly, output, null);
        return output.ToString();        
      }

      var ns = source as NamespaceDefinition;
      if (ns != null)
      {
        writer.WriteNamespace(ns, output, null);
        return output.ToString();
      }

      var module = source as ModuleDefinition;
      if (module != null)
      {
        writer.WriteModule(module, output, null);
        return output.ToString();
      }

      var type = source as TypeDefinition;
      if (type != null)
      {
        writer.WriteType(type, output, new DecompilerOptions { FullDecompilation = true });
        return output.ToString();
      }

      var method = source as MethodDefinition;
      if (method != null)
      {
        writer.WriteMethod(method, output, new DecompilerOptions { FullDecompilation = true });
        return output.ToString();
      }

      var property = source as PropertyDefinition;
      if (property != null)
      {
        writer.WriteProperty(property, output, null);
        return output.ToString();
      }

      var field = source as FieldDefinition;
      if (field != null)
      {
        writer.WriteField(field, output, null);
        return output.ToString();
      }

      var @event = source as EventDefinition;
      if (@event != null)
      {
        writer.WriteEvent(@event, output, null);
        return output.ToString();
      }

      return string.Empty;
    }
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      //LoadingAssembliesIndicator.IsBusy = true;

      ThreadPool.QueueUserWorkItem(state => Dispatcher.BeginInvoke(() =>
      {
        //foreach (var assemblyPath in StorageService.EnumerateAssemblyCache())
        //{
        //  using (var stream = StorageService.OpenCachedAssembly(assemblyPath))
        //  {
        //    try
        //    {
        //      var definition = AssemblyDefinition.ReadAssembly(stream);
        //      ApplicationModel.Current.AssemblyCache.AddAssembly(assemblyPath, definition);
        //    }
        //    catch (Exception ex)
        //    {
        //      Debug.WriteLine(ex.Message);
        //    }
        //  }
        //}

        foreach (var fileInfo in StorageService.EnumerateFiles())
        {
          ApplicationModel.Current.AssemblyCache.LoadAssembly(new AssemblyFileStream(fileInfo));
        }

        //LoadingAssembliesIndicator.IsBusy = false;
      }));
    }

    [Obsolete("Temporary")]
    private void SelectMethod(MethodDefinition definition)
    {
      /*
      var runtimeView = definition.DeclaringType.Module.Assembly.IsSilverlight() ? runtimeSilverlightView : runtimeNetView;
      var assemblyView = runtimeView.Items.OfType<TreeViewItem>()
        .FirstOrDefault(item => item.Tag is AssemblyDefinition && (item.Tag as AssemblyDefinition).FullName == definition.DeclaringType.Module.Assembly.FullName);
      var moduleView = assemblyView.Items[0] as TreeViewItem;
      var namespaceView = moduleView.Items.OfType<TreeViewItem>()
        .FirstOrDefault(item => item.Tag != null && item.Tag.Equals(definition.DeclaringType.Namespace));
      var typeView = namespaceView.Items.OfType<EntityView>()
        .FirstOrDefault(item => item.Component is TypeDefinition && (item.Component as TypeDefinition).FullName == definition.DeclaringType.FullName);

      if (!typeView.IsLoaded)
        typeView.LoadData();

      var methodView = typeView.Items.OfType<TreeViewItem>()
        .FirstOrDefault(item => item.Tag is MethodDefinition && (item.Tag as MethodDefinition).FullName == definition.FullName);

      AssembliesTree.SetSelectedContainer(methodView);      
      AssembliesTree.ExpandPath(runtimeView, assemblyView, moduleView, namespaceView, typeView, methodView);
      ShowCode(methodView);        
      */
    }

    private void OnNavigateCodeUriClick(object sender, RoutedEventArgs e)
    {
      //string codeUri = "code://ClassLibrary1:1.0.0.0/ClassLibrary1.GenericClass3<,,>/GenericMethod2<>(<!!0>,String)";
      if (string.IsNullOrWhiteSpace(CodeUri.Text)) return;
      var method = ApplicationModel.Current.FindMethodDefinition(new Uri(CodeUri.Text, UriKind.Absolute));
      if (method != null) SelectMethod(method);
    }

    private void OnSourceCodeViewRequested(object sender, SourceCodeEventArgs e)
    {
      var sourceCode = e.SourceCode;

      switch (e.SourceLanguage)
      {
        case SourceLanguageType.Xml:
        case SourceLanguageType.Xaml:
          sourceCode = FormattingUtils.FormatXml(e.SourceCode);
          break;
      }

      var view = new CodeTextBox
      {
        SourceLanguage = e.SourceLanguage,
        SourceCode = sourceCode
      };

      DisplayContent(e.SourceName, view);
    }

    private void OnImageViewRequested(object sender, ImageEventArgs e)
    {
      var view = new Image
      {
        Source = e.ImageSource,
        Stretch = Stretch.None
      };

      DisplayContent(e.ImageName, view);
    }

    private void DisplayContent(string header, object content)
    {
      DocumentView.Content = content;
      ContentTab.Header = header;
      ContentTab.Visibility = Visibility.Visible;
      ContentTab.IsSelected = true;
    }


  }
}