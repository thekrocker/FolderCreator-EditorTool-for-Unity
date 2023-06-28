using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.IO.Path;
using static System.IO.Directory;
using static UnityEngine.Application;
using static UnityEditor.AssetDatabase;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class FolderUtils
{
    public static bool HasSpecialChar(string input, out string special)
    {
        string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,*İıÖöÇçŞşÜü";
        special = specialChar;

        foreach (var item in specialChar)
            if (input.Contains(item))
                return true;

        return false;
    }
}

public class FolderCreator : EditorWindow
{
    #region UI elements

    private const string FolderPath = "Assets/Folder Creator System/Editor/";

    private VisualElement _rootElement;

    private TextField _addFolderTextField;

    private ScrollView _displayFoldersScrollView;

    private Button _createFoldersButton;

    private Label _notificationLabel;

    #endregion


    private readonly List<string> _activeFolderLabels = new List<string>();


    [MenuItem("Tools/Folder Creator")]
    private static void OpenWindow()
    {
        FolderCreator window = GetWindow<FolderCreator>();
        window.titleContent = new GUIContent("Folder Creator by Safa");
        window.minSize = new Vector2(250, 500f);
    }

    private T Get<T>(string elementPath) where T : VisualElement
    {
        return _rootElement.Q<T>(elementPath);
    }

    private void CreateGUI()
    {
        // Clear active labels
        _activeFolderLabels.Clear();

        _rootElement = rootVisualElement;
        VisualTreeAsset treeAsset =
            LoadAssetAtPath<VisualTreeAsset>(FolderPath + nameof(FolderCreator) + ".uxml");
        StyleSheet styleSheets = LoadAssetAtPath<StyleSheet>(FolderPath + nameof(FolderCreator) + ".uss");
        _rootElement.Add(treeAsset.Instantiate());
        _rootElement.styleSheets.Add(styleSheets);

        // Bind References
        _addFolderTextField = Get<TextField>(nameof(_addFolderTextField));
        _displayFoldersScrollView = Get<ScrollView>(nameof(_displayFoldersScrollView));
        _createFoldersButton = Get<Button>(nameof(_createFoldersButton));
        _notificationLabel = Get<Label>(nameof(_notificationLabel));

        _addFolderTextField.RegisterCallback<KeyDownEvent>(AddFolderAsLabel);

        _createFoldersButton.visible = false;
        _createFoldersButton.clicked += CreateFolders;
    }


    private void CreateFolders()
    {
        CreateFoldersFrom(_activeFolderLabels);
    }

    private void CreateFoldersFrom(List<string> folders)
    {
        if (_activeFolderLabels.Count <= 0)
        {
            SetNotification("There are no folders to create!", false);
            return;
        }

        string root = "_Project";
        // Create root directory
        string fullPath = Combine(dataPath, root);

        foreach (string folder in folders)
        {
            CreateDirectory(Combine(fullPath, folder));
        }

        SaveAssets();
        Refresh();

        string path = "Assets/" + root;
        UnityEngine.Object obj = LoadAssetAtPath(path, typeof(UnityEngine.Object));
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);

        SetNotification("Folders created succesfully on " + path, true);
    }

    private void AddFolderAsLabel(KeyDownEvent e)
    {
        if (Event.current.Equals(Event.KeyboardEvent("Return")))
        {
            AddFolderAsLabel();
        }
    }

    private void AddFolderAsLabel()
    {
        string value = _addFolderTextField.value;

        // If string is empty,
        if (string.IsNullOrEmpty(value))
        {
            SetNotification("Folder name cannot be empty!", false);
            _addFolderTextField.value = "";
            _addFolderTextField.Focus();
            return;
        }

        if (FolderUtils.HasSpecialChar(value, out string special))
        {
            SetNotification($"Your folder name includes invalid characters from: {special}", false);
            _addFolderTextField.value = "";
            _addFolderTextField.Focus();
            return;
        }

        // Activate button
        if (!_createFoldersButton.visible) _createFoldersButton.visible = true;

        if (!_activeFolderLabels.Contains(value))
        {
            // Create new label and add it to scroll view
            Label label = new Label();
            label.text = value;
            label.AddToClassList("folderNameText");
            _activeFolderLabels.Add(value);
            _displayFoldersScrollView.Add(label);
            _addFolderTextField.value = "";
            _addFolderTextField.Focus();
        }
        else
        {
            Debug.LogWarning("There is already folder with the same name!");
        }
    }

    public async void SetNotification(string notification, bool success)
    {
        string successClassSelector = "notificationSuccess";
        string failClassSelector = "notificationFail";

        _notificationLabel.RemoveFromClassList(successClassSelector);
        _notificationLabel.RemoveFromClassList(failClassSelector);

        if (success)
        {
            _notificationLabel.AddToClassList(successClassSelector);
        }
        else
        {
            _notificationLabel.AddToClassList(failClassSelector);
        }
        
        _notificationLabel.text = notification;
        
        await Task.Delay(3000);
        
        _notificationLabel.text = "";
    }
}