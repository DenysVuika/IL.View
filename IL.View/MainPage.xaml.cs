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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace IL.View
{
  public partial class MainPage : UserControl
  {
    public MainPage()
    {
      InitializeComponent();
    }

    // After the Frame navigates, ensure the HyperlinkButton representing the current page is selected
    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
      foreach (UIElement child in LinksStackPanel.Children)
      {
        HyperlinkButton hb = child as HyperlinkButton;
        if (hb != null && hb.NavigateUri != null)
        {
          if (ContentFrame.UriMapper.MapUri(e.Uri).ToString().Equals(ContentFrame.UriMapper.MapUri(hb.NavigateUri).ToString()))
          {
            VisualStateManager.GoToState(hb, "ActiveLink", true);
          }
          else
          {
            VisualStateManager.GoToState(hb, "InactiveLink", true);
          }
        }
      }
    }

    // If an error occurs during navigation, show an error window
    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
      e.Handled = true;
      ChildWindow errorWin = new ErrorWindow(e.Uri);
      errorWin.Show();
    }

  }
}