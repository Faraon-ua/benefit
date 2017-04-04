namespace Benefit.CardReader.Services.Factories
{
    public class ReaderFactory
    {
        private static ApiService ApiService { get; set; }
        private static OfflineService OfflineService { get; set; }
        public static IReaderManager GetReaderManager(bool isConnected)
        {
            if (isConnected)
            {
                return ApiService ?? (ApiService = new ApiService());
            }
            return OfflineService ?? (OfflineService= new OfflineService());
        }
    }
}
