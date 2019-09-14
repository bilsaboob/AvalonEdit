﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using RapidText.Document;

namespace ICSharpCode.AvalonEdit.Rendering
{
	/// <summary>
	/// EventArgs for the <see cref="TextView.VisualLineConstructionStarting"/> event.
	/// </summary>
	public class VisualLineConstructionContext : EventArgs
	{
		private ConcurrentDictionary<string, object> _data;
		
		/// <summary>
		/// Creates a new VisualLineConstructionStartEventArgs instance.
		/// </summary>
		public VisualLineConstructionContext(TextView textView, Guid id, DocumentLine firstLineInView, ITextSourceVersion textVersion)
		{
			this.TextView = textView;
			this.FirstLineInView = firstLineInView;
			this.Id = id;
			this.TextVersion = textVersion;
		}

		/// <summary>
		/// Gets/Sets the first line that is visible in the TextView.
		/// </summary>
		public DocumentLine FirstLineInView { get; private set; }

		public Guid Id { get; private set; }

		public ITextSourceVersion TextVersion { get; private set; }

		public TextView TextView { get; private set; }

		public void Set<T>(string id, T value)
		{
			if(_data == null)
				_data = new ConcurrentDictionary<string, object>();
			_data[id] = value;
		}
		
		public bool TryGet<T>(string id, out T value)
		{
			value = default(T);
			if (_data == null) return false;
			if (!_data.TryGetValue(id, out var obj))
				return false;

			try
			{
				value = (T)obj;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public T GetOrAdd<T>(string id, Func<T> defaultFn)
		{
			if(_data == null)
				_data = new ConcurrentDictionary<string, object>();
			return (T)_data.GetOrAdd(id, _=>defaultFn());
		}
	}
}
