using System.ComponentModel.DataAnnotations.Schema;

namespace Ricebird.Framework.Database.Structures
{
    [ComplexType]
    public class DateRange
    {
        public DateTime BeginOn
        {
            get; set;
        }

        public DateTime EndOn
        {
            get; set;
        }

        public DateRange()
        {
            BeginOn = DateTime.Now;
            EndOn = DateTime.Now.AddDays(1);
        }
        public DateRange(DateTime begin, DateTime end)
        {
            if (begin > end)
            {
                (begin, end) = (end, begin);
            }

            BeginOn = begin;
            EndOn = end;
        }

        public void Deconstruct(out DateTime beginOn, out DateTime endOn)
        {
            beginOn = BeginOn;
            endOn = EndOn;
        }

        public static bool TryPase(string str, out DateRange dateRange)
        {
            string[] range = str.Split('-', StringSplitOptions.TrimEntries);
            if (range.Length != 2)
            {
                dateRange = new DateRange();
                return false;
            }

            if (!TryPaseFromUnixMillis(range[0], out DateTime beginOn) || !TryPaseFromUnixMillis(range[1], out DateTime endOn))
            {
                dateRange = new DateRange();
                return false;
            }

            dateRange = new DateRange(beginOn, endOn);
            return true;
        }

        public static implicit operator DateRange(string str)
        {
            if (TryPase(str, out DateRange dateRange))
            {
                return dateRange;
            }

            return new DateRange();
        }

        public static implicit operator string(DateRange range)
        {
            return $"{range.BeginOn.ToUnixMillis()}-{range.EndOn.ToUnixMillis()}";
        }
    }
}
