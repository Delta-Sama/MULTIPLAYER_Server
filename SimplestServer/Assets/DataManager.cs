using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class DataManager : MonoBehaviour
{
    private string indexPath;

    public int lastIndex = 0;
    public Dictionary<int, string> indexesDict;

    private const int LastIndexSpecifier = 1;
    private const int FileIndexAndLoginSpecifier = 1;

    public static DataManager Instance;

    private void Start()
    {
        Instance = this;

        indexPath = Application.dataPath + Path.DirectorySeparatorChar + "ClientsData" + Path.DirectorySeparatorChar + "indexes.txt";

        LoadClientsData();
    }

    void LoadClientsData()
    {
        if (!AssetDatabase.IsValidFolder("Assets" + Path.DirectorySeparatorChar + "ClientsData"))
        {
            AssetDatabase.CreateFolder("Assets", "ClientsData");
            StreamWriter sWriter = new StreamWriter(indexPath);
            sWriter.WriteLine(LastIndexSpecifier + "," + "0" + '\n');
            sWriter.Close();
        }

        LoadDictionary();
    }

    void LoadDictionary()
    {
        indexesDict = new Dictionary<int, string>();

        StreamReader sReader = new StreamReader(indexPath);

        string line;

        while ((line = sReader.ReadLine()) != null)
        {
            string[] csv = line.Split(',');

            int lineType = int.Parse(csv[0]);

            if (lineType == LastIndexSpecifier)
            {
                lastIndex = int.Parse(csv[1]);
            }
            else if (lineType == FileIndexAndLoginSpecifier)
            {
                indexesDict.Add(int.Parse(csv[1]), csv[2]);
            }
        }

        sReader.Close();
    }

    public int RegisterNewAccountIndex(string login)
    {
        Instance = this;

        lastIndex += 1;

        StreamReader sReader = new StreamReader(indexPath);

        string file = "";
        string line;

        while ((line = sReader.ReadLine()) != null)
        {
            string[] csv = line.Split(',');
            int lineType = int.Parse(csv[0]);

            if (lineType == LastIndexSpecifier)
            {
                file += LastIndexSpecifier + "," + lastIndex + "\n";
                break;
            }
        }

        file += sReader.ReadToEnd();

        sReader.Close();

        StreamWriter sWriter = new StreamWriter(indexPath);
        sWriter.Write(file);
        sWriter.WriteLine(FileIndexAndLoginSpecifier + "," + lastIndex + "," + login);

        sWriter.Close();

        indexesDict.Add(lastIndex, login);

        return lastIndex;
    }

    public void WriteDataToAccountFile(int index, string login, string password, string email)
    {
        string path = Application.dataPath + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "File" + index + ".txt";

        StreamWriter sWriter = new StreamWriter(path);



        sWriter.Close();
    }
}
