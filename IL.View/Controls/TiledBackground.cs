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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IL.View.Controls
{
  public class TiledBackground : UserControl
  {
    private Image tiledImage = new Image();
    private BitmapImage bitmap;
    private int lastWidth, lastHeight = 0;
    private WriteableBitmap sourceBitmap;

    public TiledBackground()
    {
      // create an image as the content of the control
      tiledImage.Stretch = Stretch.None;
      this.Content = tiledImage;

      // no sizechanged to override
      this.SizeChanged += new SizeChangedEventHandler(TiledBackground_SizeChanged);
    }

    void TiledBackground_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      UpdateTiledImage();
    }

    private void UpdateTiledImage()
    {
      if (sourceBitmap != null)
      {
        int width = (int)Math.Ceiling(this.ActualWidth);
        int height = (int)Math.Ceiling(this.ActualHeight);

        // only regenerate the image if the width/height has grown
        if (width < lastWidth && height < lastHeight) return;
        lastWidth = width;
        lastHeight = height;

        WriteableBitmap final = new WriteableBitmap(width, height);

        for (int x = 0; x < final.PixelWidth; x++)
        {
          for (int y = 0; y < final.PixelHeight; y++)
          {
            int tiledX = (x % sourceBitmap.PixelWidth);
            int tiledY = (y % sourceBitmap.PixelHeight);
            final.Pixels[y * final.PixelWidth + x] = sourceBitmap.Pixels[tiledY * sourceBitmap.PixelWidth + tiledX];
          }
        }

        tiledImage.Source = final;
      }
    }

    #region SourceUri (DependencyProperty)

    /// <summary>
    /// A description of the property.
    /// </summary>
    public Uri SourceUri
    {
      get { return (Uri)GetValue(SourceUriProperty); }
      set { SetValue(SourceUriProperty, value); }
    }
    public static readonly DependencyProperty SourceUriProperty =
        DependencyProperty.Register("SourceUri", typeof(Uri), typeof(TiledBackground),
        new PropertyMetadata(null, new PropertyChangedCallback(OnSourceUriChanged)));

    private static void OnSourceUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((TiledBackground)d).OnSourceUriChanged(e);
    }

    protected virtual void OnSourceUriChanged(DependencyPropertyChangedEventArgs e)
    {
      bitmap = new BitmapImage(e.NewValue as Uri);
      bitmap.CreateOptions = BitmapCreateOptions.None;
      bitmap.ImageOpened += new EventHandler<RoutedEventArgs>(bitmap_ImageOpened);
    }

    void bitmap_ImageOpened(object sender, RoutedEventArgs e)
    {
      sourceBitmap = new WriteableBitmap(bitmap);
      UpdateTiledImage();
    }

    #endregion
  }
}
