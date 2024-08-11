using System;
using System.IO.Ports;
using System.Threading;

class Program
{
    static void Main()
    {
        // Configure the serial port
        SerialPort serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
        
        try
        {
            // Open the serial port
            serialPort.Open();
            serialPort.ReadTimeout = 5000;

            // Store phone number in slot 1
            string phoneNumber = "+1234567890"; // Replace with the actual phone number
            string storeCommand = $"AT+CPBW=1,\"{phoneNumber}\"\r";
            serialPort.WriteLine(storeCommand);
            Console.WriteLine("Storing phone number...");

            // Give some time for the command to execute
            Thread.Sleep(2000);

            // Read the phone number from slot 1
            string readCommand = "AT+CPBR=1\r";
            serialPort.WriteLine(readCommand);
            Console.WriteLine("Reading phone number...");

            // Read the response from the SIM800L
            string response = serialPort.ReadExisting();
            Console.WriteLine("Response: " + response);

            // Check if the response contains the phone number
            if (response.Contains(phoneNumber))
            {
                Console.WriteLine("Phone number successfully stored and read.");
            }
            else
            {
                Console.WriteLine("Failed to store/read the phone number.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            // Close the serial port
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
    }
}
