using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Offline;

namespace Benefit.CardReader.Services
{
    public class DataService
    {
        private const string CashiersFileName = "cashiers.dat";

        private readonly Dictionary<Type, string> TypeToFileNameMapping = new Dictionary<Type, string>()
        {
            {typeof (Cashier), CashiersFileName}
        };

        private List<T> Get<T>()
        {
            var fileName = TypeToFileNameMapping[typeof(T)];
            using (Stream fileStream = File.Open(fileName, FileMode.OpenOrCreate))
            {
                var cryptic = new DESCryptoServiceProvider
                {
                    Key = Encoding.ASCII.GetBytes(CardReaderSettingsService.OfflineFileSalt),
                    IV = Encoding.ASCII.GetBytes(CardReaderSettingsService.OfflineFileSalt)
                };

                using (var cryptoStream = new CryptoStream(fileStream, cryptic.CreateEncryptor(), CryptoStreamMode.Read))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    var result = bformatter.Deserialize(cryptoStream);
                    return (List<T>) result;
                }
            }
        }
        public void Add<T>(T item)
        {
            var fileName = TypeToFileNameMapping[typeof(T)];
            var collection = Get<T>();
            if (!collection.Contains(item))
            {
                collection.Add(item);
            }
            //serialize
            using (Stream fileStream = File.Open(fileName, FileMode.Create))
            {
                var cryptic = new DESCryptoServiceProvider
                {
                    Key = Encoding.ASCII.GetBytes(CardReaderSettingsService.OfflineFileSalt),
                    IV = Encoding.ASCII.GetBytes(CardReaderSettingsService.OfflineFileSalt)
                };

                using (var cryptoStream = new CryptoStream(fileStream, cryptic.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bformatter.Serialize(fileStream, collection);
//                    cryptoStream.FlushFinalBlock();
                }
            }
        }
    }
}
