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
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;

namespace IL.View.Model
{
  public class AssemblyCache
  {
    private readonly List<AssemblyDefinition> _assemblies = new List<AssemblyDefinition>();

    public event EventHandler<AssemblyDefinitionEventArgs> AssemblyAdded;
    public event EventHandler<AssemblyDefinitionEventArgs> AssemblyRemoved;

    private void OnAssemblyAdded(AssemblyDefinition definition)
    {
      var handler = AssemblyAdded;
      if (handler != null) handler(this, new AssemblyDefinitionEventArgs(definition));
    }

    private void OnAssemblyRemoved(AssemblyDefinition definition)
    {
      var handler = AssemblyRemoved;
      if (handler != null) handler(this, new AssemblyDefinitionEventArgs(definition));
    }

    public IEnumerable<AssemblyDefinition> Assemblies
    {
      get
      {
        foreach (var definition in _assemblies)
          yield return definition;
      }
    }

    public void AddAssembly(AssemblyDefinition definition)
    {
      Debug.Assert(definition != null, "Definition cannot be null");

      // do nothing if assembly instance already exists
      if (_assemblies.Contains(definition)) return;

      // check whether different instance of this assembly already exists
      var existing = _assemblies.FirstOrDefault(asm => asm.FullName.Equals(definition.FullName, StringComparison.OrdinalIgnoreCase));
      // remove existing instance as we may want to update assembly
      if (existing != null) _assemblies.Remove(existing);

      // register new instance
      _assemblies.Add(definition);

      // raise appropriate event
      OnAssemblyAdded(definition);
    }

    public void RemoveAssembly(AssemblyDefinition definition)
    {
      Debug.Assert(definition != null, "Definition cannot be null");

      if (_assemblies.Contains(definition))
      {
        _assemblies.Remove(definition);
        OnAssemblyRemoved(definition);
        return;
      }

      // check whether different instance of this assembly already exists
      var existing = _assemblies.FirstOrDefault(asm => asm.FullName.Equals(definition.FullName, StringComparison.OrdinalIgnoreCase));
      if (existing == null) return;

      _assemblies.Remove(existing);
      OnAssemblyRemoved(definition);
    }
  }
}
