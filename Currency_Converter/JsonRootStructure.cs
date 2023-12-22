namespace Currency_Converter
{
    public class JsonRootStructure
    {
        //public string disclaimer { get; set; }
        public string license { get; set; }
        public int timestamp { get; set; }
        //public string @base { get; set; }
        public Rates rates { get; set; }
    }

    public class Rates
    {
        public double AED { get; set; }
        public double AUD { get; set; }
        public double BGN { get; set; }
        public double BTC { get; set; }
        public double BYN { get; set; }
        public double BYR { get; set; }
        public double CAD { get; set; }
        public double CNH { get; set; }
        public double CZK { get; set; }
        public double EUR { get; set; }
        public double GBP { get; set; }
        public double HKD { get; set; }
        public double INR { get; set; }
        public double JPY { get; set; }
        public double KPW { get; set; }
        public double KRW { get; set; }
        public double KZT { get; set; }
        public double PLN { get; set; }
        public double RUB { get; set; }
        public double THB { get; set; }
        public double TRY { get; set; }
        public double UAH { get; set; }
        public double USD { get; set; }
    }
}
