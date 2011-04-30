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
using Mono.Cecil;

namespace IL.View.Repositories
{
  public class RepositoryAssemblySearch
  {
    private RepositoryClient _repositoryClient = new RepositoryClient();

    private IEnumerable<string> _repositories;
    private AssemblyDefinition _callingAssembly;
    private AssemblyNameReference _reference;
    private Action<string, bool> _callback;

    private Queue<string> _pendingRepositories;

    public RepositoryAssemblySearch(IEnumerable<string> repositories, AssemblyDefinition callingAssembly, AssemblyNameReference reference, Action<string, bool> callback)
    {
      if (repositories == null) throw new ArgumentNullException("repositories");
      if (callingAssembly == null) throw new ArgumentNullException("callingAssembly");
      if (reference == null) throw new ArgumentNullException("reference");
      if (callback == null) throw new ArgumentNullException("callback");

      _repositories = repositories;
      _reference = reference;
      _callingAssembly = callingAssembly;
      _callback = callback;
    }

    public void Start()
    {
      _pendingRepositories = new Queue<string>(_repositories);
      TryResolveAssembly();
    }

    private void RaiseCallback(string repository, bool result)
    {
      if (_callback != null)
        _callback(repository, result);
    }

    private void TryResolveAssembly()
    {
      if (_pendingRepositories.Count == 0)
      {
        RaiseCallback(null, false);
        return;
      }

      var repository = _pendingRepositories.Dequeue();

      _repositoryClient.CheckAssembly(repository, _callingAssembly, _reference, result =>
      {
        if (result)
        {
          Debug.WriteLine("Found reference for repository '{0}'", repository);
          RaiseCallback(repository, result);
          _pendingRepositories.Clear();
        }
        else
        {
          Debug.WriteLine("Reference not found for repository '{0}'", repository);
          TryResolveAssembly();
        }
      });
    }
  }
}
