using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    // Shared table (dictionary) that multiple threads will access
    private static Dictionary<int, string> table = new Dictionary<int, string>();
    
    // Object to be used for locking
    private static readonly object tableLock = new object();

    static void Main()
    {
        // Creating multiple threads to simulate concurrent access
        Thread thread1 = new Thread(() => UpdateTable(1, "Thread 1"));
        Thread thread2 = new Thread(() => UpdateTable(2, "Thread 2"));
        Thread thread3 = new Thread(() => ReadTable(1));
        Thread thread4 = new Thread(() => ReadTable(2));

        // Start the threads
        thread1.Start();
        thread2.Start();
        thread3.Start();
        thread4.Start();

        // Wait for threads to complete
        thread1.Join();
        thread2.Join();
        thread3.Join();
        thread4.Join();

        // Display the final state of the table
        Console.WriteLine("Final table state:");
        lock (tableLock)
        {
            foreach (var item in table)
            {
                Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
            }
        }
    }

    // Method to update the table, demonstrating synchronization
    private static void UpdateTable(int key, string value)
    {
        lock (tableLock)
        {
            Console.WriteLine($"Thread updating key {key} with value {value}");
            if (table.ContainsKey(key))
            {
                table[key] = value;
            }
            else
            {
                table.Add(key, value);
            }

            // Simulate some work
            Thread.Sleep(100);
            Console.WriteLine($"Thread finished updating key {key}");
        }
    }

    // Method to read from the table, demonstrating synchronization
    private static void ReadTable(int key)
    {
        lock (tableLock)
        {
            Console.WriteLine($"Thread reading key {key}");
            if (table.ContainsKey(key))
            {
                Console.WriteLine($"Key: {key}, Value: {table[key]}");
            }
            else
            {
                Console.WriteLine($"Key {key} not found in the table");
            }

            // Simulate some work
            Thread.Sleep(100);
            Console.WriteLine($"Thread finished reading key {key}");
        }
    }
}
