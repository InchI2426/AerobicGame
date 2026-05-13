using UnityEngine;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Show Password")]
    public TextMeshProUGUI showPasswordButtonText;
    private bool isPasswordVisible = false;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login")]
    public TMP_InputField loginUsername;
    public TMP_InputField loginPassword;
    public TextMeshProUGUI loginErrorText;

    [Header("Register")]
    public TMP_InputField registerUsername;
    public TMP_InputField registerPassword;
    public TextMeshProUGUI registerErrorText;

    void Start()
    {
        ShowLogin(); // เริ่มที่หน้า Login
    }

    // ===== สลับ Panel =====
    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        loginErrorText.text = "";
    }

    public void ShowRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        registerErrorText.text = "";
    }

    // ===== Login =====
    public void OnLoginPressed()
    {
        string user = loginUsername.text.Trim();
        string pass = loginPassword.text;

        if (user == "" || pass == "")
        {
            loginErrorText.text = "Please fill in all the required information";
            return;
        }

        int playerId = DatabaseManager.Instance.LoginPlayer(user, pass);

        if (playerId == -1)
        {
            loginErrorText.text = "The username or password is incorrect";
            return;
        }

        // บันทึก session
        PlayerPrefs.SetInt("PlayerId", playerId);
        PlayerPrefs.SetString("PlayerName", user);

        Debug.Log($"✅ Login สำเร็จ: {user} (id={playerId})");
        SceneLoader.GoToStageSelect();
    }

    // ===== Register =====
    public void OnRegisterPressed()
    {
        string user = registerUsername.text.Trim();
        string pass = registerPassword.text;

        if (user == "" || pass == "")
        {
            registerErrorText.text = "Please fill in all the required information";
            return;
        }

        if (pass.Length < 4)
        {
            registerErrorText.text = "The password must be at least 4 characters long";
            return;
        }

        bool success = DatabaseManager.Instance.RegisterPlayer(user, pass);

        if (!success)
        {
            registerErrorText.text = "This username already exists";
            return;
        }

        Debug.Log($"✅ Register สำเร็จ: {user}");
        // สมัครเสร็จ → กลับไป Login
        registerErrorText.text = "";
        ShowLogin();
        loginErrorText.text = "Registration successful! Please log in";
    }
    public void OnTogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            // แสดงรหัส
            registerPassword.contentType = TMP_InputField.ContentType.Standard;
            showPasswordButtonText.text = "X"; // เปลี่ยนไอคอน
        }
        else
        {
            // ซ่อนรหัส
            registerPassword.contentType = TMP_InputField.ContentType.Password;
            showPasswordButtonText.text = "";
        }

        // บังคับ refresh ทันที
        registerPassword.ForceLabelUpdate();
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}