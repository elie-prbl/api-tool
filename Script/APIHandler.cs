using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class APIHandler : MonoBehaviour
{
    private string _mainApiURL = "";
    private string _gameManagerUrl = "";
    private string _playerId = "";
    private int _gameId = 0;

    [Serializable]
    public class Body
    {
        public string user_reference;
        public int success_id;
        public bool is_validated;
    }

    [Serializable]
    public class Response1
    {
        public int id;
    }

    [Serializable]
    public class BodyMainSuccess
    {
        public string user_uuid;
        public string success_short_name;
    }


    [DllImport("__Internal")]
    public static extern void Quit();

    public void SetPlayerId(string playerId)
    {
        _playerId = playerId;
    }

    public void SetGameId(int gameId)
    {
        _gameId = gameId;
    }

    public void SetGameManagerUrl(string gameManagerUrl)
    {
        _gameManagerUrl = gameManagerUrl;
    }

    public void SetMainApiURL(string mainApiUrl)
    {
        _mainApiURL = mainApiUrl;
    }

    public IEnumerator GameEnded(bool isGameWon)
    {
        BodyMainSuccess bodyMainSuccess = new BodyMainSuccess();
        bodyMainSuccess.user_uuid = _playerId;
        bodyMainSuccess.success_short_name = "PlayGames";
        if (isGameWon)
        {
            bodyMainSuccess.success_short_name = "WinGames";
        }

        string json = JsonUtility.ToJson(bodyMainSuccess);
        UnityWebRequest www = new UnityWebRequest(_mainApiURL + "/successes/user", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        Quit();
    }

    /// <summary>
    ///  This function submit to the API when a player achieve a success in the game
    /// </summary>
    /// <param name="successTitle"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <returns></returns>
    public IEnumerator SubmitAchievement(string successTitle, bool achieved, System.Action onSuccess,
        System.Action<string> onError)
    {

        UnityWebRequest request = UnityWebRequest.Get(_gameManagerUrl + $"/successes/game/{_gameId}/success/{successTitle}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;

            Response1 jsonResult = JsonUtility.FromJson<Response1>(result);

            int successId = jsonResult.id;

            var body = new Body();
            body.success_id = successId;
            body.is_validated = achieved;
            body.user_reference = _playerId;
            string json = JsonUtility.ToJson(body);
            UnityWebRequest www = new UnityWebRequest(_gameManagerUrl + "/users/successes/", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
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
        else
        {
            onError?.Invoke(request.error);
        }
    }
}