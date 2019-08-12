using System;
using System.Text;
using ICSharpCode.Text;

namespace ZenPad.Common.Text.Buffers
{
    /// <summary>IEditableBuffer implementation</summary>
    public class EditableBuffer : IEditableBuffer, IBuffer
    {
        /// <summary>
        /// Buffer to hold the subsequent characters being inserted
        /// </summary>
        private readonly StringBuilder myInsertBuffer = new StringBuilder();
        /// <summary>The rest of the text except for the insert point</summary>
        private char[] myText = Array.Empty<char>();
        /// <summary>Point where we expect text to be inserted</summary>
        private int myInsertPoint;

        private void Normalize()
        {
            this.myText = this.GetTextInternal().ToCharArray();
            this.myInsertBuffer.Length = 0;
        }

        private string GetTextInternal()
        {
            StringBuilder stringBuilder = new StringBuilder(this.Length);
            if (this.myInsertPoint > 0)
                stringBuilder.Append(new string(this.myText, 0, this.myInsertPoint));
            stringBuilder.Append(this.myInsertBuffer.ToString());
            if (this.myInsertPoint < this.myText.Length)
                stringBuilder.Append(new string(this.myText, this.myInsertPoint, this.myText.Length - this.myInsertPoint));
            return stringBuilder.ToString();
        }

        public EditableBuffer(int length)
        {
            this.myInsertBuffer.EnsureCapacity(length);
        }

        public EditableBuffer()
          : this(128)
        {
        }

        public EditableBuffer(string text)
        {
            if (text != null)
                this.myText = text.ToCharArray();
            this.myInsertPoint = 0;
            this.myInsertBuffer.Length = 0;
        }

        public int Length
        {
            get
            {
                return this.myText.Length + this.myInsertBuffer.Length;
            }
        }

        public string GetText(TextRange range)
        {
            StringBuilder builder = new StringBuilder(range.Length);
            this.AppendTextTo(builder, range);
            return builder.ToString();
        }

        public string GetText(int index, int length)
        {
            return GetText(new TextRange(index, index + length));
        }

        public void AppendTextTo(StringBuilder builder, TextRange range)
        {
            int num1 = this.myInsertPoint + this.myInsertBuffer.Length;
            builder.EnsureCapacity(builder.Length + range.Length);
            if (range.StartOffset < this.myInsertPoint)
                builder.Append(this.myText, range.StartOffset, Math.Min(range.EndOffset, this.myInsertPoint) - range.StartOffset);
            if (range.EndOffset >= this.myInsertPoint && range.StartOffset <= num1)
            {
                int num2 = range.StartOffset > this.myInsertPoint ? range.StartOffset : this.myInsertPoint;
                int num3 = range.EndOffset > num1 ? num1 : range.EndOffset;
                char[] charArray = this.myInsertBuffer.ToString().ToCharArray(num2 - this.myInsertPoint, num3 - num2);
                builder.Append(charArray);
            }
            if (range.EndOffset <= num1)
                return;
            int num4 = range.StartOffset - num1;
            if (num4 > 0)
                builder.Append(this.myText, this.myInsertPoint + num4, range.Length);
            else
                builder.Append(this.myText, this.myInsertPoint, range.EndOffset - num1);
        }

        public string GetText()
        {
            return this.GetTextInternal();
        }

        public void Remove(int offset, int length)
        {
            //Logger.Assert(length >= 0, "The condition (length >= 0) is false.");
            if (length == 0)
                return;
            if (offset >= this.myInsertPoint && offset + length == this.myInsertPoint + this.myInsertBuffer.Length)
            {
                this.myInsertBuffer.Length -= length;
            }
            else
            {
                this.Normalize();
                this.myText = (new string(this.myText, 0, offset) + new string(this.myText, offset + length, this.myText.Length - offset - length)).ToCharArray();
                this.myInsertPoint = offset;
            }
        }

        public void Insert(int offset, string text)
        {
            if (offset != this.myInsertPoint + this.myInsertBuffer.Length)
            {
                this.Normalize();
                this.myInsertPoint = offset;
            }
            this.myInsertBuffer.Append(text);
        }

        public void Replace(int offset, int length, string text)
        {
            this.Remove(offset, length);
            this.Insert(offset, text);
        }

        public void Replace(int offset, int length, BufferRange newText)
        {
            this.Replace(offset, length, newText.GetText());
        }

        public char this[int index]
        {
            get
            {
                if (index < this.myInsertPoint)
                    return this.myText[index];
                if (index > this.myInsertPoint + this.myInsertBuffer.Length)
                    return this.myText[index - this.myInsertBuffer.Length];
                return this.myInsertBuffer[index - this.myInsertPoint];
            }
        }

        public void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length)
        {
            throw new NotImplementedException("EditableBuffer.CopyTo");
        }
    }
}
