namespace Api.Back.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetMondayOfWeek(this DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

            return date.AddDays(-1 * diff).Date;
        }
    }
}