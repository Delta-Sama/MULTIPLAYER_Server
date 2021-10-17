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
    private const int FileIndexAndLoginSpecifier = 2;

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
        string path = Application.dataPath + Path.DirectorySeparatorChar + "ClientsData" + Path.DirectorySeparatorChar + "File" + index + ".txt";

        StreamWriter sWriter = new StreamWriter(path);

        sWriter.WriteLine(AccountFileSpecifiers.Login + "," + login);
        sWriter.WriteLine(AccountFileSpecifiers.Password + "," + password);
        sWriter.WriteLine(AccountFileSpecifiers.Email + "," + email);

        sWriter.Close();
    }

    public AccountInfo GetAccountInformation(int index)
    {
        AccountInfo info = new AccountInfo();

        string path = Application.dataPath + Path.DirectorySeparatorChar + "ClientsData" + Path.DirectorySeparatorChar + "File" + index + ".txt";

        StreamReader sReader = new StreamReader(path);
        string line;

        while ((line = sReader.ReadLine()) != null)
        {
            string[] csv = line.Split(',');

            int specifier = int.Parse(csv[0]);

            if (specifier == AccountFileSpecifiers.Login)
                info.login = csv[1];
            else if (specifier == AccountFileSpecifiers.Password)
                info.password = csv[1];
            else if (specifier == AccountFileSpecifiers.Email)
                info.email = csv[1];
        }

        sReader.Close();

        return info;
    }
}
public struct AccountInfo
{
    AccountInfo(string l, string p, string e)
    {
        login = l;
        password = p;
        email = e;
    }

    public string login;
    public string password;
    public string email;
}

public static class AccountFileSpecifiers
{
    public const int Login = 1;
    public const int Password = 2;
    public const int Email = 3;

}