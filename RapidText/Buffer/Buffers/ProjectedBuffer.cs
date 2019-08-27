using System;
using System.Text;

namespace RapidText.Buffer.Buffers
{
    public class ProjectedBuffer : IBuffer
    {
        private readonly IBuffer myUnderlyingBuffer;
        private readonly TextRange myRange;

        public static IBuffer Create(IBuffer underlyingBuffer, TextRange range)
        {
            if (range.StartOffset == 0 && range.Length == underlyingBuffer.Length)
                return underlyingBuffer;
            AggregatedBuffer aggregatedBuffer = underlyingBuffer as AggregatedBuffer;
            if (aggregatedBuffer != null)
                return aggregatedBuffer.Project(range);
            ProjectedBuffer projectedBuffer = underlyingBuffer as ProjectedBuffer;
            if (projectedBuffer == null)
                return (IBuffer)new ProjectedBuffer(underlyingBuffer, range);
            int offset = projectedBuffer.myRange.StartOffset + range.StartOffset;
            int length = Math.Min(range.Length, projectedBuffer.myRange.Length - range.StartOffset);
            return (IBuffer)new ProjectedBuffer(projectedBuffer.myUnderlyingBuffer, TextRange.FromLength(offset, length));
        }

        private ProjectedBuffer(IBuffer underlyingBuffer, TextRange range)
        {
            this.myUnderlyingBuffer = underlyingBuffer;
            this.myRange = range;
        }

        public int Length
        {
            get
            {
                return this.myRange.Length;
            }
        }

        public int TextStartOffset
        {
            get
            {
                return this.myRange.StartOffset;
            }
        }

        public int TextEndOffset
        {
            get
            {
                return this.myRange.EndOffset;
            }
        }

        public string GetText()
        {
            return this.myUnderlyingBuffer.GetText(this.myRange);
        }

        public string GetText(TextRange range)
        {
            return this.myUnderlyingBuffer.GetText(range.Shift(this.myRange.StartOffset));
        }

        public string GetText(int index, int length)
        {
            return GetText(new TextRange(index, index + length));
        }

        public void AppendTextTo(StringBuilder builder, TextRange range)
        {
            this.myUnderlyingBuffer.AppendTextTo(builder, range.Shift(this.myRange.StartOffset));
        }

        public char this[int index]
        {
            get
            {
                return this.myUnderlyingBuffer[this.myRange.StartOffset + index];
            }
        }

        public void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length)
        {
            this.myUnderlyingBuffer.CopyTo(sourceIndex + this.myRange.StartOffset, destinationArray, destinationIndex, length);
        }
    }
}
