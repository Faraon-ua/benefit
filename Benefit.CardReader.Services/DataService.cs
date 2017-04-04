using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Benefit.CardReader.DataTransfer.Offline;

namespace Benefit.CardReader.Services
{
    public class DataService
    {
        private const string TokenFileName = "token.dat";
        private const string CashiersFileName = "cashiers.dat";

        private readonly Dictionary<Type, string> TypeToFileNameMapping = new Dictionary<Type, string>()
        {
            {typeof (string), TokenFileName},
            {typeof (Cashier), CashiersFileName}
        };

        public List<T> Get<T>()
        {
            var fileName = TypeToFileNameMapping[typeof(T)];
            using (Stream fileStream = File.Open(fileName, FileMode.OpenOrCreate))
            {
                if (fileStream.Length == 0)
                {
                    return new List<T>();
                }
                var encoding = new UnicodeEncoding();
                var key = encoding.GetBytes(CardReaderSettingsService.OfflineFileSalt);
                var rmCrypto = new RijndaelManaged();
                using (var cryptoStream = new CryptoStream(fileStream, rmCrypto.CreateDecryptor(key, key), CryptoStreamMode.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    var result = binaryFormatter.Deserialize(cryptoStream);
                    return (List<T>)result;
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
                var encoding = new UnicodeEncoding();
                var key = encoding.GetBytes(CardReaderSettingsService.OfflineFileSalt);
                var rmCrypto = new RijndaelManaged();
                using (var cryptoStream = new CryptoStream(fileStream, rmCrypto.CreateEncryptor(key, key), CryptoStreamMode.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(cryptoStream, collection);
                }
            }
        }
    }
}
