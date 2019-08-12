using System.Runtime.InteropServices;
using ICSharpCode.Text;

namespace ZenPad.Common.Text
{
    [StructLayout(LayoutKind.Auto)]
    public struct BufferRange
    {
        private readonly IBuffer myBuffer;
        private TextRange myRange;

        public BufferRange(IBuffer buffer, TextRange range)
        {
            this.myBuffer = buffer;
            this.myRange = range;
        }

        public void CopyTo(char[] destinationArray, int destinationIndex)
        {
            this.myBuffer.CopyTo(this.myRange.StartOffset, destinationArray, destinationIndex, this.myRange.Length);
        }

        public string GetText()
        {
            return this.myBuffer.GetText(this.myRange);
        }

        public IBuffer Buffer
        {
            get
            {
                return this.myBuffer;
            }
        }

        public TextRange Range
        {
            get
            {
                return this.myRange;
            }
        }
    }
}
