using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LoginRequest
{
    public string user_code;
}

[System.Serializable]
public class LoginResponse
{
    public int id;
    public string user_code;
    public string username;
    public string phone;
    public int passedlevels;
    public int purchasedlevels;
    public int available_points;
    public int total_points;
}

public class AuthManager : MonoBehaviour
{
    [Header("Server Settings")]
    public string serverURL = "http://127.0.0.1:8000";

    [Header("Login UI")]
    public TMP_InputField codeInput;
    public Button loginButton;
    public TMP_Text statusText;

    [Header("User Info UI")]
    public GameObject userInfoPanel;
    public TMP_Text userCodeText;
    public TMP_Text userNameText;
    public TMP_Text levelsText;
    public TMP_Text pointsText;
    public Button logoutButton;
    public Button completeLevelButton;

    private LoginResponse currentUser;
    private bool isLoggedIn = false;

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        logoutButton.onClick.AddListener(OnLogoutClicked);
        completeLevelButton.onClick.AddListener(OnCompleteLevelClicked);

        // Автозаполнение последнего кода
        string lastCode = PlayerPrefs.GetString("LastUserCode", "");
        if (!string.IsNullOrEmpty(lastCode))
        {
            codeInput.text = lastCode;
        }

        UpdateUI();
    }

    void Update()
    {
        // Enter для входа
        if (Input.GetKeyDown(KeyCode.Return) && !isLoggedIn && codeInput.text.Length > 0)
        {
            OnLoginClicked();
        }
    }

    public void OnLoginClicked()
    {
        string userCode = codeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(userCode))
        {
            ShowStatus("Введите код пользователя", Color.red);
            return;
        }

        if (userCode.Length != 6 && userCode != "DEBUG")
        {
            ShowStatus("Код должен содержать 6 символов (2 буквы + 4 цифры)", Color.red);
            return;
        }

        StartCoroutine(Login(userCode));
    }

    private IEnumerator Login(string userCode)
    {
        ShowStatus("Вход в систему...", Color.yellow);
        loginButton.interactable = false;

        LoginRequest loginData = new LoginRequest
        {
            user_code = userCode
        };

        string jsonData = JsonUtility.ToJson(loginData);

        if (userCode == "DEBUG")
        {
            SceneManager.LoadScene(1);
        }
        else { 
            using (UnityWebRequest request = new UnityWebRequest($"{serverURL}/login", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    LoginResponse userData = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    currentUser = userData;
                    isLoggedIn = true;

                    SaveUserData(userData);
                    ShowStatus($"Добро пожаловать, {userData.username}!", Color.green);
                    SceneManager.LoadScene(1);
                }
                else
                {
                    if (request.responseCode == 404)
                    {
                        ShowStatus("Пользователь с таким кодом не найден", Color.red);
                    }
                    else
                    {
                        ShowStatus($"Ошибка подключения: {request.error}", Color.red);
                    }
                }

                loginButton.interactable = true;
                UpdateUI();
            }
        }
    }

    public void OnLogoutClicked()
    {
        currentUser = null;
        isLoggedIn = false;

        PlayerPrefs.DeleteKey("UserCode");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("UserID");

        codeInput.text = "";

        ShowStatus("Вы вышли из системы", Color.white);
        UpdateUI();
    }

    public void OnCompleteLevelClicked()
    {
        if (!isLoggedIn || currentUser == null)
        {
            ShowStatus("Сначала войдите в систему", Color.red);
            return;
        }

        StartCoroutine(CompleteLevelRequest());
    }

    private IEnumerator CompleteLevelRequest()
    {
        ShowStatus("Завершение уровня...", Color.yellow);

        var levelData = new { user_code = currentUser.user_code };
        string jsonData = JsonUtility.ToJson(levelData);

        using (UnityWebRequest request = new UnityWebRequest($"{serverURL}/complete_level", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ShowStatus("Уровень завершен! +1 поинт", Color.green);
                // Обновляем данные пользователя
                yield return StartCoroutine(Login(currentUser.user_code));
            }
            else
            {
                ShowStatus($"Ошибка: {request.error}", Color.red);
            }
        }
    }

    private void SaveUserData(LoginResponse userData)
    {
        PlayerPrefs.SetString("UserCode", userData.user_code);
        PlayerPrefs.SetString("LastUserCode", userData.user_code);
        PlayerPrefs.SetString("Username", userData.username);
        PlayerPrefs.SetInt("UserID", userData.id);
        PlayerPrefs.SetInt("PassedLevels", userData.passedlevels);
        PlayerPrefs.SetInt("PurchasedLevels", userData.purchasedlevels);
        PlayerPrefs.SetInt("AvailablePoints", userData.available_points);
        PlayerPrefs.SetInt("TotalPoints", userData.total_points);
        PlayerPrefs.Save();
    }

    private void UpdateUI()
    {
        if (isLoggedIn && currentUser != null)
        {
            // Скрываем поле ввода и кнопку входа
            codeInput.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);

            // Показываем информацию о пользователе
            userInfoPanel.SetActive(true);

            userCodeText.text = $"Код: {currentUser.user_code}";
            userNameText.text = $"Имя: {currentUser.username}";
            levelsText.text = $"Уровни: {currentUser.passedlevels} пройдено | {currentUser.purchasedlevels} куплено";
            pointsText.text = $"Поинты: {currentUser.available_points} доступно | {currentUser.total_points} всего";
        }
        else
        {
            // Показываем поле ввода и кнопку входа
            codeInput.gameObject.SetActive(true);
            loginButton.gameObject.SetActive(true);

            // Скрываем информацию о пользователе
            userInfoPanel.SetActive(false);
        }
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log($"[Login] {message}");
    }

    // Автоматическая активация кнопки при вводе
    public void OnCodeInputChanged()
    {
        loginButton.interactable = codeInput.text.Length == 6;
    }

    // Для тестирования
    public void TestLogin()
    {
        // Генерируем случайный тестовый код
        string testCode = "AB" + Random.Range(1000, 9999).ToString();
        codeInput.text = testCode;
        ShowStatus($"Тестовый код: {testCode}", Color.blue);
    }
}