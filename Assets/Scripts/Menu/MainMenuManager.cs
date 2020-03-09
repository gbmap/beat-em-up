using System.Collections;
using System.Collections.Generic;
using Catacumba.Exploration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject creditsButton;
    
    [SerializeField] private GameObject baseCamera;
    [SerializeField] private GameObject startCamera;
    [SerializeField] private GameObject creditsCamera;
    [SerializeField] private GameObject initCamera;

    private Dictionary<GameObject, GameObject> cameras;

    // Start is called before the first frame update
    private void Start()
    {
        // Start button
        var startEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
        startEntry.callback.AddListener(eventData => OnHoverButton(startButton, true));
        var startExit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
        startExit.callback.AddListener(eventData => OnHoverButton(startButton, false));
        var startSubmit = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
        startSubmit.callback.AddListener(eventData => StartCoroutine(OnStart()));

        // Credits button
        var creditsEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
        creditsEntry.callback.AddListener(eventData => OnHoverButton(creditsButton, true));
        var creditsExit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
        creditsExit.callback.AddListener(eventData => OnHoverButton(creditsButton, false));

        startButton.GetComponent<EventTrigger>().triggers = new List<EventTrigger.Entry> { startEntry, startExit, startSubmit };
        creditsButton.GetComponent<EventTrigger>().triggers = new List<EventTrigger.Entry> { creditsEntry, creditsExit };

        cameras = new Dictionary<GameObject, GameObject>
        {
            {startButton, startCamera}, {creditsButton, creditsCamera}
        };
    }

    private void OnHoverButton(GameObject button, bool hover)
    {
        cameras[button].SetActive(hover);
    }

    private IEnumerator OnStart()
    {
        // Hide main menu
        var lerpValue = 0f;
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, lerpValue);
            lerpValue += Time.deltaTime;

            yield return 1;
        }
        
        // Enable start camera
        initCamera.SetActive(true);

        // Wait for timeline to finish
        var playable = initCamera.GetComponent<PlayableDirector>();
        while (playable.state == PlayState.Playing)
            yield return 1;

        // Disable cameras
        startCamera.SetActive(false);
        baseCamera.SetActive(false);
        creditsCamera.SetActive(false);
        
        // Give input to player
        // Initialize first player camera and input
        CameraManager.Instance.Initialize();
        
        // Disable main menu
        gameObject.SetActive(false);
        yield break;
    }
}
