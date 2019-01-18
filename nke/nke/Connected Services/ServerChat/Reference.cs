﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace nke.ServerChat {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServerChat.IServerChat", CallbackContract=typeof(nke.ServerChat.IServerChatCallback))]
    public interface IServerChat {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerChat/Connect", ReplyAction="http://tempuri.org/IServerChat/ConnectResponse")]
        int Connect(string Name, string Role);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerChat/Connect", ReplyAction="http://tempuri.org/IServerChat/ConnectResponse")]
        System.Threading.Tasks.Task<int> ConnectAsync(string Name, string Role);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerChat/Disconnect", ReplyAction="http://tempuri.org/IServerChat/DisconnectResponse")]
        void Disconnect(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerChat/Disconnect", ReplyAction="http://tempuri.org/IServerChat/DisconnectResponse")]
        System.Threading.Tasks.Task DisconnectAsync(int id);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServerChat/SendMsg")]
        void SendMsg(string msg, int id, string Role);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServerChat/SendMsg")]
        System.Threading.Tasks.Task SendMsgAsync(string msg, int id, string Role);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerChat/GetAuthorizedUsers", ReplyAction="http://tempuri.org/IServerChat/GetAuthorizedUsersResponse")]
        string[] GetAuthorizedUsers();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerChat/GetAuthorizedUsers", ReplyAction="http://tempuri.org/IServerChat/GetAuthorizedUsersResponse")]
        System.Threading.Tasks.Task<string[]> GetAuthorizedUsersAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServerChatCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServerChat/MsgCallBack")]
        void MsgCallBack(string msg);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServerChatChannel : nke.ServerChat.IServerChat, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServerChatClient : System.ServiceModel.DuplexClientBase<nke.ServerChat.IServerChat>, nke.ServerChat.IServerChat {
        
        public ServerChatClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public ServerChatClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public ServerChatClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ServerChatClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ServerChatClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public int Connect(string Name, string Role) {
            return base.Channel.Connect(Name, Role);
        }
        
        public System.Threading.Tasks.Task<int> ConnectAsync(string Name, string Role) {
            return base.Channel.ConnectAsync(Name, Role);
        }
        
        public void Disconnect(int id) {
            base.Channel.Disconnect(id);
        }
        
        public System.Threading.Tasks.Task DisconnectAsync(int id) {
            return base.Channel.DisconnectAsync(id);
        }
        
        public void SendMsg(string msg, int id, string Role) {
            base.Channel.SendMsg(msg, id, Role);
        }
        
        public System.Threading.Tasks.Task SendMsgAsync(string msg, int id, string Role) {
            return base.Channel.SendMsgAsync(msg, id, Role);
        }
        
        public string[] GetAuthorizedUsers() {
            return base.Channel.GetAuthorizedUsers();
        }
        
        public System.Threading.Tasks.Task<string[]> GetAuthorizedUsersAsync() {
            return base.Channel.GetAuthorizedUsersAsync();
        }
    }
}
