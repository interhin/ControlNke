using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Server
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(IServerChatCallBack))]
    public interface IServerChat
    {

        [OperationContract]
        int Connect(string Name,string Role);

        [OperationContract]
        void Disconnect(int id);

        [OperationContract(IsOneWay = true)]
        void SendMsg(string msg, int id, string Role);

        [OperationContract]
        List<string> GetAuthorizedUsers();



    }

    public interface IServerChatCallBack
    {
        [OperationContract(IsOneWay =true)]
        void MsgCallBack(string msg);
    }

    
}
