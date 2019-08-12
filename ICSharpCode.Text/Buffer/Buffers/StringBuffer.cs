using System.Text;
using ICSharpCode.Text;

namespace ZenPad.Common.Text.Buffers
{
    public class StringBuffer : IBuffer
    {
        public static readonly StringBuffer Empty = new StringBuffer(string.Empty);
        private readonly string myString;

        public StringBuffer(string @string)
        {
            this.myString = @string;
        }

        public int Length
        {
            get
            {
                return this.myString.Length;
            }
        }

        public string GetText()
        {
            return this.myString;
        }

        public string GetText(TextRange range)
        {
            return this.myString.Substring(range.StartOffset, range.Length);
        }

        public string GetText(int index, int length)
        {
            return this.myString.Substring(index, length);
        }

        public void AppendTextTo(StringBuilder builder, TextRange range)
        {
            builder.Append(this.myString, range.StartOffset, range.Length);
        }

        public char this[int index]
        {
            get
            {
                return this.myString[index];
            }
        }

        public void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length)
        {
            this.myString.CopyTo(sourceIndex, destinationArray, destinationIndex, length);
        }
    }
}
