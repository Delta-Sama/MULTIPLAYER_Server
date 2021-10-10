using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using System.Net.Mail;
using System.Net;

public class NetworkedServer : MonoBehaviour
{
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;

    // Start is called before the first frame update
    void Start()
    {
        SetupConnection();

        /*SendEmailMessage("Fernando.Restituto@georgebrown.ca",
            "Assignment2 Notifier",
            "Hello there!\n\nThis is Maxim, and this email is sent from unity. The code is attached bellow!\n\nRegards,\nMaxim Dobrivskiy\n101290100",
            new string[] { "C://Users//Maxim//Desktop//SmtpCode.txt" });*/
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

    void SendEmailMessage(string recipientAddress, string subject, string passedMessage, string[] attachments = null)
    {
        MailAddress Notifier = new MailAddress("deltas.notifier@gmail.com", "Delta's Notifier");
        MailAddress Recipient = new MailAddress(recipientAddress);

        MailMessage message = new MailMessage(Notifier, Recipient);
        message.Subject = subject;
        message.Body = passedMessage;

        if (attachments != null && attachments.Length > 0)
        {
            foreach (string attachment in attachments)
            {
                message.Attachments.Add(new Attachment(attachment));
            }
        }

        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.Credentials = new NetworkCredential("deltas.notifier@gmail.com","Rimskogo-Korsakova1kv19");
        smtp.EnableSsl = true;
        smtp.Send(message);
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
                // Decrypt a byte buffer to a string
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedRequest(msg, recConnectionID, recHostID);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                break;
        }

    }
  
    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        // Encrypt the string into a byte buffer
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        // Send an encrypted message through reliable channel
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    

    private void ProcessRecievedRequest(string msg, int id, int hostId)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id + ", hostId: " + hostId);
        string[] csv = msg.Split(',');

        int requestType = int.Parse(csv[0]);

        if (requestType == ClientToServerTransferSignifiers.CreateAccount)
        {
            string login = csv[1];
            string password = csv[2];
            string email = csv[3];

            CreateAccount(id, login, password, email);
        }

        SendMessageToClient("Received your message, boi!", id);
    }

    private void CreateAccount(int id, string login, string password, string email)
    {
        foreach (var idx in DataManager.Instance.indexesDict)
        {
            if (idx.Value == login)
            {
                SendMessageToClient(ServerToClientTransferSignifiers.Message + "That login is already in use! Try to sing in instead!", id);
                return;
            }
        }

        int index = DataManager.Instance.RegisterNewAccountIndex(login);

        DataManager.Instance.WriteDataToAccountFile(index, login, password, email);
    }

    
}

public static class ClientToServerTransferSignifiers
{
    public const int CreateAccount = 1;
    public const int Login = 2;

}

public static class ServerToClientTransferSignifiers
{
    public const int Message = 1;

}