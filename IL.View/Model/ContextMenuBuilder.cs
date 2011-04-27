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
using System.Windows.Controls;
using System.Windows.Input;
using IL.View.Extensibility;

namespace IL.View.Model
{
  [Export]
  public class ContextMenuBuilder
  {
    [ImportMany("http://schemas.il.view/browser/commands", AllowRecomposition = true)]
    public List<ExportFactory<ICommand, IBrowserCommandMetadata>> CommandFactories { get; set; } 

    public IEnumerable<MenuItem> GetMenuItems(object target)
    {
      if (target == null) yield break;

      var targetType = target.GetType();
      var factories = CommandFactories.Where(f => f.Metadata.TargetType == targetType);

      foreach (var factory in factories)
      {
        yield return new MenuItem
        {
          Header = factory.Metadata.Caption,
          Command = factory.CreateExport().Value,
          CommandParameter = target
        };
      }
    }
  }
}
