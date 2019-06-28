// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace simulated_device
{
    class SimulatedDevice
    {
        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        // TODO: Use per-device connection strings instead of using "mftest" for everything
        private readonly static string s_connectionString = "This key is available for interested customers, contact us at Aqua Robur";

        // Async method to send simulated telemetry
        private static async void SendDeviceToCloudMessagesAsync(string deviceID, ushort intervalSecs, short valueOffset)
        {
            Console.WriteLine($"DeviceID:\t{deviceID}");
            Console.WriteLine($"Interval:\t{intervalSecs} seconds");
            Console.WriteLine($"Value offset:\t{valueOffset}");
            // Initial telemetry values
            Random rand = new Random();

            while (true)
            {
                double value = rand.NextDouble() * 15 + valueOffset;
                /* Message should look like this
                {
                   Type = "Sensor",
                   Ts = messageTime,
                   Device_id = deviceId,
                   Io_id = sensorNumber,
                   Raw_value = sensorValue,
                   Value = transformedValue
                }
            */

                // Create JSON message
                var telemetryDataPoint = new
                {
                    Type = "Sensor",
                    Ts = DateTime.Now.ToUniversalTime(),
                    Device_id = deviceID,
                    Io_id = 0,
                    Raw_value = value,
                    Value = value
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Send the telemetry message
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                await s_deviceClient.SendEventAsync(message);
                await Task.Delay(1000*intervalSecs);
            }
        }
        private static void Main(string[] args)
        {
            Console.WriteLine("Simulated device. Press enter to exit.\n");
            string deviceID = "simulated";
            UInt16 intervalSecs = 1;
            Int16 valueOffset = 0;
            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
            if (args.Length > 0)
            {
                deviceID = args[0];
            }
            if (args.Length > 1)
            {
                UInt16.TryParse(args[1], out intervalSecs);
            }
            if (args.Length > 2)
            {
                Int16.TryParse(args[2], out valueOffset);
            }
            SendDeviceToCloudMessagesAsync(deviceID, intervalSecs, valueOffset);
            Console.ReadLine();
        }
    }
}
