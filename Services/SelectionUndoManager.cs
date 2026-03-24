using SelectionAggregate.Models;
using System.Collections.Generic;
using System.Linq;

namespace SelectionAggregate.Services
{
    // Review this
    public class SelectionUndoManager
    {
        private readonly Stack<SelectionSnapshot> _undoStack = new Stack<SelectionSnapshot>();

        public bool CanUndo => _undoStack.Count > 0;

        public void Push(IEnumerable<long> elementIds)
        {
            if (elementIds == null) return;

            var ids = elementIds.ToList();
            if (ids.Count == 0) return;

            if (_undoStack.Count > 0)
            {
                var top = _undoStack.Peek();
                if (top.ElementIds.OrderBy(x => x).SequenceEqual(ids.OrderBy(x => x)))
                    return;
            }

            _undoStack.Push(new SelectionSnapshot
            {
                ElementIds = ids
            });
        }

        public SelectionSnapshot Pop()
        {
            if (_undoStack.Count == 0) return null;
            return _undoStack.Pop();
        }

        public void Clear()
        {
            _undoStack.Clear();
        }
    }
}
