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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

using RapidText;
using RapidText.Document;
using RapidText.Utils;
using RapidTextExt.Utils;

#if NREFACTORY
using ICSharpCode.NRefactory.Editor;
#endif

namespace RapidTextExt.Document
{
	
	/// <summary>
	/// <para>
	/// A collection of text segments that supports efficient lookup of segments
	/// intersecting with another segment.
	/// </para>
	/// </summary>
	/// <remarks><inheritdoc cref="TextSegment"/></remarks>
	/// <see cref="TextSegment"/>
	public class TextSegmentCollection<T> : TextSegmentTree<T>, IWeakEventListener where T : TextSegment
	{
		#region Constructor
		/// <summary>
		/// Creates a new TextSegmentCollection that needs manual calls to <see cref="UpdateOffsets(DocumentChangeEventArgs)"/>.
		/// </summary>
		public TextSegmentCollection()
			: base()
		{
		}
		
		/// <summary>
		/// Creates a new TextSegmentCollection that updates the offsets automatically.
		/// </summary>
		/// <param name="textDocument">The document to which the text segments
		/// that will be added to the tree belong. When the document changes, the
		/// position of the text segments will be updated accordingly.</param>
		public TextSegmentCollection(TextDocument textDocument)
			: base(textDocument)
		{
			TextDocumentWeakEventManager.Changed.AddListener(textDocument, this);
		}
		#endregion
		
		#region OnDocumentChanged / UpdateOffsets
		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(TextDocumentWeakEventManager.Changed)) {
				OnDocumentChanged((DocumentChangeEventArgs)e);
				return true;
			}
			return false;
		}
		
		#endregion
	}
}
