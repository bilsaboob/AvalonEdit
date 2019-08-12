using System.Text;
using ICSharpCode.Text;

namespace ZenPad.Common.Text
{
    /// <summary>Buffer that allows retrieval of contents by position</summary>
    public interface IBuffer
    {
        int Length { get; }

        string GetText();

        string GetText(TextRange range);

        string GetText(int index, int length);

        void AppendTextTo(StringBuilder builder, TextRange range);

        char this[int index] { get; }

        void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length);
    }

    /// <summary>
    /// Interface for a buffer supporting efficient editing of content in arbitrary positions
    /// </summary>
    public interface IEditableBuffer : IBuffer
    {
        void Insert(int offset, string text);

        void Remove(int offset, int length);

        void Replace(int offset, int length, string newText);

        void Replace(int offset, int length, BufferRange newText);
    }
}
