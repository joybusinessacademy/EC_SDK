using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager;

public class LicenseBlocker : EditorWindow
{
	private static string windowTitle = "CCK License";
	private static string information = "I notice your license has expired, please go to SkillsVR to renew your license";
	
	private static bool focused = false;
	internal static LicenseBlocker windowInstance;

	public delegate void LicenseBlockerWindowClosed();
	public LicenseBlockerWindowClosed licenseBlockerClosed;

	private void OnGUI()
	{
		if(focused == false)
		{
			rootVisualElement.Focus();
		}

		focused = true;
	}

	public static void Show()
	{
		focused = false;

		windowInstance = CreateInstance<LicenseBlocker>();
		windowInstance.titleContent = new GUIContent(windowTitle);
		windowInstance.minSize = new Vector2(600, 400);
		windowInstance.maxSize = new Vector2(600, 400);
		windowInstance.rootVisualElement.name = "new-scene-window";
		windowInstance.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/NewSceneWindow"));

		windowInstance.rootVisualElement.Add(new Label("License Data") { name = "heading" });

		Label licenseInformation = new Label(information);

		windowInstance.rootVisualElement.Add(licenseInformation);
		

		VisualElement buttonContainer = new VisualElement();
		buttonContainer.style.flexGrow = 1;
		buttonContainer.style.flexDirection = FlexDirection.Row;

		buttonContainer.Add(OpenEnterpriseURL());
		buttonContainer.Add(CloseWindow());
		windowInstance.rootVisualElement.Add(buttonContainer);

		windowInstance.ShowModal();
	}

	private static Button CloseWindow()
	{
		Button clearSelectionButton = new()
		{
			name = "scene-button",
		};

		clearSelectionButton.text = "Close Window";
		clearSelectionButton.style.flexGrow = 1;
		clearSelectionButton.style.flexDirection = FlexDirection.Row;

		clearSelectionButton.clicked += () =>
		{
			windowInstance.Close();
		};

		return clearSelectionButton;
	}

	private static Button OpenEnterpriseURL()
	{
		Button clearSelectionButton = new()
		{
			name = "scene-button",
		};

		clearSelectionButton.text = "Extend License";
		clearSelectionButton.style.flexGrow = 1;
		clearSelectionButton.style.flexDirection = FlexDirection.Row;

		clearSelectionButton.clicked += () =>
		{
			Application.OpenURL("https://www.skillsvr.com");
		};

		return clearSelectionButton;
	}

	private void OnDisable()
	{
		focused = false;

		if (licenseBlockerClosed != null)
			licenseBlockerClosed.Invoke();
	}
}
