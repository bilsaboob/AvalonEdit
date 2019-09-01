using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Rapid.Extensions;
using RapidText;

namespace ZenPad.Common.Core.Collections
{
    public class UniqueTextRangeSet<T>
    {
        private UniqueTextRange<T> _first;
        private UniqueTextRange<T> _last;

        public UniqueTextRangeSet()
        {
        }

        public UniqueTextRange<T> First => _first;

        public IEnumerable<UniqueTextRange<T>> GetAll()
        {
            var e = First;
            while (e != null)
            {
                yield return e;
                e = e.Next;
            }
        }

        public void Clear()
        {
            _first = null;
            _last = null;
        }

        public void Add(TextRange newRange, List<T> associatedItems = null)
        {
            // if no existing value... set the first
            if (_first == null)
            {
                _first = _last = new UniqueTextRange<T>() {
                    Range = newRange,
                    AssociatedItems = associatedItems
                };
                return;
            }
            
            // find the first range that overlaps the start
            var first = FindFirst(_first, newRange);
            if (first == null)
            {
                // we have no overlap... first before and after...
                var prev = FindFirstBefore(_first, newRange);
                var next = prev?.Next;
                if (prev == null)
                {
                    // there is no range before this new... so it's a new first...
                    next = _first;
                }

                var newUniqueRange = new UniqueTextRange<T>() {
                    Range = newRange,
                    AssociatedItems = associatedItems,
                };
                newUniqueRange.Prev = prev;
                newUniqueRange.Next = next;
                if (prev != null)
                    prev.Next = newUniqueRange;
                if (next != null)
                    next.Prev = newUniqueRange;
                if (prev == null)
                    _first = newUniqueRange;
                if (next == null)
                    _last = newUniqueRange;
                return;
            }

            // check if the first and the added are exactly same
            if (first.Range.Equals(newRange))
            {
                // just combine the associated items
                first.AssociatedItems = first.AssociatedItems.UniqueUnionWith(associatedItems).ToList();
                return;
            }

            // we have found the first offset, we will now update the existing ranges with the associated items, and then create new ranges for the "in between" sections...
            var range = first;
            var remaining = newRange;
            while (range != null && !remaining.IsEmpty)
            {
                var remainingStartWithinRange = ContainsStart(range, remaining);
                var remainingEndWithinRange = ContainsEnd(range, remaining);

                if (!remainingStartWithinRange && !remainingEndWithinRange)
                {
                    // neither START nor END is within the range... so it can either CONTAIN the range... or it's before / after...
                    if (remaining.EndOffset <= range.StartOffset)
                    {
                        // it's before...
                    }
                    else if (remaining.StartOffset >= range.EndOffset)
                    {
                        // it's after
                    }
                    else
                    {
                        // it completely contains the range... but could need to create range to the left / right

                        TextRange left = TextRange.Empty;
                        TextRange mid = TextRange.Empty;

                        var newAssociatedItems = range.AssociatedItems.UniqueUnionWith(associatedItems).ToList();
                        var leftAssociatedItems = newAssociatedItems;
                        var midAssociatedItems = newAssociatedItems;

                        var startDiff = range.StartOffset - remaining.StartOffset;
                        var endDiff = remaining.EndOffset - range.EndOffset;

                        var nextStart = remaining.StartOffset;
                        if (startDiff > 0)
                        {
                            left = new TextRange(nextStart, range.StartOffset);
                            nextStart = left.EndOffset;
                            leftAssociatedItems = associatedItems;
                        }

                        mid = new TextRange(nextStart, range.EndOffset);
                        nextStart = mid.EndOffset;

                        if (endDiff > 0)
                        {
                            remaining = new TextRange(nextStart, remaining.EndOffset);
                        }
                        
                        ReplaceRange(range, NewUniqueRange(left, leftAssociatedItems), NewUniqueRange(mid, midAssociatedItems), null);
                    }
                }
                else if (remainingStartWithinRange && remainingEndWithinRange)
                {
                    // both the START & END are within the range
                    // fully contained... so we split in 3 pieces...

                    TextRange left = TextRange.Empty;
                    TextRange mid = TextRange.Empty;
                    TextRange right = TextRange.Empty;

                    var newAssociatedItems = range.AssociatedItems.UniqueUnionWith(associatedItems).ToList();
                    var leftAssociatedItems = newAssociatedItems;
                    var midAssociatedItems = newAssociatedItems;
                    var rightAssociatedItems = newAssociatedItems;

                    var startDiff = remaining.StartOffset - range.StartOffset;
                    var endDiff = range.EndOffset - remaining.EndOffset;

                    var nextStart = range.StartOffset;
                    if (startDiff > 0)
                    {
                        left = new TextRange(nextStart, remaining.StartOffset);
                        nextStart = left.EndOffset;
                        leftAssociatedItems = range.AssociatedItems;
                    }

                    mid = new TextRange(nextStart, remaining.EndOffset);
                    nextStart = mid.EndOffset;

                    if (endDiff > 0)
                    {
                        right = new TextRange(nextStart, range.EndOffset);
                        rightAssociatedItems = range.AssociatedItems;
                    }
                    
                    ReplaceRange(range, NewUniqueRange(left, leftAssociatedItems), NewUniqueRange(mid, midAssociatedItems), NewUniqueRange(right, rightAssociatedItems));

                    remaining = TextRange.Empty;

                    // we are done... since it's contained
                    break;
                }
                else if (remainingStartWithinRange)
                {
                    // start offset is within... so we split in 2 pieces at the new start offset...

                    TextRange left = TextRange.Empty;
                    TextRange mid = TextRange.Empty;

                    var newAssociatedItems = range.AssociatedItems.UniqueUnionWith(associatedItems).ToList();
                    var leftAssociatedItems = newAssociatedItems;
                    var midAssociatedItems = newAssociatedItems;

                    var startDiff = range.EndOffset - remaining.StartOffset;
                    var endDiff = remaining.EndOffset - range.EndOffset;

                    var nextStart = range.StartOffset;
                    if (startDiff > 0)
                    {
                        left = new TextRange(nextStart, remaining.StartOffset);
                        nextStart = left.EndOffset;
                        leftAssociatedItems = range.AssociatedItems;
                    }

                    mid = new TextRange(nextStart, range.EndOffset);
                    nextStart = mid.EndOffset;

                    if (endDiff > 0)
                    {
                        remaining = new TextRange(nextStart, remaining.EndOffset);
                    }
                    
                    var (l, m, r) = ReplaceRange(range, NewUniqueRange(left, leftAssociatedItems), NewUniqueRange(mid, midAssociatedItems), null);

                    // we may not be done yet... continue with the remainder
                    range = r ?? m ?? l;
                }
                else if (remainingEndWithinRange)
                {
                    // end offset is within... so we split in 2 pieces at the end offset... and we are done

                    TextRange left = TextRange.Empty;
                    TextRange mid = TextRange.Empty;
                    TextRange right = TextRange.Empty;

                    var newAssociatedItems = range.AssociatedItems.UniqueUnionWith(associatedItems).ToList();
                    var leftAssociatedItems = newAssociatedItems;
                    var midAssociatedItems = newAssociatedItems;
                    var rightAssociatedItems = newAssociatedItems;

                    var startDiff = range.StartOffset - remaining.StartOffset;
                    var endDiff = range.EndOffset - remaining.EndOffset;

                    var nextStart = remaining.StartOffset;
                    if (startDiff > 0)
                    {
                        left = new TextRange(nextStart, range.StartOffset);
                        nextStart = left.EndOffset;
                        leftAssociatedItems = associatedItems;
                    }

                    mid = new TextRange(nextStart, remaining.EndOffset);
                    nextStart = mid.EndOffset;

                    if (endDiff > 0)
                    {
                        right = new TextRange(nextStart, range.EndOffset);
                        rightAssociatedItems = range.AssociatedItems;
                    }
                    
                    ReplaceRange(range, NewUniqueRange(left, leftAssociatedItems), NewUniqueRange(mid, midAssociatedItems), NewUniqueRange(right, rightAssociatedItems));

                    remaining = TextRange.Empty;

                    // we are done... since it's contained
                    break;
                }

                range = range.Next;
            }

            // add a new "last range" for the remaining
            if (!remaining.IsEmpty)
            {
                var prevLast = _last;
                _last = new UniqueTextRange<T>() {
                    Range = remaining,
                    AssociatedItems = associatedItems,
                };

                _last.Prev = prevLast;
                if (prevLast != null)
                    prevLast.Next = _last;
            }
        }

        private UniqueTextRange<T> FindFirstBefore(UniqueTextRange<T> from, TextRange newRange)
        {
            UniqueTextRange<T> prev = null;

            var range = from;
            while (range != null)
            {
                if (newRange.StartOffset >= range.EndOffset)
                {
                    prev = range;
                    range = range.Next;
                    continue;
                }

                break;
            }

            return prev;
        }

        private bool ContainsEnd(UniqueTextRange<T> range, TextRange other)
        {
            return other.EndOffset > range.StartOffset && other.EndOffset <= range.EndOffset;
        }

        private bool ContainsStart(UniqueTextRange<T> range, TextRange other)
        {
            return other.StartOffset >= range.StartOffset && other.StartOffset < range.EndOffset;
        }

        private UniqueTextRange<T> NewUniqueRange(TextRange textRange, List<T> associatedItems)
        {
            if (textRange.IsEmpty) return null;

            return new UniqueTextRange<T>() {
                Range = textRange,
                AssociatedItems = associatedItems
            };
        }

        private (UniqueTextRange<T>, UniqueTextRange<T>, UniqueTextRange<T>) ReplaceRange(
            UniqueTextRange<T> range,
            UniqueTextRange<T> t1,
            UniqueTextRange<T> t2,
            UniqueTextRange<T> t3)
        {
            if (t1 == null)
            {
                t1 = t2;
                t2 = null;
            }

            if (t2 == null)
            {
                t2 = t3;
                t3 = null;
            }

            if(t1 == null) return (null, null, null);
            
            t1.Prev = range.Prev;
            if (t1.Prev != null)
                t1.Prev.Next = t1;
            else
                _first = t1;
            if (range.Next != null)
                range.Next.Prev = t1;
            else
                _last = t1;
            t1.Next = range.Next;

            if (t2 == null) return (t1, null, null);
            
            t2.Prev = t1;
            t2.Prev.Next = t2;
            if (range.Next != null)
                range.Next.Prev = t2;
            else
                _last = t2;
            t2.Next = range.Next;

            if (t3 == null) return (t1, t2, null);
            
            t3.Prev = t2;
            t3.Prev.Next = t3;
            if (range.Next != null)
                range.Next.Prev = t3;
            else
                _last = t3;
            t3.Next = range.Next;

            return (t1, t2, t3);
        }

        private UniqueTextRange<T> FindFirst(UniqueTextRange<T> from, TextRange textRange)
        {
            var range = from;

            while (range != null)
            {
                if (ContainsStart(range, textRange)) return range;
                if (ContainsEnd(range, textRange)) return range;
                range = range.Next;
            }

            return null;
        }
    }

    public class UniqueTextRange<T>
    {
        public TextRange Range { get; set; }
        public int StartOffset => Range.StartOffset;
        public int EndOffset => Range.EndOffset;
        public int Length => Range.Length;

        public List<T> AssociatedItems { get; set; }

        internal UniqueTextRange<T> Prev { get; set; }
        internal UniqueTextRange<T> Next { get; set; }

        public (TextRange, TextRange) Split(int offset)
        {
            var left = new TextRange(Range.StartOffset, offset);

            var right = new TextRange(offset, Range.EndOffset);
            
            return (left, right);
        }
        
        public override string ToString()
        {
            return Range.ToString();
        }
    }
}
