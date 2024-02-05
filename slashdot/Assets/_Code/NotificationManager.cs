using System.Collections;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;
    public GameObject notificationPanel;
    void Awake()
    {
        notificationPanel.SetActive(false);
        if (Instance != null)
        {
            Debug.LogWarning("Multiple NotificationManagers in scene!");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void ShowNotification(string message)
    {
        notificationPanel.SetActive(true);
        notificationPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = message;
        StartCoroutine(HideNotification());
    }

    IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(2f);
        notificationPanel.SetActive(false);
    }
}
