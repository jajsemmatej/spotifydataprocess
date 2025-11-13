using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyDataProcess
{
    class DateInterval
    {
        public DateTime Start { get; }
        public DateTime End { get; }
        public TimeSpan Duration => End - Start;
        public DateInterval(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End date must be greater than or equal to start date.");
            Start = start;
            End = end;
        }
        public bool Contains(DateTime date) =>
            date >= Start && date <= End;

        public bool Overlaps(DateInterval other) =>
            Start <= other.End && End >= other.Start;

        public long OverlapDays(DateInterval other)
        {
            if (!Overlaps(other))
                return 0;
            DateTime from = Start > other.Start ? Start : other.Start;
            DateTime to = End > other.End ? other.End : End;
            return (long)(to.Date - from.Date).TotalDays + 1;
        }
            
        public override string ToString() =>
            $"{Start:yyyy-MM-dd} – {End:yyyy-MM-dd}";
    }
}