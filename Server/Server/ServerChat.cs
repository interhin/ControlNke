using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class ServerChat : IServerChat
    {

        List<UserClass> Users = new List<UserClass>();
        int curID = 1;

        public int Connect(string Name,string _Role)
        {
            UserClass userClass = new UserClass()
            {
                ID = curID,
                UserName = Name,
                operationContext = OperationContext.Current,
                Role = _Role
            };
            curID++;

            SendMsg(userClass.UserName + " - Подключился", 0,userClass.Role);
            Users.Add(userClass);
            return userClass.ID;
        }

        public void Disconnect(int id)
        {
            var user = Users.FirstOrDefault(x => x.ID == id);
            if (user != null)
            {
                Users.Remove(user);
                SendMsg(user.UserName + " - Покинул чат", 0,user.Role);
            }
        }

        public List<string> GetAuthorizedUsers()
        {
            List<string> authorizedUsers = new List<string>();
            foreach (var u in Users)
                authorizedUsers.Add(u.UserName);
            return authorizedUsers;
        }

        public void SendMsg(string msg, int id, string _Role) // _Role переменная чтобы знать кому отправить информацию о том кто коннектнулся или дисконнектнулся
        {
            var User = Users.FirstOrDefault(x => x.ID == id);

            string answer = DateTime.Now.ToShortTimeString() + " | ";

            if (User != null)
                answer += $"{User.UserName} ({User.Role}): ";


            bool isPrivate = false;

            // Список пользователей которым нужно отправить сообщение
            List<string> usersToSend = new List<string>();
            if (msg.Contains("@"))
            {
                // Добавляем самого себя
                usersToSend.Add(User.UserName);
                var matches = Regex.Matches(msg, @"@.[\S]*"); // Ищем всех получателей по паттерну
                foreach (var m in matches) // Цикл по всем получателям
                {
                    string userLogin = m.ToString().Replace("@", string.Empty); // Убираем из логина @
                    var isset = Users.FirstOrDefault(x => x.UserName == userLogin); // Смотрим авторизован ли такой пользователь в чате
                    Console.WriteLine(userLogin);
                    if (isset != null)
                        usersToSend.Add(isset.UserName); // Если авторизован то добавляем в список
                }
                if (usersToSend.Count > 1) // Если в списке есть кто-то кроме нас то отправляем
                    isPrivate = true;

            }
            if (isPrivate)
            {
                msg = Regex.Replace(msg, @"@.[\S]*\s", string.Empty); // Чистим сообщение от получателей
                answer += msg;
                foreach (var us in usersToSend)
                {
                    var isset = Users.First(x => x.UserName == us);
                    isset.operationContext.GetCallbackChannel<IServerChatCallBack>().MsgCallBack(answer); // Отправляем сообщение
                }
            }
            else
            {
                answer += msg;
                foreach (var user in Users) // Тут говно код ( можно упростить )
                {

                    //Message from Student
                    if (User != null && User.Role == "Student" && (user.Role == "Student" || user.Role == "Admin"))
                        user.operationContext.GetCallbackChannel<IServerChatCallBack>().MsgCallBack(answer);

                    //Student Disconnect / Connect
                    if (User == null && _Role == "Student" && (user.Role == "Student" || user.Role == "Admin"))
                        user.operationContext.GetCallbackChannel<IServerChatCallBack>().MsgCallBack(answer);

                    //Message from Teacher
                    if (User != null && User.Role == "Teacher" && (user.Role == "Teacher" || user.Role == "Admin"))
                        user.operationContext.GetCallbackChannel<IServerChatCallBack>().MsgCallBack(answer);

                    //Teacher Disconnect / Connect
                    if (User == null && _Role == "Teacher" && (user.Role == "Teacher" || user.Role == "Admin"))
                        user.operationContext.GetCallbackChannel<IServerChatCallBack>().MsgCallBack(answer);

                    //Message from Admin
                    if (User != null && User.Role == "Admin")
                        user.operationContext.GetCallbackChannel<IServerChatCallBack>().MsgCallBack(answer);

                    //Message from Server
                    if (User == null && _Role == null)
                        user.operationContext.GetCallbackChannel<IServerChatCallBack>().MsgCallBack(answer);
                }
            }
        }
    }
}
