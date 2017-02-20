using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Benefit.CardReader.DataTransfer.Reader;
using Benefit.Common.CustomEventArgs;
using Benefit.Common.Helpers;

namespace Benefit.CardReader.Services
{
    public class ReaderService : IDisposable
    {
        private SerialPort _serialPort;
        private byte[] readSymbols;

        public  delegate void CardReadedEventHandler(object sender, NfcEventArgs e);

        public event CardReadedEventHandler CardReaded;

        public ReaderService()
        {
            readSymbols = new byte[128];
            _serialPort = new SerialPort { BaudRate = 38400, ReadTimeout = 500, WriteTimeout = 500 };
            _serialPort.DataReceived += _serialPort_DataReceived;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var sp = (SerialPort)sender;
                sp.Read(readSymbols, 0, readSymbols.Length);
                if (readSymbols[8] == 0)
                {
                    ProcessCardRead(readSymbols.ToList().GetRange(0, 8));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void CloseComPort()
        {
            _serialPort.Close();
        }
        private void ProcessCardRead(List<byte> nfcBytes)
        {
            var saltedNfc = nfcBytes.GetRange(0, 4);
            var salt = nfcBytes.GetRange(4, 4);
            var decodedNfcBytes = HexHelper.XOR(saltedNfc, salt);
            var decodedNfc = string.Join("", decodedNfcBytes.Select(entry => entry.ToString("X2")));
            SendCardReaded(decodedNfc);
        }

        public void SendCardReaded(string nfc)
        {
            if (this.CardReaded != null)
            {
                this.CardReaded(this, new NfcEventArgs(nfc));
            }
        }

        public HandShakeResult HandShake()
        {
            var result = new HandShakeResult();
            var allComPorts = SerialPort.GetPortNames();
            foreach (var comPort in allComPorts)
            {
                try
                {
                    _serialPort.PortName = comPort;
                    _serialPort.Open();

                    var prefix = CardReaderSettingsService.ReaderHandShakePrefix;
                    var authCode = StringHelper.RandomString(4);
                    var handShakeMessage = prefix + authCode;
                    _serialPort.Write(handShakeMessage);
                    do
                    {
                        Thread.Sleep(100);
                    } while (readSymbols[0] == 0);
                    var data = readSymbols.ToList();
                    var saltedAuthCode = data.GetRange(10, 4);
                    var authCodeSalt = data.GetRange(20, 4);
                    var saltedLicenseKey = data.GetRange(30, 32);
                    var licenseKeySalt = data.GetRange(70, 32);

                    //check if was encoded correctly
                    var decodedAuthKeyBytes = HexHelper.XOR(saltedAuthCode, authCodeSalt);
                    var decodedAuthKey = HexHelper.HexBytesToString(decodedAuthKeyBytes);
                    if (decodedAuthKey == authCode)
                    {
                        //get licensekey
                        var decodedLicenseBytes = HexHelper.XOR(saltedLicenseKey, licenseKeySalt);
                        result.LicenseKey = HexHelper.HexBytesToString(decodedLicenseBytes);
                        result.PortName = comPort;
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    //skip if can not connect
                    Debug.WriteLine(ex.Message);
                }
            }
            return null;
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
    }
}
