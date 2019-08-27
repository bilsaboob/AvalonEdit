using System;
using System.Diagnostics;
using System.Text;

namespace RapidText.Buffer.Buffers
{
    public class ArrayBuffer : IEditableBuffer, IBuffer
    {
        //private static readonly Statistics Statistics = Statistics.Allocate(string.Format("Collections.ArrayBuffer"));
        private const int GapSize = 1024;
        private char[] myBuffer;
        private int myLength;
        private string myText;

        public ArrayBuffer(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            this.myText = text;
            this.myLength = text.Length;
        }

        public ArrayBuffer(IBuffer buf)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(buf));
            this.myText = buf.GetText();
            this.myLength = buf.Length;
        }

        public ArrayBuffer(char[] buf)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(buf));
            this.myBuffer = new char[buf.Length];
            Array.Copy((Array)buf, 0, (Array)this.myBuffer, 0, buf.Length);
            this.myLength = buf.Length;
            this.myText = new string(buf);
        }

        public char[] Buffer
        {
            get
            {
                return this.myBuffer;
            }
        }

        public int Length
        {
            get
            {
                return this.myLength;
            }
        }

        public string GetText(TextRange range)
        {
            int startOffset = range.StartOffset;
            int length = range.Length;
            return GetText(startOffset, length);
        }

        public string GetText(int startOffset, int length)
        {
            this.AssertOffsetAndLength(startOffset, length);
            if (this.myBuffer == null)
                return this.myText.Substring(startOffset, length);
            return new string(this.myBuffer, startOffset, length);
        }

        public void AppendTextTo(StringBuilder builder, TextRange range)
        {
            int startOffset = range.StartOffset;
            int length = range.Length;
            this.AssertOffsetAndLength(startOffset, length);
            if (this.myBuffer != null)
                builder.Append(this.myBuffer, startOffset, length);
            else
                builder.Append(this.myText, startOffset, length);
        }

        public string GetText()
        {
            if (this.myText == null)
                this.myText = new string(this.myBuffer, 0, this.myLength);
            return this.myText;
        }

        public void Insert(int offset, string text)
        {
            this.AssertOffsetAndLength(offset, 0);
            //Logger.Assert(offset <= this.myLength, "The condition (offset <= myLength) is false.");
            this.EnsureCharArray(this.myLength + text.Length);
            Array.Copy((Array)this.myBuffer, offset, (Array)this.myBuffer, offset + text.Length, this.myLength - offset);
            text.CopyTo(0, this.myBuffer, offset, text.Length);
            this.myLength += text.Length;
        }

        public void Remove(int offset, int length)
        {
            this.AssertOffsetAndLength(offset, length);
            //Logger.Assert(offset + length <= this.myLength, "The condition (offset + length <= myLength) is false.");
            this.EnsureCharArray(this.myLength - offset - length);
            Array.Copy((Array)this.myBuffer, offset + length, (Array)this.myBuffer, offset, this.myLength - offset - length);
            this.myLength -= length;
        }

        public void Replace(int offset, int length, string newText)
        {
            this.AssertOffsetAndLength(offset, length);
            this.EnsureCharArray(this.myLength - length + newText.Length);
            Array.Copy((Array)this.myBuffer, offset + length, (Array)this.myBuffer, offset + newText.Length, this.myLength - offset - length);
            newText.CopyTo(0, this.myBuffer, offset, newText.Length);
            this.myLength = this.myLength - length + newText.Length;
        }

        public void Replace(int offset, int length, BufferRange newText)
        {
            this.AssertOffsetAndLength(offset, length);
            this.EnsureCharArray(this.myLength - length + newText.Range.Length);
            Array.Copy((Array)this.myBuffer, offset + length, (Array)this.myBuffer, offset + newText.Range.Length, this.myLength - offset - length);
            newText.CopyTo(this.myBuffer, offset);
            this.myLength = this.myLength - length + newText.Range.Length;
        }

        public char this[int index]
        {
            get
            {
                if (this.myBuffer != null)
                    return this.myBuffer[index];
                return this.myText[index];
            }
        }

        public void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length)
        {
            if (this.myBuffer == null)
                this.myText.CopyTo(sourceIndex, destinationArray, destinationIndex, length);
            else
                Array.Copy((Array)this.myBuffer, sourceIndex, (Array)destinationArray, destinationIndex, length);
        }

        private void EnsureCharArray(int capacity)
        {
            if (this.myBuffer == null)
            {
                if (capacity <= this.myText.Length)
                {
                    this.myBuffer = this.myText.ToCharArray();
                }
                else
                {
                    this.myBuffer = new char[capacity + 1024];
                    this.myText.CopyTo(0, this.myBuffer, 0, this.myText.Length);
                }
                this.myText = (string)null;
            }
            else
            {
                this.myText = (string)null;
                if (capacity <= this.myBuffer.Length)
                    return;
                char[] chArray = new char[capacity + 1024];
                this.myBuffer.CopyTo((Array)chArray, 0);
                this.myBuffer = chArray;
            }
        }

        /// <summary>
        /// Assertion method to avoid creating closure objects when no exceptions are thrown
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        [Conditional("JET_MODE_ASSERT")]
        private void AssertOffsetAndLength(int offset, int length)
        {
            if (length < 0)
                this.ThrowLengthException(offset, length);
            if (offset >= 0 && offset + length <= this.myLength)
                return;
            this.ThrowRangeException(offset, length);
        }

        private void ThrowRangeException(int offset, int length)
        {
            ArgumentException exception = new ArgumentException("Range falls outside buffer.");
            /*exception.AddData<ArgumentException>("Offset", (Func<object>)(() => (object)offset));
            exception.AddData<ArgumentException>("Length", (Func<object>)(() => (object)length));
            exception.AddData<ArgumentException>("BufferLength", (Func<object>)(() => (object)this.myLength));*/
            throw exception;
        }

        private void ThrowLengthException(int offset, int length)
        {
            ArgumentOutOfRangeException exception = new ArgumentOutOfRangeException(nameof(length), "Range length must be nonnegative.");
            /*exception.AddData<ArgumentOutOfRangeException>("Offset", (Func<object>)(() => (object)offset));
            exception.AddData<ArgumentOutOfRangeException>("Length", (Func<object>)(() => (object)length));
            exception.AddData<ArgumentOutOfRangeException>("BufferLength", (Func<object>)(() => (object)this.myLength));*/
            throw exception;
        }
    }
}
