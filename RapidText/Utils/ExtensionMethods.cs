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
using System.Xml;

namespace RapidText.Utils
{
	public static class ExtensionMethods
	{
		#region Epsilon / IsClose / CoerceValue
		/// <summary>
		/// Epsilon used for <c>IsClose()</c> implementations.
		/// We can use up quite a few digits in front of the decimal point (due to visual positions being relative to document origin),
		/// and there's no need to be too accurate (we're dealing with pixels here),
		/// so we will use the value 0.01.
		/// Previosly we used 1e-8 but that was causing issues:
		/// http://community.sharpdevelop.net/forums/t/16048.aspx
		/// </summary>
		public const double Epsilon = 0.01;
		
		/// <summary>
		/// Returns true if the doubles are close (difference smaller than 0.01).
		/// </summary>
		public static bool IsClose(this double d1, double d2)
		{
			if (d1 == d2) // required for infinities
				return true;
			return Math.Abs(d1 - d2) < Epsilon;
		}
		
		/// <summary>
		/// Forces the value to stay between mininum and maximum.
		/// </summary>
		/// <returns>minimum, if value is less than minimum.
		/// Maximum, if value is greater than maximum.
		/// Otherwise, value.</returns>
		public static double CoerceValue(this double value, double minimum, double maximum)
		{
			return Math.Max(Math.Min(value, maximum), minimum);
		}
		
		/// <summary>
		/// Forces the value to stay between mininum and maximum.
		/// </summary>
		/// <returns>minimum, if value is less than minimum.
		/// Maximum, if value is greater than maximum.
		/// Otherwise, value.</returns>
		public static int CoerceValue(this int value, int minimum, int maximum)
		{
			return Math.Max(Math.Min(value, maximum), minimum);
		}
		#endregion
		
		#region AddRange / Sequence
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> elements)
		{
			foreach (T e in elements)
				collection.Add(e);
		}
		
		/// <summary>
		/// Creates an IEnumerable with a single value.
		/// </summary>
		public static IEnumerable<T> Sequence<T>(T value)
		{
			yield return value;
		}
		#endregion
		
		#region XML reading
		/// <summary>
		/// Gets the value of the attribute, or null if the attribute does not exist.
		/// </summary>
		public static string GetAttributeOrNull(this XmlElement element, string attributeName)
		{
			XmlAttribute attr = element.GetAttributeNode(attributeName);
			return attr != null ? attr.Value : null;
		}
		
		/// <summary>
		/// Gets the value of the attribute as boolean, or null if the attribute does not exist.
		/// </summary>
		public static bool? GetBoolAttribute(this XmlElement element, string attributeName)
		{
			XmlAttribute attr = element.GetAttributeNode(attributeName);
			return attr != null ? (bool?)XmlConvert.ToBoolean(attr.Value) : null;
		}
		
		/// <summary>
		/// Gets the value of the attribute as boolean, or null if the attribute does not exist.
		/// </summary>
		public static bool? GetBoolAttribute(this XmlReader reader, string attributeName)
		{
			string attributeValue = reader.GetAttribute(attributeName);
			if (attributeValue == null)
				return null;
			else
				return XmlConvert.ToBoolean(attributeValue);
		}
		#endregion
		
		[Conditional("DEBUG")]
		public static void Log(bool condition, string format, params object[] args)
		{
			if (condition) {
				string output = DateTime.Now.ToString("hh:MM:ss") + ": " + string.Format(format, args) + Environment.NewLine + Environment.StackTrace;
				Console.WriteLine(output);
				Debug.WriteLine(output);
			}
		}
	}
}
