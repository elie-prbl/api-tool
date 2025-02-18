using System.Collections;
using System.Runtime.InteropServices;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class APIHandler : MonoBehaviour
{
    private string _baseURL = "http://10.49.32.196:8000/";
    private string _playerId = "987654321";
    private string _gameId = "1";

    public class Body
    {
        public string user_reference;
        public int success_id;
        public bool is_validated;
    }


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
    /// <param name="successTitle"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <returns></returns>
    public IEnumerator SubmitAchievement(string successTitle, System.Action onSuccess,
        System.Action<string> onError)
    {

        UnityWebRequest request = UnityWebRequest.Get(_baseURL + $"successes/game/{_gameId}/success/{successTitle}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;

            // Result to JSON
            JObject jsonResult = JObject.Parse(result);

            int successId = int.Parse(jsonResult["id"]?.ToString());

            var body = new Body();
            body.success_id = successId;
            body.is_validated = true;
            body.user_reference = _playerId;
            string json = JsonUtility.ToJson(body);
            UnityWebRequest www = new UnityWebRequest(_baseURL + "users/successes/", "POST");
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