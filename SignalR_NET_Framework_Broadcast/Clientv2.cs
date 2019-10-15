using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace SignalR_NET_Framework_Broadcast {
    public class Clientv2 {
        public Clientv2() {
        }

        public async Task tryClient() {
            string url = "https://localhost:44336/connect";//"https://ar-sphere-server.azurewebsites.net/connect";
            var hubCtion = new HubConnectionBuilder().WithUrl(url).Build();
            
            Console.WriteLine("Attempt connection...");
            await hubCtion.StartAsync();

            Console.WriteLine("Connected at " + hubCtion.ConnectionId);
            /*
            hubCtion.On<string>("Ping", (re) => {
                Console.WriteLine("Recieved  {0} ", re);
            });
            */
            string res = hubCtion.InvokeAsync<string>("Ping", "message").Result;
            Console.WriteLine("Received after invoke: {0} ", res);
            
            Console.ReadKey();
        }
    }
}