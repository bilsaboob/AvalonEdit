using System;
using System.Collections.Generic;
using System.Text;

namespace RapidText.Buffer.Buffers
{
    public class AggregatedBuffer : IBuffer
    {
        private readonly IBuffer[] myBuffers;

        public AggregatedBuffer(IBuffer b1, IBuffer b2, IBuffer b3 = null)
        {
            List<IBuffer> localList = new List<IBuffer>((b1 == null ? 0 : 1) + (b2 == null ? 0 : 1) + (b3 == null ? 0 : 1));
            if (b1 != null)
            {
                AggregatedBuffer aggregatedBuffer = b1 as AggregatedBuffer;
                if (aggregatedBuffer != null)
                    localList.AddRange(aggregatedBuffer.myBuffers);
                else
                    localList.Add(b1);
            }
            if (b2 != null)
            {
                AggregatedBuffer aggregatedBuffer = b2 as AggregatedBuffer;
                if (aggregatedBuffer != null)
                    localList.AddRange(aggregatedBuffer.myBuffers);
                else
                    localList.Add(b2);
            }
            if (b3 != null)
            {
                AggregatedBuffer aggregatedBuffer = b3 as AggregatedBuffer;
                if (aggregatedBuffer != null)
                    localList.AddRange(aggregatedBuffer.myBuffers);
                else
                    localList.Add(b3);
            }
            this.myBuffers = localList.ToArray();
        }

        public AggregatedBuffer(IBuffer[] buffers, bool simplify = false)
        {
            if (simplify)
            {
                List<IBuffer> localList = new List<IBuffer>(buffers.Length);
                foreach (IBuffer buffer in buffers)
                {
                    AggregatedBuffer aggregatedBuffer = buffer as AggregatedBuffer;
                    if (aggregatedBuffer != null)
                        localList.AddRange(aggregatedBuffer.myBuffers);
                    else
                        localList.Add(buffer);
                }
                this.myBuffers = localList.ToArray();
            }
            else
                this.myBuffers = buffers;
        }

        public int Length
        {
            get
            {
                int num = 0;
                for (int index = 0; index < this.myBuffers.Length; ++index)
                    num += this.myBuffers[index].Length;
                return num;
            }
        }

        public string GetText()
        {
            return this.GetText(TextRange.FromLength(this.Length));
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
            range.AssertValid();
            int index = 0;
            int num1 = 0;
            while (index < this.myBuffers.Length)
            {
                int num2 = num1 + this.myBuffers[index].Length;
                if (range.StartOffset < num2)
                {
                    TextRange range1 = new TextRange(range.StartOffset - num1, Math.Min(range.EndOffset, num2) - num1);
                    this.myBuffers[index].AppendTextTo(builder, range1);
                    if (range.EndOffset <= num2)
                        break;
                    range = range.SetStartTo(num2);
                }
                ++index;
                num1 = num2;
            }
        }

        public char this[int index]
        {
            get
            {
                int index1 = 0;
                int num1 = 0;
                while (index1 < this.myBuffers.Length)
                {
                    int num2 = num1 + this.myBuffers[index1].Length;
                    if (index < num2)
                        return this.myBuffers[index1][index - num1];
                    ++index1;
                    num1 = num2;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public void CopyTo(int sourceIndex, char[] destinationArray, int destinationIndex, int length)
        {
            int index = 0;
            int num1 = 0;
            while (index < this.myBuffers.Length)
            {
                int num2 = num1 + this.myBuffers[index].Length;
                if (sourceIndex < num2)
                {
                    int length1 = Math.Min(Math.Min(length, this.myBuffers[index].Length - (sourceIndex - num1)), destinationArray.Length - destinationIndex);
                    this.myBuffers[index].CopyTo(sourceIndex - num1, destinationArray, destinationIndex, length1);
                    if (sourceIndex + length <= num2)
                        break;
                    sourceIndex = num2;
                    destinationIndex += length1;
                    length -= length1;
                }
                ++index;
                num1 = num2;
            }
        }

        public IBuffer Project(TextRange range)
        {
            range.AssertValid();
            List<IBuffer> localList = new List<IBuffer>(this.myBuffers.Length);
            int index = 0;
            int num1 = 0;
            while (index < this.myBuffers.Length)
            {
                int num2 = num1 + this.myBuffers[index].Length;
                if (range.StartOffset < num2)
                {
                    TextRange range1 = new TextRange(range.StartOffset - num1, Math.Min(range.EndOffset, num2) - num1);
                    localList.Add(ProjectedBuffer.Create(this.myBuffers[index], range1));
                    if (range.EndOffset > num2)
                        range = range.SetStartTo(num2);
                    else
                        break;
                }
                ++index;
                num1 = num2;
            }
            if (localList.Count == 1)
                return localList[0];
            return (IBuffer)new AggregatedBuffer(localList.ToArray(), false);
        }
    }
}
