using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

public class NetworkedServer : MonoBehaviour
{
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;

    private Dictionary<int, UserAccount> connectedUsers;

    void Start()
    {
        SetupConnection();

        connectedUsers = new Dictionary<int, UserAccount>();
    }

    void SetupConnection()
    {
        // Initialize NetworkTransport for its following usage
        NetworkTransport.Init();

        // Create a config to describe the connection channels
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable); // Guarantee of delivering but no guarantee of ordering
        unreliableChannelID = config.AddChannel(QosType.Unreliable); // No guarantee of delivering or ordering

        // Create a HostTopology which describes the default connection, number of such connections, and special types of connections (if exists)
        HostTopology topology = new HostTopology(config, maxConnections);

        // Create a host with a given topology, bind a socket to a given port id
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        Debug.Log("Host server id: " + hostID);
    }

    void Update()
    {
        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0;

        // Receive an event
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

        // Process received event
        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedRequest(msg, recConnectionID, recHostID);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                ProcessDisconnect(recConnectionID);
                break;
        }

    }
  
    public void SendClientRequest(string msg, int id)
    {
        byte error = 0;
        // Encrypt the string into a byte buffer
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        // Send an encrypted message through reliable channel
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    

    private void ProcessRecievedRequest(string msg, int id, int hostId)
    {
        Debug.Log("[CLIENT " + id +"]: " + msg);

        string[] csv = msg.Split(',');

        int requestType = int.Parse(csv[0]);

        if (requestType == ClientToServerTransferSignifiers.CreateAccount)
        {
            string login = csv[1];
            string password = csv[2];
            string email = csv[3];

            CreateAccount(id, login, password, email);
        }
        else if (requestType == ClientToServerTransferSignifiers.Login)
        {
            string login = csv[1];
            string password = csv[2];

            LoginIn(id, login, password);
        }
        else if (requestType == ClientToServerTransferSignifiers.ForgotPassword)
        {
            string login = csv[1];

            SendPasswordToEmail(id, login);
        }
        else if (requestType == ClientToServerTransferSignifiers.SendGlobalMessage)
        {
            string message = csv[1];

            ProcessGlobalMessage(id, message);
        }
        else if (requestType == ClientToServerTransferSignifiers.SendPrivateMessage)
        {
            int receiverId = int.Parse(csv[1]);
            string message = csv[2];

            ProcessPrivateMessage(id, receiverId, message);
        }
    }

    private void LoginIn(int id, string login, string password)
    {
        int accountIdx = -1;
        foreach (var idx in DataManager.Instance.indexesDict)
        {
            if (idx.Value == login)
            {
                accountIdx = idx.Key;
                break;
            }
        }

        if (accountIdx < 0)
        {
            SendClientRequest(ServerToClientTransferSignifiers.Message + "," + "Account login or password is incorrect!", id);
            return;
        }

        AccountInfo info = DataManager.Instance.GetAccountInformation(accountIdx);

        if (info.password == password)
        {
            UserAccount account = new UserAccount();
            account.login = info.login;
            account.userId = id;

            connectedUsers.Add(id, account);

            SendClientRequest(ServerToClientTransferSignifiers.Message + "," + "You are logged in!" + "," + "3.0" + "," + "2", id);
            SendClientRequest(ServerToClientTransferSignifiers.SuccessfulLogin.ToString(),id);

            foreach (var user in connectedUsers)
            {
                if (user.Key != id)
                    SendClientRequest(ServerToClientTransferSignifiers.AddUserToLocalClient + "," + user.Value.userId + ","
                        + user.Value.login, id);
            }
        }
        else
            SendClientRequest(ServerToClientTransferSignifiers.Message + "," + "Account login or password is incorrect!", id);

    }

    private void CreateAccount(int id, string login, string password, string email)
    {
        foreach (var idx in DataManager.Instance.indexesDict)
        {
            if (idx.Value == login)
            {
                SendClientRequest(ServerToClientTransferSignifiers.Message + "," + "That login is already in use! Try to sing in instead!", id);
                return;
            }
        }

        int index = DataManager.Instance.RegisterNewAccountIndex(login);

        DataManager.Instance.WriteDataToAccountFile(index, login, password, email);

        SendClientRequest(ServerToClientTransferSignifiers.Message + "," + "Your account " + login + " was created!" + "," + "3.0" + "," + "2", id);
    }

    private void SendPasswordToEmail(int id, string login)
    {
        int accountIdx = -1;
        foreach (var idx in DataManager.Instance.indexesDict)
        {
            if (idx.Value == login)
            {
                accountIdx = idx.Key;
                break;
            }
        }

        if (accountIdx < 0)
        {
            SendClientRequest(ServerToClientTransferSignifiers.Message + "," + "No such login is found!", id);
            return;
        }

        AccountInfo info = DataManager.Instance.GetAccountInformation(accountIdx);

        string passwordReminder = "Hello!\n\nThis is Delta`s Notifier. We've just got a request for your password reminder.\n\nYour password: " +
            info.password + "\n\nIf you didn`t request a password reminder, just ignore this message.\n\nRegards,\nDelta`s Notifier";

        EmailService.SendEmailMessage(info.email, "Password reminder", passwordReminder);

        SendClientRequest(ServerToClientTransferSignifiers.Message + "," + "Your password was sent to your connected email!" + "," + "4.5" + "," + "3", id);
    }

    private void ProcessGlobalMessage(int id, string message)
    {
        foreach (var user in connectedUsers)
        {
            SendClientRequest(ServerToClientTransferSignifiers.ReceiveGlobalMessage + "," + id + "," + message, user.Key);
        }
    }

    private void ProcessPrivateMessage(int id, int receiverId, string message)
    {
        if (connectedUsers.ContainsKey(receiverId))
        {
            SendClientRequest(ServerToClientTransferSignifiers.ReceivePrivateMessage + "," + id + "," + message, receiverId);
        }
    }

    private void ProcessDisconnect(int id)
    {
        if (connectedUsers.ContainsKey(id))
            connectedUsers.Remove(id);

        foreach (var user in connectedUsers)
        {
            SendClientRequest(ServerToClientTransferSignifiers.UserDisconnected + "," + id, user.Key);
        }
    }
}

public static class ClientToServerTransferSignifiers
{
    public const int CreateAccount = 1;
    public const int Login = 2;
    public const int ForgotPassword = 3;

    public const int SendGlobalMessage = 4;
    public const int SendPrivateMessage = 5;
}

public static class ServerToClientTransferSignifiers
{
    public const int Message = 1;
    public const int SuccessfulLogin = 2;

    public const int AddUserToLocalClient = 3;
    public const int UserDisconnected = 4;

    public const int ReceiveGlobalMessage = 5;
    public const int ReceivePrivateMessage = 6;
}