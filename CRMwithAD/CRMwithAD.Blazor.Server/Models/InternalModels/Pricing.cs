namespace CRMwithAD.Blazor.Server.Models.InternalModels
{
    public class pricingMeta
    {
        public int offset { get; set; }
        public int max { get; set; }
        public int size { get; set; }
        public int total { get; set; }
    }

    public class Price
    {

        public int id { get; set; }
        public bool active { get; set; }
        public string priceType { get; set; }
        public string priceUnit { get; set; }
        public decimal price { get; set; }
        public string currency { get; set; }

        //public int id { get; set; }
        //public string name { get; set; }
        //public string code { get; set; }
        //public bool active { get; set; }
        //public string priceType { get; set; }
        //public string priceUnit { get; set; }
        //public string additionalPriceUnit { get; set; }
        //public double price { get; set; }
        //public double customPrice { get; set; }
        //public object markupType { get; set; }
        //public double markup { get; set; }
        //public double markupPercent { get; set; }
        //public double cost { get; set; }
        //public string currency { get; set; }
        //public string incurCharges { get; set; }
        //public object platform { get; set; }
        //public object software { get; set; }
        ////public VolumeType volumeType { get; set; }
        //public object datastore { get; set; }
        //public object account { get; set; }
    }

    public class Pricing
    {
        public List<Price> prices { get; set; }
        public Meta meta { get; set; }
    }

    //public class VolumeType
    //{
    //    public int id { get; set; }
    //    public string code { get; set; }
    //    public string name { get; set; }
    //}
}
