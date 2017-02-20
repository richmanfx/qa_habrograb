namespace qa_habrograb
{
    /// Период времени - дата в стандартном формате JS (без времени)
    public class Period
    {
        public Period() {}

        public Period(string fromDate, string toDate)
        {
            this.FromDate = fromDate;
            this.ToDate = toDate;
        }

        private string FromDate { get; set; }    // Дата и начала периода.
        private string ToDate { get; set; }      // Дата окончания периода.
    }
}
