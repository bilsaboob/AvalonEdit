using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RapidText.Document
{
	public interface ITextAccess
	{
		Task<T> ReadAccess<T>(Func<TextDocument, T> action);
		Task WriteAccess(Action<TextDocument> action);
	}
}
