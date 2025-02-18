using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class APIHandler : MonoBehaviour
{
    private string _baseURL = "http://10.49.32.196:8080/api/v1";
    private string _playerId = "987654321";
    private string _gameId = "mygame_001";


    [DllImport("__Internal")]
    public static extern void Quit();

    public void SetPlayerId(string playerId)
    {
        _playerId = playerId;
    }

    public void SetGameId(string gameId)
    {
        _gameId = gameId;
    }

    public void SetBaseUrl(string url)
    {
        _baseURL = url;
    }

    // Soumettre un score
    public IEnumerator SubmitScore(int score, System.Action onSuccess, System.Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("sessionId", _gameId);
        form.AddField("playerId", _playerId);
        form.AddField("score", score);

        UnityWebRequest www = UnityWebRequest.Post(_baseURL + "/unitySDK/", form);
        yield return www.SendWebRequest();
        Debug.Log("SubmitScore");

        if (www.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
        }
        else
        {
            onError?.Invoke(www.error);
        }
    }

    /// <summary>
    ///  This function submit to the API when a player achieve a success in the game
    /// </summary>
    /// <param name="achievementId"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <returns></returns>
    public IEnumerator SubmitAchievement(string achievementId, System.Action onSuccess,
        System.Action<string> onError)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerId", _playerId);
        form.AddField("gameId", _gameId);
        form.AddField("achievementId", achievementId);
        UnityWebRequest www = UnityWebRequest.Post(_baseURL + "/unitySDK/", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
        }
        else
        {
            onError?.Invoke(www.error);
        }
    }
}