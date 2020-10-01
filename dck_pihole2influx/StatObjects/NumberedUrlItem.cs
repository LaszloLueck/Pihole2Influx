namespace dck_pihole2influx.StatObjects
{
    public class NumberedUrlItem
    {
        public int position { get; }
        public int count { get; }
        public string url { get; }


        public NumberedUrlItem(int position, int count, string url)
        {
            this.position = position;
            this.count = count;
            this.url = url;
        }
    }
}