namespace Benefit.DataTransfer.JSON
{
    public class ValueData
    {
        public string value { get; set; }
        public object data { get; set; }
    }
    public class AutocompleteSearch
    {
        public string query { get; set; }
        public ValueData[] suggestions { get; set; }
    }
}
