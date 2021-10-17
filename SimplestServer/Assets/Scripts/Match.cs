using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
    public int matchId = 0;
    public bool inProgress = false;
    public string name = "";
    public List<UserAccount> players;

    public void AddUser(int userId)
    {
        if (players.Count < MatchManager.Instance.PlayersLimit)
        {
            // Check if user is already connected
            foreach (var player in players)
            {
                if (player.userId == userId) return;
            }

            UserAccount account = UsersManager.Instance.GetUser(userId);

            players.Add(account);

            if (players.Count == MatchManager.Instance.PlayersLimit)
            {
                StartMatch();
            }
        }
    }

    public void RemoveUser(int userId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].userId == userId)
            {
                players.RemoveAt(i);
                break;
            }
        }

        if (players.Count == 0)
        {
            MatchManager.Instance.RemoveMatch(matchId);
        }
    }

    public void StartMatch()
    {
        if (!inProgress)
        {
            inProgress = true;

            foreach (var player in players)
            {
                NetworkedServer.Instance.SendClientRequest(ServerToClientTransferSignifiers.MatchStarted + ",", player.userId);
            }
        }
    }
}
