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
using System.ComponentModel.Composition;
using System.Windows.Input;
using IL.View.Decompiler;
using IL.View.Extensibility;
using Mono.Cecil;
using System.Diagnostics;

namespace IL.View.Toolkit.Commands
{
  public class DisassembleCommand<T> : ICommand where T : class
  {
    [Import]
    public DecompilerManager DecompilerManager { get; set; }

    public bool CanExecute(object parameter)
    {
      return parameter is T;
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
      if (Debugger.IsAttached) Debugger.Break();
      // TODO: Provide calling assembly
      DecompilerManager.RequestCodeDisassembly(null, parameter);
    }
  }

  [BrowserCommand(typeof(AssemblyDefinition), "DisassembleAssembly", "Disassemble")]
  public sealed class DisassembleAssemblyCommand : DisassembleCommand<AssemblyDefinition> { }

  [BrowserCommand(typeof(NamespaceDefinition), "DisassembleNamespace", "Disassemble")]
  public sealed class DisassembleNamespaceCommand : DisassembleCommand<NamespaceDefinition> { }

  [BrowserCommand(typeof(ModuleDefinition), "DisassembleModule", "Disassemble")]
  public sealed class DisassembleModuleCommand : DisassembleCommand<ModuleDefinition> { }

  [BrowserCommand(typeof(TypeDefinition), "DisassembleType", "Disassemble")]
  public sealed class DisassembleTypeCommand : DisassembleCommand<TypeDefinition> { }

  [BrowserCommand(typeof(MethodDefinition), "DisassembleMethod", "Disassemble")]
  public sealed class DisassembleMethodCommand : DisassembleCommand<MethodDefinition> { }

  [BrowserCommand(typeof(PropertyDefinition), "DisassembleProperty", "Disassemble")]
  public sealed class DisassemblePropertyCommand : DisassembleCommand<PropertyDefinition> { }

  [BrowserCommand(typeof(FieldDefinition), "DisassembleField", "Disassemble")]
  public sealed class DisassembleFieldCommand : DisassembleCommand<FieldDefinition> { }

  [BrowserCommand(typeof(EventDefinition), "DisassembleEvent", "Disassemble")]
  public sealed class DisassembleEventCommand : DisassembleCommand<EventDefinition> { }
}