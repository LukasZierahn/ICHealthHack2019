using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;


public struct ClickOrder
{
    public string type;
    public int id;
    public float x;
    public float z;

    public ClickOrder(int id, float x, float z) {
        type = "CO";
        this.id = id;
        this.x = x;
        this.z = z;
    }
};

public struct UpdatedCell
{
    public int cellType;
    public int id;
    public float x;
    public float y;
    public bool removed;
};

public struct CellGJ
{
    public int type;
    public int id;
    public GameObject GJ;

    public CellGJ(int type, int id, GameObject GJ)
    {
        this.type = type;
        this.id = id;
        this.GJ = GJ;
    }
}


public class NetworkConnection : MonoBehaviour 
{
    public GameObject virusPrefab;

    public List<GameObject> prefabs = new List<GameObject>();

    const int cellTypeCount = 4;

    const string ipServer = "146.179.203.201";
    Socket socket;

    byte[] inpBuffer = new byte[1024];

    public List<object> messages = new List<object>();

    public List<Dictionary<int, CellGJ>> CellList = new List<Dictionary<int, CellGJ>>();
    System.Random rand = new System.Random();

    // Start is called before the first frame update
    public NetworkConnection()
    {
        socket = ConnectSocket(ipServer, 8000);

    }

    public void Start()
    {
        for (int i = 0; i < cellTypeCount; i++) {
            CellList.Add(new Dictionary<int, CellGJ>());
        }

        prefabs.Add(virusPrefab);
    }

    public void FixedUpdate()
    {
        string requestParams = "";

        foreach (object mes in messages) 
        {
            foreach (var field in mes.GetType().GetFields())
            {
                requestParams += field.Name + "=" + field.GetValue(mes).ToString() + "&";
            }
        } 
        messages.Clear();

        int length = requestParams.Length;

        string request = "POST / HTTP/1.1\r\nHost: " + ipServer + "\r\nConnection: Keep-Alive\r\nContent-Type: application/x-www-form-urlencoded\r\nContent-Length: " + length.ToString() + "\r\n\r\n";
        request += requestParams;
        Byte[] bytesSent = Encoding.ASCII.GetBytes(request);

        if (socket.Connected)
        {
            socket.Send(bytesSent, bytesSent.Length, 0);

            int inpByteCount = socket.Receive(inpBuffer);

            String input = Encoding.ASCII.GetString(inpBuffer, 0, inpByteCount);
            String[] inputArr = input.Split('\n');


            String intermediate = inputArr[inputArr.Length - 1].Substring(1, inputArr[inputArr.Length - 1].Length - 2).Replace(":", ",");
            String[] JSONArr = intermediate.Split(',');

            for (int i = 0; i < JSONArr.Length; i += 1)
            {
                JSONArr[i] = JSONArr[i].Substring(1, JSONArr[i].Length - 2);
            }

            Dictionary<Type, object> resultDic = new Dictionary<Type, object>();

            for (int i = 0; i < JSONArr.Length; i += 2)
            {
                if (!JSONArr[i].Equals("type"))
                {
                    Debug.Log("1: " + JSONArr[i]);
                    throw new Exception("Incoming parsing error");
                }

                Type currentType = Type.GetType(JSONArr[i + 1]);
                object newObj = Activator.CreateInstance(currentType);

                foreach (var field in currentType.GetFields())
                {
                    i += 2;

                    if (!field.Name.Equals(JSONArr[i]))
                    {
                        Debug.Log(field.Name);
                        throw new Exception("Incoming parsing error");
                    }

                    field.SetValue(newObj, Convert.ChangeType(JSONArr[i + 1], field.FieldType));
                }

                resultDic.Add(currentType, newObj);
            }

            foreach (KeyValuePair<Type, object> entry in resultDic)
            {
                if (entry.Key == typeof(UpdatedCell))
                {
                    UpdatedCell uc = ((UpdatedCell)entry.Value);

                    AddOrMoveCell(uc);
                    //AddVirus(vp.id, new Vector3(vp.x, 0, vp.z));
                }
                //entry.Key.GetMethod("Process").Invoke(entry.Value, new object[] { this });
            }
        }
    }

    private static Socket ConnectSocket(string server, int port)
    {
        Socket s = null;
        IPHostEntry hostEntry = null;

        // Get host related information.
        hostEntry = Dns.GetHostEntry(server);

        // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
        // an exception that occurs when the host IP Address is not compatible with the address family
        // (typical in the IPv6 case).
        foreach (IPAddress address in hostEntry.AddressList)
        {
            IPEndPoint ipe = new IPEndPoint(address, port);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tempSocket.Connect(ipe);

            if (tempSocket.Connected)
            {
                s = tempSocket;
                break;
            }
            else
            {
                continue;
            }
        }
        return s;
    }

    Vector3 getAbsMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void AddOrMoveCell(UpdatedCell UC)
    {
        Vector3 pos = new Vector3(UC.x, 0, UC.y);

        CellGJ CGJ;
        if (CellList[UC.cellType].TryGetValue(UC.id, out CGJ))
        {
            if (UC.removed)
            {
                CellList[UC.cellType].Remove(UC.id);
            }
            else
            {
                CGJ.GJ.transform.position = pos;
            }
        }
        else
        {
            Quaternion rot = new Quaternion((float) rand.NextDouble(), (float) rand.NextDouble(), (float) rand.NextDouble(),(float) rand.NextDouble());
            CellList[UC.cellType].Add(UC.id, new CellGJ(UC.cellType, UC.id, Instantiate(prefabs[UC.cellType], pos, rot)));
        }
    }

}
