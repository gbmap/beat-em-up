using UnityEngine;

public class RulesOfThirds : MonoBehaviour
{
    public bool enableGizmosRules = false;
    public bool enableDebugRules = false;
    public float screenOffSet = 0.001f;

    private Camera cameraMain;

    // Use this for initialization
    void Start()
    {
        cameraMain = GetComponent<Camera>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        cameraMain = GetComponent<Camera>();

        Vector3 zero = cameraMain.ScreenPointToRay(new Vector3(0, 0, 0)).GetPoint(screenOffSet);
        Vector3 right = cameraMain.ScreenPointToRay(new Vector3(Screen.width, 0, 0)).GetPoint(screenOffSet);
        Vector3 up = cameraMain.ScreenPointToRay(new Vector3(0, Screen.height, 0)).GetPoint(screenOffSet);

        Vector3 upDirection = transform.up * (up - zero).magnitude;
        Vector3 rightDirection = transform.right * (right - zero).magnitude;

        Vector3 bottomLeft = cameraMain.ScreenPointToRay(new Vector3(Screen.width / 3, 0, 0)).GetPoint(screenOffSet);
        Vector3 bottomRight = cameraMain.ScreenPointToRay(new Vector3(2 * Screen.width / 3, 0, 0)).GetPoint(screenOffSet);
        Vector3 RightTop = cameraMain.ScreenPointToRay(new Vector3(0, Screen.height / 3, 0)).GetPoint(screenOffSet);
        Vector3 LeftTop = cameraMain.ScreenPointToRay(new Vector3(0, 2 * Screen.height / 3, 0)).GetPoint(screenOffSet);

        if (enableDebugRules)
        {
            Gizmos.DrawRay(bottomLeft, upDirection);
            Gizmos.DrawRay(bottomRight, upDirection);
            Gizmos.DrawRay(RightTop, rightDirection);
            Gizmos.DrawRay(LeftTop, rightDirection);
        }
        if (enableDebugRules)
        {
            Debug.DrawRay(bottomLeft, upDirection, Color.yellow);
            Debug.DrawRay(bottomRight, upDirection, Color.yellow);
            Debug.DrawRay(RightTop, rightDirection, Color.yellow);
            Debug.DrawRay(LeftTop, rightDirection, Color.yellow);
        }

    }
}