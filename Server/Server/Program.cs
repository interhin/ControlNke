using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            ServiceHost host = new ServiceHost(typeof(ServerChat));

            foreach (ChannelDispatcher channelDipsatcher in host.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpointDispatcher in channelDipsatcher.Endpoints)
                {

                    endpointDispatcher.DispatchRuntime.AutomaticInputSessionShutdown = false;
                }
            }

            host.Open();
            Console.WriteLine("Сервер запущен!");
            Console.ReadLine();
            host.Close();
            
        }
    }
}
