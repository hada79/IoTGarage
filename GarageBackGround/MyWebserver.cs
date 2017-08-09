using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace GarageBackGround
{
    class MyWebserver
    {
        private const string TRIGGER_PASSWORD = "aita";
        private const uint BufferSize = 8192;
        private const string HadaHubConnectionString = "HostName=HadaHouseHub.azure-devices.net;DeviceId=hada-pi3;SharedAccessKey=gIJQfuMqugiWVyAqwct8EAMW6pgbRWHzH6LzSzXEBI8=";


        static GarageDoor g;

        public async void Start()
        {

            g = new GarageDoor();
            // WEB SERVER
            //var listener = new StreamSocketListener();

            //await listener.BindServiceNameAsync("8081");

            //listener.ConnectionReceived += async (sender, args) =>
            //{
            //    var request = new StringBuilder();

            //    using (var input = args.Socket.InputStream)
            //    {
            //        var data = new byte[BufferSize];
            //        IBuffer buffer = data.AsBuffer();
            //        var dataRead = BufferSize;

            //        while (dataRead == BufferSize)
            //        {
            //            await input.ReadAsync(
            //                 buffer, BufferSize, InputStreamOptions.Partial);
            //            request.Append(Encoding.UTF8.GetString(
            //                                          data, 0, data.Length));
            //            dataRead = buffer.Length;
            //        }
            //    }

            //    string query = GetQuery(request);

            //    string doorResponse;

            //    if (query.ToLower().Equals("?" + PASSWORD))
            //    {
            //        g.TriggerDoor();
            //        doorResponse = "Door triggered.";
            //    } else
            //    {
            //        doorResponse = "Invalid API";
            //    }

            //    await SendDataToAzure(doorResponse);

            //    using (var output = args.Socket.OutputStream)
            //    {
            //        using (var response = output.AsStreamForWrite())
            //        {
            //            var html = Encoding.UTF8.GetBytes(
            //            $"<html><head><title>Background Message</title></head><body>{doorResponse}</body></html>");
            //            using (var bodyStream = new MemoryStream(html))
            //            {
            //                var header = $"HTTP/1.1 200 OK\r\nContent-Length: {bodyStream.Length}\r\nConnection: close\r\n\r\n";
            //                var headerArray = Encoding.UTF8.GetBytes(header);
            //                await response.WriteAsync(headerArray,
            //                                          0, headerArray.Length);
            //                await bodyStream.CopyToAsync(response);
            //                await response.FlushAsync();
            //            }
            //        }
            //    }
            //};



            // Azure IoT Hub
            await ReceiveDataFromAzure();

            // Night time policy

            // if open after 10 pm, sent alert
        }

        public async static Task SendMessageToAzure(string message)
        {
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(HadaHubConnectionString, TransportType.Http1);

            var msg = new Message(Encoding.UTF8.GetBytes(message));

            await deviceClient.SendEventAsync(msg);
        }        

        public async static Task ReceiveDataFromAzure()
        {
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(HadaHubConnectionString, TransportType.Http1);

            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    await deviceClient.CompleteAsync(receivedMessage);

                    await handleReceiveMessage(messageData);
                }
            }
        }

        private async static Task handleReceiveMessage(string messageData)
        {
            string returnMessage = "";

            if (messageData.Equals(TRIGGER_PASSWORD))
            {
                g.TriggerDoor();
                returnMessage = "Door triggered.";
            }           
            else
            {
                returnMessage = "Invalid message";
            }

            await SendMessageToAzure(returnMessage);
        }

        private static string GetQuery(StringBuilder request)
        {
            var requestLines = request.ToString().Split(' ');

            var url = requestLines.Length > 1
                              ? requestLines[1] : string.Empty;

            var uri = new Uri("http://localhost" + url);
            var query = uri.Query;
            return query;
        }
    }
}
