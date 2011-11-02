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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace IL.View
{
  internal static class DefaultImages
  {    
    public static class AssemblyBrowser
    {
      private const string ResourcePath = "/IL.View;component/Images/AssemblyBrowser/";

      public const string Assembly = "assembly-project-16";
      public const string Module = "assembly-module-16";
      public const string Reference = "project-reference-16";
      public const string Namespace = "element-namespace-16";
      public const string Class = "element-class-16";
      public const string Structure = "element-structure-16";
      public const string Enumeration = "element-enumeration-16";
      public const string Literal = "element-literal-16";
      public const string Field = "element-field-16";
      public const string Property = "element-property-16";
      public const string Method = "element-method-16";
      public const string Delegate = "element-delegate-16";
      public const string ExtensionMethod = "element-extensionmethod-16";
      public const string Interface = "element-interface-16";
      public const string Event = "element-event-16";

      public const string Private = "visibility-private-16";
      public const string Protected = "visibility-protected-16";
      public const string Internal = "visibility-internal-16";
      public const string Static = "static-16";

      public const string BaseTypes = "base-types-16";
      public const string DerivedTypes = "derived-types-16";

      public const string Package = "package-x-generic_22";
      public const string XapPackage = "package-xap";
      public const string NugetPackage = "package-nuget";

      public const string FileXml = "file-xml-16";
      public const string FileMisc = "file-misc-16";
      public const string FileImage = "file-image-16";
      public const string FileResource = "file-resource-16";
      public const string Folder = "folder-16";
      public const string Bug = "bug";
      public const string BugError = "bug_error";

      // TODO: Introduce caching!
      public static Image GetDefaultImage(string name)
      {
        return new Image
        {
          Source = new BitmapImage(new Uri(string.Format("{0}{1}.png", ResourcePath, name), UriKind.RelativeOrAbsolute))
        };
      }

      // Returns 

      /// <summary>
      /// Gets the name of the icon resource for a given file name based on file extension.
      /// </summary>
      /// <param name="fileName">Name of the file.</param>
      /// <returns>Name of the default icon resource.</returns>
      public static string GetFileIcon(string fileName)
      {
        if (string.IsNullOrWhiteSpace(fileName)) return FileMisc;

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension)) return FileMisc;

        switch (extension.ToLowerInvariant())
        {
          case ".dll":
            return Assembly;
          case ".xml":
          case ".xaml":
          case ".clientconfig":
            return FileXml;
          case ".png":
          case ".jpg":
            return FileImage;
          default:
            return FileMisc;
        }
      }
    }    
  }
}
