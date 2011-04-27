using System.Collections.Generic;

namespace IL.View.Controls.CodeView
{
  /// <summary>
  /// Defines the contract for a source code formatter.
  /// </summary>
  internal interface IFormatter
  {
    /// <summary>
    /// Writes the parsed source code to the ouput using the specified style sheet.
    /// </summary>
    /// <param name="parsedSourceCode">The parsed source code to format and write to the output.</param>
    /// <param name="scopes">The captured scopes for the parsed source code.</param>
    /// <param name="styleSheet">The style sheet according to which the source code will be formatted.</param>
    void Write(string parsedSourceCode, IList<Scope> scopes, IStyleSheet styleSheet);
  }
}
