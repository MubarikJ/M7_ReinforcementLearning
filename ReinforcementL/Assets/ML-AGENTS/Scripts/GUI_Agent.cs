using UnityEngine;

public class GUI_Agent : MonoBehaviour
{
    [SerializeField] private MubarikAgent _GUIAgent;

    private GUIStyle _defaultStyle = new GUIStyle();
    private GUIStyle _positiveStyle = new GUIStyle();
    private GUIStyle _negativeStyle = new GUIStyle();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        string debugEpisode = "Episode: " + _GUIAgent.CurrentEpisode + " -Step: " + _GUIAgent.StepCount;
        string debugReward = "Reward: " + _GUIAgent.CumulativeReward.ToString();

        GUIStyle rewardStyle = _GUIAgent.CumulativeReward < 0 ? _negativeStyle : _positiveStyle;

        GUI.Label(new Rect (20, 20, 500, 30), debugEpisode, _defaultStyle);
        GUI.Label(new Rect (20, 60, 500, 30), debugReward, rewardStyle);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
