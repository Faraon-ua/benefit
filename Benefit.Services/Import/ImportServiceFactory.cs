using Benefit.Domain.Models;

namespace Benefit.Services.Import
{
    public class ImportServiceFactory
    {
        public static BaseImportService GetImportServiceInstance(SyncType importSyncType)
        {
            if (importSyncType == SyncType.Gbs)
            {
                return new GbsImportService();
            }
            if (importSyncType == SyncType.FirebirdSql)
            {
                return new FirebirdImportService();
            }
            if (importSyncType == SyncType.Yml)
            {
                return new YmlImportService();
            }
            if (importSyncType == SyncType.OneCCommerceMl)
            {
                return new OneСImportService();
            }
            if (importSyncType == SyncType.Excel)
            {
                return new ExcelImportService();
            }
            return null;
        }
    }
}
