using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CameraAngleButton : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private bool isNextButton = true;

    private void Start()
    {
        if (cameraController == null)
            cameraController = FindAnyObjectByType<CameraController>();

        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (cameraController != null)
            {
                if (isNextButton)
                    cameraController.NextPreset();
                else
                    cameraController.PreviousPreset();
            }
        });
    }
}
