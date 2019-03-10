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
    public float y;

    public ClickOrder(int id, float x, float y) {
        type = "CO";
        this.id = id;
        this.x = x;
        this.y = y;
    }
};

public struct UpdatedCell
{
    public int cellType;
    public int id;
    public float x;
    public float y;
    public bool removed;

    public UpdatedCell(int cellType, int id, float x, float y, bool removed) {
        this.cellType = cellType;
        this.id = id;
        this.x = x;
        this.y = y;
        this.removed = removed;
    }
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
    public GameObject wbcPrefab;
    public GameObject antiBodyPrefab;
    public GameObject cellPrefab;

    public List<GameObject> prefabs = new List<GameObject>();

    const string ipServer = "146.179.194.147";
    Socket socket;

    byte[] inpBuffer = new byte[2048];

    public List<object> messages = new List<object>();

    public List<Dictionary<int, CellGJ>> CellList = new List<Dictionary<int, CellGJ>>();
    System.Random rand = new System.Random();

    public void Start()
    {
        prefabs.Add(virusPrefab);
        prefabs.Add(wbcPrefab);
        prefabs.Add(antiBodyPrefab);
        prefabs.Add(cellPrefab);

        for (int i = 0; i < prefabs.Count; i++)
        {
            CellList.Add(new Dictionary<int, CellGJ>());
        }

        UpdatedCell UP = new UpdatedCell(0, 1, 0, 0, false);
        //AddOrMoveCell(UP);

        socket = ConnectSocket(ipServer, 8000);
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

        if (socket != null && socket.Connected)
        {
            socket.Send(bytesSent, bytesSent.Length, 0);

            int inpByteCount = socket.Receive(inpBuffer);

            String input = Encoding.ASCII.GetString(inpBuffer, 0, inpByteCount);
            Debug.Log(input);
            String[] inputArr = input.Split('\n');

            if (inputArr[inputArr.Length - 1].Length == 2) return;

            String intermediate = inputArr[inputArr.Length - 1].Replace(":", ",").Replace("}", "").Replace("{","").Replace("]","").Replace("[","").Replace("\"","");
            Debug.Log(intermediate);
            String[] JSONArr = intermediate.Split(',');

            List<object> resultList = new List<object>();

            for (int i = 0; i < JSONArr.Length; i += 2)
            {
                if (!JSONArr[i].Equals("type"))
                {
                    Debug.Log("UnkownType: " + JSONArr[i]);
                    continue;
                    //throw new Exception("Incoming parsing error");
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

                resultList.Add(newObj);
            }

            foreach (object obj in resultList)
            {
                if (obj.GetType() == typeof(UpdatedCell))
                {
                    UpdatedCell uc = ((UpdatedCell)obj);

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
                Destroy(CellList[UC.cellType][UC.id].GJ);
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

            float VirusScale = 0.2f;
            if (UC.cellType == 0) //virus
            {
                CellList[UC.cellType][UC.id].GJ.transform.localScale = new Vector3(VirusScale, VirusScale, VirusScale);
            }

            float WBCScale = 0.5f;
            if (UC.cellType == 1) //WBC
            {
                CellList[UC.cellType][UC.id].GJ.transform.localScale = new Vector3(WBCScale, WBCScale, WBCScale);
            }

            float antibodyScale = 0.6f;
            if (UC.cellType == 2) //antibody
            {
                CellList[UC.cellType][UC.id].GJ.transform.localScale = new Vector3(antibodyScale, antibodyScale, antibodyScale);
            }

            float CellScale = 0.8f;
            if (UC.cellType == 3) //Cell
            {
                CellList[UC.cellType][UC.id].GJ.transform.localScale = new Vector3(CellScale, CellScale, CellScale);
            }

            CellList[UC.cellType][UC.id].GJ.tag = "Cell";
        }
    }

}
