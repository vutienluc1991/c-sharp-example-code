using System;
using System.IO.Ports;
using System.Threading;

class Program
{
    static void Main()
    {
        // Set up the serial port
        SerialPort serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
        serialPort.ReadTimeout = 500;
        serialPort.WriteTimeout = 500;
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;

        try
        {
            serialPort.Open();

            // Connect to SIM800L
            Console.WriteLine("Connecting to SIM800L...");
            Thread.Sleep(2000); // Wait for the module to initialize

            // Check provider
            Console.WriteLine("Checking network provider...");
            string networkProvider = GetNetworkProvider(serialPort);
            Console.WriteLine($"Network Provider: {networkProvider}");

            // Send SMS in PDU mode
            string phoneNumber = "+1234567890"; // Replace with the recipient's phone number
            string message = "Hello from SIM800L!"; // The message to send
            SendSmsPdu(serialPort, phoneNumber, message);

            Console.WriteLine("SMS sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }

    static string GetNetworkProvider(SerialPort port)
    {
        port.WriteLine("AT+CSQ"); // Signal quality command
        Thread.Sleep(1000);
        string response = port.ReadExisting();
        return response;
    }

    static void SendSmsPdu(SerialPort port, string phoneNumber, string message)
    {
        // Prepare PDU encoded message
        string pduMessage = EncodePdu(phoneNumber, message);

        port.WriteLine("AT+CMGF=0"); // Set SMS mode to PDU
        Thread.Sleep(1000);
        port.WriteLine($"AT+CMGS={pduMessage.Length / 2 + 2}"); // Send SMS
        Thread.Sleep(1000);
        port.Write(pduMessage + "\x1A"); // Send the PDU encoded message followed by Ctrl+Z
        Thread.Sleep(2000);
    }

    static string EncodePdu(string phoneNumber, string message)
    {
        // Encode phone number to PDU format
        string encodedPhoneNumber = EncodePhoneNumber(phoneNumber);

        // Encode message to PDU format
        string encodedMessage = EncodeMessage(message);

        // Construct the full PDU message
        string pdu = "001100" + encodedPhoneNumber + "00" + encodedMessage;

        return pdu;
    }

    static string EncodePhoneNumber(string phoneNumber)
    {
        // Remove '+' and convert phone number to PDU format
        phoneNumber = phoneNumber.Replace("+", "");
        string pdu = phoneNumber.Length.ToString("X2") + "91";
        for (int i = 0; i < phoneNumber.Length; i += 2)
        {
            if (i + 1 < phoneNumber.Length)
                pdu += phoneNumber[i + 1] + phoneNumber[i];
            else
                pdu += phoneNumber[i] + "F";
        }
        return pdu;
    }

    static string EncodeMessage(string message)
    {
        // Encode message in hex format (simply for demonstration, this might need adjustment)
        string hexMessage = BitConverter.ToString(System.Text.Encoding.ASCII.GetBytes(message)).Replace("-", "");
        string pdu = hexMessage.Length.ToString("X2") + hexMessage;
        return pdu;
    }
}
