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
    private readonly Dictionary<AssemblyDefinition, AssemblyStream> _assemblyStreams = new Dictionary<AssemblyDefinition, AssemblyStream>(); 

    public event EventHandler<AssemblyDefinitionEventArgs> AssemblyAdded;
    public event EventHandler<AssemblyDefinitionEventArgs> AssemblyRemoved;

    private void OnAssemblyAdded(AssemblyDefinition definition, AssemblyStream source)
    {
      var handler = AssemblyAdded;
      if (handler != null) handler(this, new AssemblyDefinitionEventArgs(definition, source));
    }

    private void OnAssemblyRemoved(AssemblyDefinition definition, AssemblyStream source)
    {
      var handler = AssemblyRemoved;
      if (handler != null) handler(this, new AssemblyDefinitionEventArgs(definition, source));
    }

    public IEnumerable<AssemblyDefinition> Assemblies
    {
      get { return _assemblies; }
    }

    // TODO: Decide whether caching of assembly streams is needed indeed
    public AssemblyStream GetAssemblyStream(AssemblyDefinition definition)
    {
      AssemblyStream result;
      return _assemblyStreams.TryGetValue(definition, out result) ? result : null;
    }

    public void LoadAssembly(AssemblyStream source, bool raiseEvent = true)
    {
      LoadAssembly(source, AssemblyDefinition.ReadAssembly(source.OpenRead()), raiseEvent);
    }

    public void LoadAssembly(AssemblyStream source, AssemblyDefinition definition, bool raiseEvent = true)
    {
      Debug.Assert(source != null, "Source cannot be null");
      Debug.Assert(definition != null, "Definition cannot be null");

      // do nothing if assembly instance already exists
      if (_assemblies.Contains(definition)) return;

      // check whether different instance of this assembly already exists
      var existing = _assemblies.FirstOrDefault(asm => asm.FullName.Equals(definition.FullName, StringComparison.OrdinalIgnoreCase));
      // remove existing instance as we may want to update assembly
      if (existing != null) _assemblies.Remove(existing);

      // register new instance
      _assemblies.Add(definition);

      // cache reference to source stream
      _assemblyStreams[definition] = source;

      // raise appropriate event
      if (raiseEvent) OnAssemblyAdded(definition, source);
    }

    public void UnloadAssembly(AssemblyDefinition definition, bool raiseEvent = true)
    {
      Debug.Assert(definition != null, "Definition cannot be null");

      AssemblyStream source;

      if (_assemblyStreams.TryGetValue(definition, out source))
        _assemblyStreams.Remove(definition);
      
      if (_assemblies.Contains(definition))
      {
        _assemblies.Remove(definition);
        if (raiseEvent) OnAssemblyRemoved(definition, source);
        return;
      }

      // check whether different instance of this assembly already exists
      var existing = _assemblies.FirstOrDefault(asm => asm.FullName.Equals(definition.FullName, StringComparison.OrdinalIgnoreCase));
      if (existing == null) return;

      if (_assemblyStreams.TryGetValue(existing, out source))
        _assemblyStreams.Remove(existing);

      _assemblies.Remove(existing);
      if (raiseEvent) OnAssemblyRemoved(definition, source);
    }
  }
}
