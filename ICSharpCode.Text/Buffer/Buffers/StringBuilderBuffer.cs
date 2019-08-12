using System.Text;

namespace RapidText.Buffer.Buffers
{
    public class StringBuilderBuffer : IBuffer
    {
        public static readonly StringBuilderBuffer Empty = new StringBuilderBuffer(new StringBuilder());
        private readonly StringBuilder myString;

        public StringBuilderBuffer(StringBuilder @string)
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
            return this.myString.ToString();
        }

        public string GetText(TextRange range)
        {
            return this.myString.ToString(range);
        }

        public string GetText(int index, int length)
        {
            return this.myString.ToString(index, length);
        }

        public void AppendTextTo(StringBuilder builder, TextRange range)
        {
            builder.Append(this.myString.ToString(range));
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
