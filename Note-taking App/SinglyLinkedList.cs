using System.Collections.Generic;
using System;

namespace NoteTakingApp.Data
{
    // Lớp chứa dữ liệu của mỗi phần tử trong danh sách liên kết đơn
    public class SinglyLinkedListNode
    {
        public NoteEntry Data;  // Dữ liệu của phần tử
        public SinglyLinkedListNode Next;  // Trỏ tới phần tử tiếp theo

        public SinglyLinkedListNode(NoteEntry data)
        {
            Data = data;
            Next = null;
        }
    }

    // Lớp danh sách liên kết đơn chứa các NoteEntry
    public class SinglyLinkedList
    {
        private SinglyLinkedListNode head;  // Đầu danh sách liên kết

        // Thêm phần tử vào danh sách
        public void Add(NoteEntry data)
        {
            var newNode = new SinglyLinkedListNode(data);
            if (head == null)
            {
                head = newNode;
            }
            else
            {
                var current = head;
                while (current.Next != null)
                    current = current.Next;
                current.Next = newNode;
            }
        }

        // Xóa phần tử theo tiêu đề
        public void Remove(string title)
        {
            if (head == null) return;

            if (head.Data.Title == title)
            {
                head = head.Next;
                return;
            }

            var current = head;
            while (current.Next != null && current.Next.Data.Title != title)
                current = current.Next;

            if (current.Next != null)
                current.Next = current.Next.Next;
        }

        // Lấy tất cả các ghi chú trong danh sách
        public List<NoteEntry> GetAllNotes()
        {
            var notes = new List<NoteEntry>();
            var current = head;
            while (current != null)
            {
                notes.Add(current.Data);
                current = current.Next;
            }
            return notes;
        }

        // Tìm ghi chú theo tiêu đề
        public NoteEntry Find(string title)
        {
            var current = head;
            while (current != null)
            {
                if (string.Equals(current.Data.Title, title, StringComparison.OrdinalIgnoreCase))
                    return current.Data;
                current = current.Next;
            }
            return null;
        }

        public void Swap(string title1, string title2)
        {
            if (title1 == title2) return;

            SinglyLinkedListNode prevNode1 = null, node1 = head;
            SinglyLinkedListNode prevNode2 = null, node2 = head;

            // Tìm node 1
            while (node1 != null && node1.Data.Title != title1)
            {
                prevNode1 = node1;
                node1 = node1.Next;
            }

            // Tìm node 2
            while (node2 != null && node2.Data.Title != title2)
            {
                prevNode2 = node2;
                node2 = node2.Next;
            }

            if (node1 == null || node2 == null)
            {
                throw new InvalidOperationException("One or both notes not found.");
            }

            // Hoán đổi các node
            if (prevNode1 == null)
            {
                head = node2;
            }
            else
            {
                prevNode1.Next = node2;
            }

            if (prevNode2 == null)
            {
                head = node1;
            }
            else
            {
                prevNode2.Next = node1;
            }

            // Hoán đổi các node tiếp theo
            SinglyLinkedListNode temp = node1.Next;
            node1.Next = node2.Next;
            node2.Next = temp;
        }


        // Kiểm tra danh sách có chứa ghi chú với tiêu đề đã cho không
        public bool Contains(string title)
        {
            var current = head;
            while (current != null)
            {
                if (current.Data.Title == title)
                {
                    return true;
                }
                current = current.Next;
            }
            return false;
        }
    }
}
