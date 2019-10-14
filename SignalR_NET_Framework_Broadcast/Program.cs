using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR_NET_Framework_Broadcast {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Attempt to startup");

            string url = "http://localhost:8080";
            using (WebApp.Start(url)) {
                ObjectHub hub = new ObjectHub();
                Console.WriteLine("Server running on {0}", url);

                tryClient().Wait();
            }
        }

        public static /*void*/ async Task tryClient() {
            Console.WriteLine("Attempt client connection");
            var hubConnection = new HubConnection("http://localhost:8080");
            
            hubConnection.TraceLevel = TraceLevels.All;
            hubConnection.TraceWriter = Console.Out;

            IHubProxy proxy = hubConnection.CreateHubProxy("ObjectHub");

            proxy.On("sendIntObj", (res) => {
                Console.WriteLine("Incoming data: {0}", res);
                //throw new ArgumentNullException();
            });

//            System.Net.ServicePointManager.DefaultConnectionLimit = 10;

            Console.WriteLine("\tStarting client connection...");
            await hubConnection.Start();
        }
    }

    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.MapSignalR();
        }

    }

    public class ObjectHub : Hub {
        Int16 intObj = 0;

        [HubMethodName("sendIntObj")] //use this to start connection

        public string SendIntObj() { //server sends int obj
            Console.WriteLine("Request int object");

            string connectionstring = "Data Source=DESKTOP-G59947P\\SQLEXPRESS;Initial Catalog=dbo;Integrated Security=True";

            using (var connection = new SqlConnection(connectionstring)) {//ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString)) {
                //string query = "SELECT  NewMessageCount, NewCircleRequestCount, NewNotificationsCount, NewJobNotificationsCount FROM [dbo].[Modeling_NewMessageNotificationCount] WHERE UserProfileId=" + "61764";
                string query = "SELECT IntObj FROM [dbo].[Model_Test] WHERE UserID=" + "3";
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection)) {
                    command.Notification = null;
                    DataTable dt = new DataTable();
                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);
                    if (connection.State == System.Data.ConnectionState.Closed)
                        connection.Open();

                    var reader = command.ExecuteReader();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0) {
                        intObj = Int16.Parse(dt.Rows[0]["IntObj"].ToString());
                    }

                }

            }

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ObjectHub>();

            return context.Clients.All.RecieveNotification(intObj);

        }

        private void dependency_OnChange(object sender, SqlNotificationEventArgs e) {
            if (e.Type == SqlNotificationType.Change) {
                ObjectHub iHub = new ObjectHub();
                iHub.SendIntObj(); //???
            }
        }
    }
}
