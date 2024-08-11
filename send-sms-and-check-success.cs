using System;
using System.IO.Ports;
using System.Threading;

class Program
{
    static void Main()
    {
        string portName = "COM3"; // Replace with your COM port
        string phoneNumber = "+84912345678"; // Replace with the recipient's phone number
        string message = "Hello, this is a test SMS from SIM800L."; // The message to be sent

        try
        {
            using (SerialPort serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One))
            {
                serialPort.Open();
                Console.WriteLine("Serial port opened.");

                // Initialize the SIM800L
                if (InitializeModem(serialPort))
                {
                    Console.WriteLine("SIM800L initialized successfully.");

                    // Send the SMS and check if it was successful
                    bool smsSent = SendSmsAndCheckResponse(serialPort, phoneNumber, message);

                    if (smsSent)
                    {
                        Console.WriteLine("SMS sent successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to send SMS.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to initialize SIM800L.");
                }

                serialPort.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static bool InitializeModem(SerialPort serialPort)
    {
        // Send AT command to check if the module is responding
        serialPort.WriteLine("AT\r");
        Thread.Sleep(500);
        string response = serialPort.ReadExisting();

        if (response.Contains("OK"))
        {
            // Set SMS mode to text
            serialPort.WriteLine("AT+CMGF=1\r");
            Thread.Sleep(500);
            response = serialPort.ReadExisting();
            return response.Contains("OK");
        }

        return false;
    }

    static bool SendSmsAndCheckResponse(SerialPort serialPort, string phoneNumber, string message)
    {
        try
        {
            // Set the recipient phone number
            serialPort.WriteLine($"AT+CMGS=\"{phoneNumber}\"\r");
            Thread.Sleep(1000);

            // Send the SMS content
            serialPort.WriteLine($"{message}\x1A"); // \x1A is Ctrl+Z to send the message
            Thread.Sleep(5000); // Wait for the SMS to be sent

            // Read the response
            string response = serialPort.ReadExisting();
            Console.WriteLine("Modem Response: " + response);

            // Check if the response contains "+CMGS" which indicates the SMS was sent successfully
            if (response.Contains("+CMGS"))
            {
                return true;
            }
            else if (response.Contains("ERROR"))
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending SMS: " + ex.Message);
        }

        return false;
    }
}
