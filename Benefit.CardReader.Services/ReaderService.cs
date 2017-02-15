using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using Benefit.CardReader.DataTransfer.Reader;
using Benefit.Common.Helpers;

namespace Benefit.CardReader.Services
{
    public class ReaderService
    {
        private SerialPort _serialPort;
        private byte[] readSymbols;

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
                var sp = (SerialPort) sender;
                sp.Read(readSymbols, 0, readSymbols.Length);
            }
            catch { }
        }

        private string ParseResult()
        {
            var output = new StringBuilder();
            if (readSymbols[0] == 'T')
            {
                if (readSymbols[1] == (char)48) output.Append('+');
                else
                    if (readSymbols[1] == (char)255) output.Append('-');
                output.Append(readSymbols[2]);
                output.Append(readSymbols[3]);
                output.Append('.');
                output.Append(readSymbols[4]);
            }
            else if (readSymbols[0] == 't')
            {
                foreach (byte b in readSymbols.Skip(1))
                    output.Append((char)b);
            }
            return output.ToString();
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
                    var saltedLicenseKey = data.GetRange(30, 31);
                    var licenseKeySalt = data.GetRange(70, 31);

                    //check if was encoded correctly
                    var decodedAuthKeyBytes = HexHelper.XOR(saltedAuthCode, authCodeSalt);
                    var decodedAuthKey = HexHelper.HexBytesToString(decodedAuthKeyBytes);
                    if (decodedAuthKey == authCode)
                    {
                        //get licensekey
                        var decodedLicenseBytes = HexHelper.XOR(saltedLicenseKey, licenseKeySalt);
                        result.LicenseKey = HexHelper.HexBytesToString(decodedLicenseBytes);
                        result.PortName = comPort;
                        _serialPort.Close();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    //skip if can not connect
                    _serialPort.Close();
                    Debug.WriteLine(ex.Message);
                }
            }
            return null;
        }
    }
}
