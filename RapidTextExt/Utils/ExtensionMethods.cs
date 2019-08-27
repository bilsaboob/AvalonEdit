// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using RapidText.Utils;

namespace RapidTextExt.Utils
{
	public static class ExtensionMethodsExt
	{
		#region Epsilon / IsClose / CoerceValue	
		/// <summary>
		/// Returns true if the doubles are close (difference smaller than 0.01).
		/// </summary>
		public static bool IsClose(this Size d1, Size d2)
		{
			return ExtensionMethods.IsClose(d1.Width, d2.Width) && ExtensionMethods.IsClose(d1.Height, d2.Height);
		}
		
		/// <summary>
		/// Returns true if the doubles are close (difference smaller than 0.01).
		/// </summary>
		public static bool IsClose(this Vector d1, Vector d2)
		{
			return ExtensionMethods.IsClose(d1.X, d2.X) && ExtensionMethods.IsClose(d1.Y, d2.Y);
		}

		#endregion
		
		#region CreateTypeface
		/// <summary>
		/// Creates typeface from the framework element.
		/// </summary>
		public static Typeface CreateTypeface(this FrameworkElement fe)
		{
			return new Typeface((FontFamily)fe.GetValue(TextBlock.FontFamilyProperty),
			                    (FontStyle)fe.GetValue(TextBlock.FontStyleProperty),
			                    (FontWeight)fe.GetValue(TextBlock.FontWeightProperty),
			                    (FontStretch)fe.GetValue(TextBlock.FontStretchProperty));
		}
		#endregion

		#region DPI independence
		public static Rect TransformToDevice(this Rect rect, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
			return Rect.Transform(rect, matrix);
		}
		
		public static Rect TransformFromDevice(this Rect rect, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
			return Rect.Transform(rect, matrix);
		}
		
		public static Size TransformToDevice(this Size size, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
			return new Size(size.Width * matrix.M11, size.Height * matrix.M22);
		}
		
		public static Size TransformFromDevice(this Size size, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
			return new Size(size.Width * matrix.M11, size.Height * matrix.M22);
		}
		
		public static Point TransformToDevice(this Point point, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
			return new Point(point.X * matrix.M11, point.Y * matrix.M22);
		}
		
		public static Point TransformFromDevice(this Point point, Visual visual)
		{
			Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
			return new Point(point.X * matrix.M11, point.Y * matrix.M22);
		}
		#endregion
		
		#region System.Drawing <-> WPF conversions
		public static System.Drawing.Point ToSystemDrawing(this Point p)
		{
			return new System.Drawing.Point((int)p.X, (int)p.Y);
		}
		
		public static Point ToWpf(this System.Drawing.Point p)
		{
			return new Point(p.X, p.Y);
		}
		
		public static Size ToWpf(this System.Drawing.Size s)
		{
			return new Size(s.Width, s.Height);
		}
		
		public static Rect ToWpf(this System.Drawing.Rectangle rect)
		{
			return new Rect(rect.Location.ToWpf(), rect.Size.ToWpf());
		}
		#endregion
		
		public static IEnumerable<DependencyObject> VisualAncestorsAndSelf(this DependencyObject obj)
		{
			while (obj != null) {
				yield return obj;
				if (obj is Visual || obj is System.Windows.Media.Media3D.Visual3D) {
					obj = VisualTreeHelper.GetParent(obj);
				} else if (obj is FrameworkContentElement) {
					// When called with a non-visual such as a TextElement, walk up the
					// logical tree instead.
					obj = ((FrameworkContentElement)obj).Parent;
				} else {
					break;
				}
			}
		}
		
		[Conditional("DEBUG")]
		public static void CheckIsFrozen(Freezable f)
		{
			if (f != null && !f.IsFrozen)
				Debug.WriteLine("Performance warning: Not frozen: " + f.ToString());
		}
	}
}
