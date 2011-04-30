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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Browser;
using System.Text;
using System.Windows;
using Mono.Cecil;

namespace IL.View.Repositories
{
  public class RepositoryClient : DependencyObject
  {
    private static string GetRepositoryAddress(AssemblyDefinition callingAssembly, AssemblyNameReference reference)
    {
      var builder = new StringBuilder();
      builder.AppendFormat("/{0}/", callingAssembly.MainModule.Architecture.ToString());
      if (reference.HasSilverlightToken() || callingAssembly.IsSilverlight())
        builder.Append("Silverlight");
      else
        builder.Append(callingAssembly.MainModule.Runtime.ToString());
      builder.AppendFormat("/assembly?name={0}", reference.Name);
      builder.AppendFormat("&version={0}", reference.Version.ToString());
      builder.AppendFormat("&token={0}", reference.GetPublicTokenKeyString());
      builder.Append("&specificversion=false");

      return builder.ToString();
    }

    public void CheckAssembly(string repository, AssemblyDefinition callingAssembly, AssemblyNameReference reference, Action<bool> callback)
    {
      if (string.IsNullOrWhiteSpace(repository)) throw new ArgumentNullException("repository");
      if (callingAssembly == null) throw new ArgumentNullException("callingAssembly");
      if (reference == null) throw new ArgumentNullException("reference");
      if (callback == null) throw new ArgumentNullException("callback");

      repository = repository.TrimEnd('/') + "/verify";

      var uriString = GetRepositoryAddress(callingAssembly, reference);
      var uri = new Uri(repository + uriString, UriKind.RelativeOrAbsolute);
      var request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(uri);
      request.Method = "GET";
      request.BeginGetResponse(r =>
      {
        try
        {
          var rs = (HttpWebResponse)request.EndGetResponse(r);
          if (rs != null && rs.StatusCode == HttpStatusCode.OK)
            Dispatcher.BeginInvoke(() => callback(true));
        }
        catch (WebException ex)
        {
          Debug.WriteLine(ex.Message);
          Dispatcher.BeginInvoke(() => callback(false));
        }

      }, request);
    }

    public void GetAssembly(string repository, AssemblyDefinition callingAssembly, AssemblyNameReference reference, Action<Stream> callback)
    {
      if (string.IsNullOrWhiteSpace(repository)) throw new ArgumentNullException("repository");
      if (callingAssembly == null) throw new ArgumentNullException("callingAssembly");
      if (reference == null) throw new ArgumentNullException("reference");
      if (callback == null) throw new ArgumentNullException("callback");

      repository = repository.TrimEnd('/');

      var uriString = GetRepositoryAddress(callingAssembly, reference);
      var uri = new Uri(repository + uriString, UriKind.RelativeOrAbsolute);
      var request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(uri);
      request.Method = "GET";
      request.BeginGetResponse(r =>
      {
        try
        {
          var rs = (HttpWebResponse)request.EndGetResponse(r);
          var stream = rs.GetResponseStream();

          Dispatcher.BeginInvoke(() => callback(stream));
        }
        catch (WebException ex)
        {
          Debug.WriteLine(ex.Message);
          Dispatcher.BeginInvoke(() => callback(null));
        }

      }, request);
    }
  }
}
