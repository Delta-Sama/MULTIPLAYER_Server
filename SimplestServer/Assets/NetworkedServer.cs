using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

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

    // Update is called once per frame
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
                ProcessRecievedMsg(msg, recConnectionID, recHostID);
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
    
    private void ProcessRecievedMsg(string msg, int id, int hostId)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id + ", hostId: " + hostId);
        SendMessageToClient("Received your message, boi!", id);
    }

}
