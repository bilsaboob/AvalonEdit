using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.AvalonEdit.Utils
{
	public interface IDispatcher
	{
		Task<T> InvokeAsync<T>(Func<T> action)
			where T : class;

		Task InvokeAsync(Action action);
	}
}
