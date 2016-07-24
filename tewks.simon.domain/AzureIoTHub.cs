using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Tewks.Simon.Domain;
using Newtonsoft.Json;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "SimonDevice". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=simoniot.azure-devices.net;DeviceId=SimonDevice;SharedAccessKey=<ACCESS KEY>";

    //
    // To monitor messages sent to device "SimonDevice" use iothub-explorer as follows:
    //    iothub-explorer HostName=simonhub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=Qjo46J/Mh3QuJuJ6NlEJYvR+CraGQT5lK50n2At007U= monitor-events "SimonDevice"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync(object msg)
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

        var serializedMessage = JsonConvert.SerializeObject(msg);

        var message = new Message(Encoding.ASCII.GetBytes(serializedMessage));

        await deviceClient.SendEventAsync(message);
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
