using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private int playersLimit = 2;

    public static MatchManager Instance;

    public List<Match> matches;
    public int PlayersLimit { get => playersLimit; }

    private int currentId = 0;

    private void Awake()
    {
        Instance = this;

        matches = new List<Match>();
    }

    void Start()
    {
        
    }

    public void AddMatch(int creatorId, string matchName)
    {
        UserAccount creator = UsersManager.Instance.GetUser(creatorId);

        Match match = new Match();
        match.players.Add(creator);
        match.matchId = currentId;
        match.name = matchName;

        currentId++;
    }

    public void RemoveMatch(int matchId)
    {
        int matchIdx = FindMatch(matchId);

        if (matchIdx < 0) return;

        foreach (var player in matches[matchIdx].players)
        {
            NetworkedServer.Instance.SendClientRequest(ServerToClientTransferSignifiers.MatchRemoved + ",", player.userId);
        }

        matches.RemoveAt(matchIdx);
    }

    public void AddPlayerToMatch(int userId, int matchId)
    {
        int matchIdx = FindMatch(matchId);

        if (matchIdx < 0)
        {
            Debug.LogError("No match found for id: " + matchId);
            return;
        }

        matches[matchIdx].AddUser(userId);
    }

    public int FindMatch(int matchId)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchId == matchId)
            {
                return i;
            }
        }

        return -1;
    }
}
