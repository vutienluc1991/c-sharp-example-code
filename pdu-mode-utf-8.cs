using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

class Program
{
    static void Main()
    {
        string portName = "COM3"; // Replace with your COM port
        string phoneNumber = "+84912345678"; // Replace with the recipient's phone number
        string message = "Đây là tin nhắn SMS dài hơn 200 ký tự bằng tiếng Việt để kiểm tra chức năng gửi tin nhắn dài bằng chế độ PDU."; // Your UTF-8 Vietnamese text message

        try
        {
            using (SerialPort serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One))
            {
                serialPort.Open();
                Console.WriteLine("Serial port opened.");

                // Encode and send the SMS
                SendLongSms(serialPort, phoneNumber, message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void SendLongSms(SerialPort serialPort, string phoneNumber, string message)
    {
        // Convert the message to UCS2 (UTF-16 Big Endian) and then to hexadecimal
        byte[] messageBytes = Encoding.BigEndianUnicode.GetBytes(message);
        string messageHex = BitConverter.ToString(messageBytes).Replace("-", "");

        // Calculate the number of parts required (153 septets per part for UCS2, considering UDH)
        int maxSegmentLength = 134; // 67 UCS2 characters (134 hex characters)
        int numberOfParts = (int)Math.Ceiling((double)messageHex.Length / maxSegmentLength);

        // Generate unique reference number for message
        string referenceNumber = "00"; // Fixed value or generate a random value

        for (int i = 0; i < numberOfParts; i++)
        {
            int segmentStart = i * maxSegmentLength;
            int segmentLength = Math.Min(maxSegmentLength, messageHex.Length - segmentStart);
            string segment = messageHex.Substring(segmentStart, segmentLength);

            // UDH for segmented messages
            string udh = "050003" + referenceNumber + numberOfParts.ToString("X2") + (i + 1).ToString("X2");

            // Calculate PDU length
            string pdu = EncodePDU(phoneNumber, udh + segment);
            int pduLength = (pdu.Length / 2) - ((phoneNumber.Length + 1) / 2) - 3;

            // Send AT command to modem
            string command = $"AT+CMGS={pduLength}\r";
            serialPort.Write(command);
            Thread.Sleep(100); // Small delay for modem to process the command

            // Send the PDU and Ctrl+Z character (ASCII 26)
            serialPort.Write(pdu + char.ConvertFromUtf32(26));
            Thread.Sleep(5000); // Wait for the modem to send the message
            string response = serialPort.ReadExisting();
            Console.WriteLine("Response: " + response);
        }
    }

    static string EncodePDU(string phoneNumber, string udhAndMessage)
    {
        // SCA (Service Center Address) - Set to empty (00) to use default
        string sca = "00";

        // PDU-Type: 01 = SMS-SUBMIT, UDHI set (40) for long messages
        string pduType = "41";

        // Message Reference (MR)
        string mr = "00";

        // Destination Address
        string da = EncodePhoneNumber(phoneNumber);

        // Protocol Identifier (PID) - 00 (default)
        string pid = "00";

        // Data Coding Scheme (DCS) - 08 for UCS2
        string dcs = "08";

        // UDL (User Data Length)
        string udl = (udhAndMessage.Length / 2).ToString("X2");

        // Final PDU string
        return sca + pduType + mr + da + pid + dcs + udl + udhAndMessage;
    }

    static string EncodePhoneNumber(string phoneNumber)
    {
        phoneNumber = phoneNumber.TrimStart('+');
        if (phoneNumber.Length % 2 != 0)
        {
            phoneNumber += "F"; // Padding if length is odd
        }

        // Swap every two digits
        var swappedNumber = string.Concat(phoneNumber.Select((c, i) => new { c, i })
            .GroupBy(x => x.i / 2)
            .Select(g => g.Last().c + "" + g.First().c));

        // Length of phone number (half of the swapped number length) and type of address (91 for international)
        return phoneNumber.Length.ToString("X2") + "91" + swappedNumber;
    }
}
