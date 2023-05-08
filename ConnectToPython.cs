

using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class ConnectToPython : MonoBehaviour
{
    // [SerializeField]
    private string host = "192.168.0.6"; // Replace with the IP address of your server if it's remote
    [SerializeField]
    private int port = 1234;
    [SerializeField]
    private TcpClient client;
    [SerializeField]
    NetworkStream stream;

    public ConnectToPython(string host){
        Debug.Log("_______________________CONNECT TO PYTHON "+host+"_______________________");
        this.host = host;
    }
    public ConnectToPython(string host, int port){
        Debug.Log("_______________________CONNECT TO PYTHON "+host+"_______________________");
        this.host = host;
        this.port = port;
    }
    public ConnectToPython(){
        Debug.Log("_______________________CONNECT TO PYTHON "+host+"_______________________");
    }

    private void Start()
    {
       ConnectToServer();
    }


    public void ConnectToServer()
    {
         try{
            // Connect to the server
            client = new TcpClient(host, port);
            Debug.Log("Connected to server."+client.ToString());

            // Get the stream
            stream = client.GetStream();

            // Send data to the server
            if(SendAndReceiveData("Dir/File.txt, 1.2345, 2.3456, 3.5678, 4.5678, 5.6789, 6.7891")){
                Debug.Log("_______________________DATA SENT AND RECEIVED SUCCESSFULLY_______________________");
            }
            else{
                Debug.Log("_______________________DATA NOT SENT AND RECEIVED SUCCESSFULLY_______________________");
            }
        }
        catch(SocketException e){
            Debug.Log("SocketException: "+e);
        }
    }

    public void ConnectToRobot(){
         try{
            // Connect to the server
            client = new TcpClient(host, port);
            Debug.Log("Connected to server."+client.ToString());

            // Get the stream
            stream = client.GetStream();
        }
        catch(SocketException e){
            Debug.Log("SocketException: "+e);
        }
    }

    private void OnDestroy()
    {
        // Close the connection
        stream.Close();
        client.Close();
    }

    public int SendData(string data_to_send)
    {
        byte[] data = Encoding.ASCII.GetBytes(data_to_send);
        stream.Write(data, 0, data.Length);
        Debug.Log($"_______________________SENT DATA TO PYTHON_______________________{data}");

        //return the number of bytes sent
        return data_to_send.Length;
    }

    public string ReceiveData()
    {
        byte[] data = new byte[256];
        int bytes = stream.Read(data, 0, data.Length);
        string responseData = Encoding.ASCII.GetString(data, 0, bytes);
        Debug.Log($"_______________________RECEIVED DATA FROM PYTHON_______________________{responseData}");
        return responseData;
    }

    public bool SendAndReceiveData(string data_to_send)
    {
        //if the data is sent successfully we should recieve the size of the data sent
        int bytes_sent = SendData(data_to_send);
        string bytes_received = ReceiveData();
        if(bytes_received == "404"){
            Debug.Log("_______________________DATA NOT SENT AND RECEIVED SUCCESSFULLY_______________________");
            return false;
        }
        if ( bytes_sent.ToString() == bytes_received)
        {
            return true;
        }
        else
        {
            Debug.Log("______________________Expected to receive " + bytes_sent.ToString() + " bytes but received " + bytes_received + " bytes instead.");
            return false;
        }
    }


}