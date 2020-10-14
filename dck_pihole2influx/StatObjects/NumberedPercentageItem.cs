namespace dck_pihole2influx.StatObjects
{
    public class NumberedPercentageItem : IBaseResult
    {
        public int position { get; }
        public double percentage { get; }
        public string entry { get; }

        public NumberedPercentageItem(int position, double percentage, string entry)
        {
            this.position = position;
            this.percentage = percentage;
            this.entry = entry;
        }
    }
}