using System;
using System.Collections;
using BlackBook.Models;

namespace BlackBook.Helpers {
    public class ObjectiveSortComparer : IComparer {
        // Sort order buckets (ascending):
        // 1) Emergency
        // 2) Important
        // 3) Medium
        // 4) Easy
        // 5) Waiting
        // 6) Delegated
        // 7) Follow-up date expired (waiting/delegated with follow-up < today)
        // 8) Deadline expired (due date < today) — supersedes follow-up expiry
        // 9) Done
        // 10) Cancelled
        // Within each bucket: by earliest DueDate first, then by Created (oldest first)
        private static int GroupKey(Objective o) {
            var today = DateTime.Today;

            // 9) Done
            if (o.Importance == Objective.Priority.Done) return 9;

            // 10) Cancelled
            if (o.Importance == Objective.Priority.Cancelled) return 10;

            // 8) Deadline expired supersedes follow-up state (only if not Done/Cancelled)
            if (o.DueDate is DateTime due && due.Date < today) return 8;

            // 7) Follow-up expired (only for waiting or delegated and not overdue)
            if ((o.Waiting || o.Importance == Objective.Priority.Delegated)
                && o.FollowUp is DateTime fu && fu.Date < today) return 7;

            // Map current priority to buckets 1..6
            return o.Importance switch {
                Objective.Priority.Emergency => 1,
                Objective.Priority.Important => 2,
                Objective.Priority.Medium => 3,
                Objective.Priority.Easy => 4,
                Objective.Priority.Waiting => 5,
                Objective.Priority.Delegated => 6,
                _ => 6 // default to Delegated bucket for any unforeseen values
            };
        }

        public int Compare(object? x, object? y) {
            if (ReferenceEquals(x, y)) return 0;
            if (x is not Objective a) return -1;
            if (y is not Objective b) return 1;

            var g1 = GroupKey(a);
            var g2 = GroupKey(b);
            var byGroup = g1.CompareTo(g2);
            if (byGroup != 0) return byGroup;

            // Within same bucket: earliest deadline first (nulls last)
            var d1 = a.DueDate;
            var d2 = b.DueDate;
            if (d1 is null && d2 is null) {
                // Fallback: created date oldest first
                return DateTime.Compare(a.Created, b.Created);
            }
            if (d1 is null) return 1; // nulls last
            if (d2 is null) return -1;

            var dateOnly1 = d1.Value.Date;
            var dateOnly2 = d2.Value.Date;
            var byDue = DateTime.Compare(dateOnly1, dateOnly2);
            if (byDue != 0) return byDue;

            // Tiebreaker: created date oldest first
            return DateTime.Compare(a.Created, b.Created);
        }
    }
}