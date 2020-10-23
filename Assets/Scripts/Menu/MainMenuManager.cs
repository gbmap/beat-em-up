using System.Collections;
using System.Collections.Generic;
using Catacumba;
using Catacumba.Exploration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;
using Catacumba.Entity;

public class MainMenuManager : MonoBehaviour
{
    //[SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject creditsButton;
    
    [SerializeField] private GameObject baseCamera;
    [SerializeField] private GameObject startCamera;
    [SerializeField] private GameObject creditsCamera;
    [SerializeField] private GameObject initCamera;

    private Dictionary<GameObject, GameObject> cameras;

    CharacterPlayerInput[] inputs;

    // Start is called before the first frame update
    private void Start()
    {
        if (StateManager.Retry)
        {
            //mainMenuObject.SetActive(false);
            mainMenuCanvas.SetActive(false);
            gameObject.SetActive(false);
            return;
        }
        else
        {
            inputs = FindObjectsOfType<CharacterPlayerInput>();
            foreach (var i in inputs)
            {
                i.enabled = false;
            }
        }

        // Start button
        var startEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
        startEntry.callback.AddListener(eventData => OnHoverButton(startButton, true));
        var startExit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
        startExit.callback.AddListener(eventData => OnHoverButton(startButton, false));
        var startSubmit = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
        startSubmit.callback.AddListener(eventData => StartCoroutine(COnStart()));

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

        EventSystem.current.SetSelectedGameObject(startButton);

    }

    private void OnHoverButton(GameObject button, bool hover)
    {
        cameras[button].SetActive(hover);
    }

    public void OnStart()
    {
        StartCoroutine(COnStart());
    }

    private IEnumerator COnStart()
    {
               
        // Hide main menu
        var lerpValue = 0f;
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, lerpValue);
            lerpValue += Time.deltaTime;

            yield return 1;
        }

        mainMenuCanvas.SetActive(false);

        // Enable start camera
        initCamera.SetActive(true);

        // Wait for timeline to finish
        var playable = initCamera.GetComponent<PlayableDirector>();
        while (playable.state == PlayState.Playing)
            yield return 1;

        foreach (var i in inputs)
        {
            i.enabled = true;
        }

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
