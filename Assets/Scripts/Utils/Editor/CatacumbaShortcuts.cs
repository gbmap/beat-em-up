using UnityEngine;
using UnityEditor;
using System.Linq;
using Catacumba.Entity;

public class CatacumbaShortcuts : MonoBehaviour
{
    //////////////////
    /// Esse método pode e vai quebrar eventualmente.
    /// 
    [MenuItem("Catacumba/Play From Beginning")]
    public static void PlayFromBeginning()
    {
        var player = FindObjectOfType<CharacterPlayerInput>();
        GameObject startPosition = GameObject.Find("PlayerStartPosition");
        if (!startPosition)
        {
            Debug.LogError("Couldn't find starting position.");
            return;
        }

        GameObject cameraManager = GameObject.Find("CameraManager");
        if (!cameraManager)
        {
            Debug.LogError("Couldn't find camera maanger.");
            return;
        }

        GameObject menu = GameObject.Find("MainMenu");
        if (!menu)
        {
            Debug.LogError("Couldn't find menu.");
            return;
        }

        GameObject menuCanvas = GameObject.Find("MainMenu Canvas");
        if (!menuCanvas)
        {
            Debug.LogError("Couldn't find menu canvas.");
            return;
        }

        menu.SetActive(true);
        menuCanvas.SetActive(true);

        var cams = cameraManager.GetComponentsInChildren<Cinemachine.CinemachineVirtualCamera>();
        foreach (var cam in cams)
        {
            bool active = cam.gameObject.name == "vcam-centerhallbalcony";
            cam.gameObject.SetActive(active);
        }

        player.transform.position = startPosition.transform.position;

        EditorApplication.ExecuteMenuItem("Edit/Play");
    }

    [MenuItem("Catacumba/Play from Test Position")]
    public static void PlayFromTestPosition()
    {
        var player = FindObjectOfType<CharacterPlayerInput>();
        GameObject startPosition = GameObject.Find("PlayerTestPosition");
        if (!startPosition)
        {
            Debug.LogError("Couldn't find starting position.");
            return;
        }

        GameObject cameraManager = GameObject.Find("CameraManager");
        if (!cameraManager)
        {
            Debug.LogError("Couldn't find camera maanger.");
            return;
        }

        // blargh
        var cams = cameraManager.GetComponentsInChildren<Cinemachine.CinemachineVirtualCamera>(includeInactive: true);
        var targetCamera = cams.OrderBy(c => Vector3.Distance(startPosition.transform.position, c.transform.position)).First();
        foreach (var cam in cams)
        {
            cam.gameObject.SetActive(targetCamera == cam);
        }

        player.transform.position = startPosition.transform.position;

        EditorApplication.ExecuteMenuItem("Edit/Play");
    }
}
