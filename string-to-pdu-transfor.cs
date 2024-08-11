using System;
using System.Text;

public class PduEncoder
{
    public static string EncodeToPdu(string phoneNumber, string message)
    {
        string pduString = "";

        // Convert phone number to international format if not already in that format
        string internationalPhoneNumber = ConvertToInternationalFormat(phoneNumber);

        // Encode phone number
        string encodedPhoneNumber = EncodePhoneNumber(internationalPhoneNumber);
        string phoneNumberLength = (encodedPhoneNumber.Length / 2).ToString("X");

        // Encode message
        string encodedMessage = EncodeMessage(message);

        // Assemble PDU
        pduString = phoneNumberLength + "91" + encodedPhoneNumber + "0000" + encodedMessage;

        return pduString;
    }

    private static string ConvertToInternationalFormat(string phoneNumber)
    {
        // Remove spaces, dashes, and other non-numeric characters
        phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (phoneNumber.Length > 0 && phoneNumber[0] == '0')
        {
            // Remove leading zero and add international prefix (assuming +1 for US)
            phoneNumber = "1" + phoneNumber.Substring(1);
        }

        return phoneNumber;
    }

    private static string EncodePhoneNumber(string phoneNumber)
    {
        StringBuilder encoded = new StringBuilder();
        for (int i = 0; i < phoneNumber.Length; i += 2)
        {
            if (i + 1 < phoneNumber.Length)
            {
                encoded.Append(phoneNumber[i + 1]);
                encoded.Append(phoneNumber[i]);
            }
            else
            {
                encoded.Append(phoneNumber[i]);
                encoded.Append('F'); // Pad with 'F' if the length is odd
            }
        }
        return encoded.ToString();
    }

    private static string EncodeMessage(string message)
    {
        // Convert message to GSM 7-bit encoding (basic version, more complex handling may be needed)
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        StringBuilder encoded = new StringBuilder(messageBytes.Length * 2);
        foreach (byte b in messageBytes)
        {
            encoded.Append(b.ToString("X2"));
        }
        return encoded.ToString();
    }
}

class Program
{
    static void Main()
    {
        string phoneNumber = "+1234567890";
        string message = "Hello, world!";
        string pduString = PduEncoder.EncodeToPdu(phoneNumber, message);
        Console.WriteLine("PDU String: " + pduString);
    }
}
