using System;
using System.IO.Ports;
using System.Threading;

class Program
{
    private static SerialPort serialPort;

    static void Main(string[] args)
    {
        // Initialize serial port
        serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
        serialPort.ReadTimeout = 500;
        serialPort.WriteTimeout = 500;

        try
        {
            // Open the serial port
            serialPort.Open();

            // Initialize SIM800L
            InitializeSIM800L();

            // Example of sending an SMS in PDU mode
            string pduMessage = "0011000B911234567890F0000B9123456789000B913456789000"; // Replace with your PDU message
            SendSMSInPDU(pduMessage);

            // Read response from SIM800L
            string response = ReadResponse();
            Console.WriteLine(response);

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            // Close the serial port
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }

    private static void InitializeSIM800L()
    {
        SendCommand("AT"); // Test command to check if module is responsive
        SendCommand("AT+CMGF=0"); // Set to PDU mode
        SendCommand("AT+CSCA=\"+1234567890\""); // Set SMS message center address (replace with actual number)
    }

    private static void SendSMSInPDU(string pduMessage)
    {
        // Send the PDU message to the module
        SendCommand($"AT+CMGS={pduMessage.Length / 2}");
        SendCommand(pduMessage + char.ConvertFromUtf32(26)); // Append CTRL+Z to indicate end of message
    }

    private static void SendCommand(string command)
    {
        // Send command to SIM800L
        serialPort.WriteLine(command);
        Thread.Sleep(500); // Wait for command to be processed
    }

    private static string ReadResponse()
    {
        // Read response from SIM800L
        try
        {
            return serialPort.ReadExisting();
        }
        catch (TimeoutException)
        {
            return "No response or timeout";
        }
    }
}
