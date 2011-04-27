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
using System.Linq;
using Mono.Cecil;

namespace IL.View.Controls
{
  internal static class OverlayIconProvider
  {
    public static IEnumerable<string> GetOverlays(TypeDefinition definition)
    {
      var overlays = new List<string>();

      if (definition.IsNotPublic || definition.IsNestedPrivate)
        overlays.Add(DefaultImages.AssemblyBrowser.Private);

      return overlays;
    }

    public static IEnumerable<string> GetOverlays(PropertyDefinition definition)
    {
      return (definition.GetMethod != null) ? GetOverlays(definition.GetMethod) : Enumerable.Empty<string>();
    }

    public static IEnumerable<string> GetOverlays(EventDefinition definition)
    {
      return (definition.AddMethod != null) ? GetOverlays(definition.AddMethod) : Enumerable.Empty<string>();
    }

    public static IEnumerable<string> GetOverlays(MethodDefinition definition)
    {
      var overlays = new List<string>();

      if (definition.IsAssembly)
        overlays.Add(DefaultImages.AssemblyBrowser.Internal);

      if (definition.IsFamily)
        overlays.Add(DefaultImages.AssemblyBrowser.Protected);

      if (definition.IsPrivate)
        overlays.Add(DefaultImages.AssemblyBrowser.Private);

      if (definition.IsStatic)
        overlays.Add(DefaultImages.AssemblyBrowser.Static);

      return overlays;
    }

    public static IEnumerable<string> GetOverlays(FieldDefinition definition)
    {
      var overlays = new List<string>();

      if (definition.IsAssembly)
        overlays.Add(DefaultImages.AssemblyBrowser.Internal);

      if (definition.IsFamily)
        overlays.Add(DefaultImages.AssemblyBrowser.Protected);

      if (definition.IsPrivate)
        overlays.Add(DefaultImages.AssemblyBrowser.Private);

      if (definition.IsStatic && !definition.IsLiteral)
        overlays.Add(DefaultImages.AssemblyBrowser.Static);

      return overlays;
    }
  }
}
