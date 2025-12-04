using UnityEngine;
using Unity.MLAgents;   // <- important

public class GUI_FallingAgent : MonoBehaviour
{
    [SerializeField] private Agent _agent;   // e.g. your FallingTrashAgent

    private GUIStyle _defaultStyle = new GUIStyle();
    private GUIStyle _positiveStyle = new GUIStyle();
    private GUIStyle _negativeStyle = new GUIStyle();

    void Start()
    {
        _defaultStyle.fontSize = 20;
        _defaultStyle.normal.textColor = Color.yellow;

        _positiveStyle.fontSize = 20;
        _positiveStyle.normal.textColor = Color.green;

        _negativeStyle.fontSize = 20;
        _negativeStyle.normal.textColor = Color.red;
    }

    private void OnGUI()
    {
        if (_agent == null) return;

        int episode = _agent.CompletedEpisodes;
        int step = _agent.StepCount;
        float cumReward = _agent.GetCumulativeReward();

        string debugEpisode = $"Episode: {episode} - Step: {step}";
        string debugReward = $"Reward: {cumReward:F3}";

        GUIStyle rewardStyle = cumReward < 0f ? _negativeStyle : _positiveStyle;

        GUI.Label(new Rect(20, 20, 500, 30), debugEpisode, _defaultStyle);
        GUI.Label(new Rect(20, 60, 500, 30), debugReward, rewardStyle);
    }
}
