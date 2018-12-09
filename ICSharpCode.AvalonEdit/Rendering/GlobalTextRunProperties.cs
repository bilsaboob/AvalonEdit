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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace ICSharpCode.AvalonEdit.Rendering
{
	sealed class GlobalTextRunProperties : TextRunProperties
	{
		private Dictionary<string, object> _properties = new Dictionary<string, object>();

		internal Typeface typeface;
		internal double fontRenderingEmSize;
		internal Brush foregroundBrush;
		internal Brush backgroundBrush;
		internal System.Globalization.CultureInfo cultureInfo;

		public override Typeface Typeface { get { return typeface; } }
		public override double FontRenderingEmSize { get { return fontRenderingEmSize; } }
		public override double FontHintingEmSize { get { return fontRenderingEmSize; } }
		public override TextDecorationCollection TextDecorations { get { return null; } }
		public override Brush ForegroundBrush { get { return foregroundBrush; } }
		public override Brush BackgroundBrush { get { return backgroundBrush; } }
		public override System.Globalization.CultureInfo CultureInfo { get { return cultureInfo; } }
		public override TextEffectCollection TextEffects { get { return null; } }

		public T GetValue<T>(string key)
		{
			if (!_properties.TryGetValue(key, out var value)) return default(T);

			try
			{
				return (T)value;
			}
			catch (Exception)
			{
				return default(T);
			}
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			object objValue;
			if (!_properties.TryGetValue(key, out objValue))
			{
				value = default(T);
				return false;
			}

			try
			{
				value = (T)objValue;
				return true;
			}
			catch (Exception)
			{
				value = default(T);
				return false;
			}
		}

		public void SetValue<T>(string key, T value)
		{
			_properties[key] = value;
		}
	}

	public static class TextRunPropertiesExtensions
	{
		public static T GetValueOrDefault<T>(this TextRunProperties p, string key, Func<T> defaultValue)
		{
			if (p is GlobalTextRunProperties gp)
			{
				if (!gp.TryGetValue<T>(key, out var value))
				{
					value = defaultValue();
					p.SetValue(key, value);
				}

				return value;
			}

			return default(T);
		}

		public static T GetValue<T>(this TextRunProperties p, string key)
		{
			if (p is GlobalTextRunProperties gp)
			{
				return gp.GetValue<T>(key);
			}

			return default(T);
		}

		public static void SetValue<T>(this TextRunProperties p, string key, T value)
		{
			if (p is GlobalTextRunProperties gp)
			{
				gp.SetValue<T>(key, value);
			}
		}
	}
}
