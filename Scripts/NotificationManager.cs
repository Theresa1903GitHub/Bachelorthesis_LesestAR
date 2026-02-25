// using UnityEngine;
// using Unity.Notifications.Android;
// using UnityEngine.Android;
// using System;
// using System.Collections;

// public class NotificationManager : MonoBehaviour
// {
//     private static NotificationManager instance;

//     [Header("Schedule Settings")]
//     private const int daysToSchedule = 7;
//     private const int notificationHour = 20;
//     private const int notificationMinute = 45;

//     [Header("Notification Icons")]
//     [SerializeField] private string smallIcon = "icon_0";
//     [SerializeField] private string largeIcon = "icon_1";

//     private const string CHANNEL_ID = "default_channel";
//     private const string LAST_SCHEDULE_DATE_KEY = "LAST_NOTIFICATION_SCHEDULE_DATE";

//     void Awake()
//     {
//         // Singleton + Persistenz
//         if (instance != null)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         instance = this;
//         DontDestroyOnLoad(gameObject);
//     }

//     void Start()
//     {
//         CreateNotificationChannel();
//         StartCoroutine(WaitForPermissionAndSchedule());
//     }

//     // ===== Permission Handling =====
//     IEnumerator WaitForPermissionAndSchedule()
//     {
//         while (!HasNotificationPermission())
//         {
//             Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
//             yield return new WaitForSeconds(0.5f);
//         }

// #if UNITY_EDITOR || DEVELOPMENT_BUILD
//         // Verhindert Blockade beim Testen
//         PlayerPrefs.DeleteKey(LAST_SCHEDULE_DATE_KEY);
// #endif

//         ScheduleNotificationsSafe();
//     }

//     bool HasNotificationPermission()
//     {
//         return Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS");
//     }

//     // ===== Pause / Focus Backup =====
//     void OnApplicationPause(bool pause)
//     {
//         if (pause && HasNotificationPermission())
//         {
//             ScheduleNotificationsSafe();
//         }
//     }

//     void OnApplicationFocus(bool hasFocus)
//     {
//         if (!hasFocus && HasNotificationPermission())
//         {
//             ScheduleNotificationsSafe();
//         }
//     }

//     // ===== Channel =====
//     void CreateNotificationChannel()
//     {
//         var channel = new AndroidNotificationChannel
//         {
//             Id = CHANNEL_ID,
//             Name = "Daily Reminder",
//             Importance = Importance.High,
//             Description = "Tägliche Erinnerung um 16 Uhr"
//         };

//         AndroidNotificationCenter.RegisterNotificationChannel(channel);
//     }

//     // ===== Scheduling =====
//     void ScheduleNotificationsSafe()
//     {
//         string today = DateTime.Now.ToString("yyyyMMdd");

//         // Schon heute geplant → abbrechen
//         if (PlayerPrefs.GetString(LAST_SCHEDULE_DATE_KEY, "") == today)
//             return;

//         AndroidNotificationCenter.CancelAllScheduledNotifications();

//         DateTime now = DateTime.Now;

//         for (int i = 0; i < daysToSchedule; i++)
//         {
//             DateTime fireTime = DateTime.Today
//                 .AddHours(notificationHour)
//                 .AddMinutes(notificationMinute)
//                 .AddDays(i);

//             // Sicherheitsabstand: niemals zu nah / in der Vergangenheit
//             if (fireTime <= now.AddMinutes(1))
//                 fireTime = fireTime.AddDays(1);

//             ScheduleNotification(fireTime);
//         }

//         PlayerPrefs.SetString(LAST_SCHEDULE_DATE_KEY, today);
//         PlayerPrefs.Save();
//     }

//     void ScheduleNotification(DateTime fireTime)
//     {
//         var notification = new AndroidNotification
//         {
//             Title = "Hallo Abenteurer! 🌟",
//             Text = "Es ist 16:00 Uhr \nLumi freut sich schon auf euer nächstes Abenteuer!",
//             FireTime = fireTime,
//             SmallIcon = smallIcon,
//             LargeIcon = largeIcon
//         };

//         AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);

// #if UNITY_EDITOR || DEVELOPMENT_BUILD
//         Debug.Log("Notification scheduled for: " + fireTime);
// #endif
//     }
// }


using UnityEngine;
using Unity.Notifications.Android;
using UnityEngine.Android;
using System;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    private static NotificationManager instance;

    [Header("Test Settings")]
    private const int TEST_DELAY_SECONDS = 5;

    [Header("Notification Icons")]
    [SerializeField] private string smallIcon = "icon_0";
    [SerializeField] private string largeIcon = "icon_1";

    private const string CHANNEL_ID = "default_channel";

    void Awake()
    {
        // Singleton + Persistenz
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CreateNotificationChannel();
        StartCoroutine(WaitForPermission());
    }

    IEnumerator WaitForPermission()
    {
        while (!HasNotificationPermission())
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            yield return new WaitForSeconds(0.5f);
        }
    }

    bool HasNotificationPermission()
    {
        return Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS");
    }

    // TEST: Sofort beim Background
    void OnApplicationPause(bool pause)
    {
        if (!pause || !HasNotificationPermission())
            return;

        AndroidNotificationCenter.CancelAllScheduledNotifications();

        DateTime fireTime = DateTime.Now.AddSeconds(TEST_DELAY_SECONDS);

        var notification = new AndroidNotification
        {
            Title = "Hallo Abenteurer! 🌟",
            Text = "Lumi freut sich schon auf \neuer nächstes Abenteuer!",
            FireTime = fireTime,
            SmallIcon = smallIcon,
            LargeIcon = largeIcon
        };

        AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("TEST Notification scheduled for: " + fireTime);
#endif
    }

    // ===== Channel =====
    void CreateNotificationChannel()
    {
        var channel = new AndroidNotificationChannel
        {
            Id = CHANNEL_ID,
            Name = "Test Channel",
            Importance = Importance.High,
            Description = "Test Notification Channel"
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }
}
