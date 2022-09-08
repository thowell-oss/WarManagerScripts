
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Linq;

using UnityEngine;

using WarManager.Backend;
using WarManager.Unity3D;
using WarManager.Unity3D.Windows;

namespace WarManager
{
    [Notes.Author("The file picker")]
    public class FilePicker
    {
        private Sprite FileSprite => ActiveSheetsDisplayer.main.FileSprite;
        private Sprite FolderSprite => ActiveSheetsDisplayer.main.FolderSprite;
        private Sprite ExitSprite => ActiveSheetsDisplayer.main.DeleteSprite;
        private Sprite UpSprite => ActiveSheetsDisplayer.main.UpSprite;
        private Sprite BackSprite => ActiveSheetsDisplayer.main.BackSprite;

        private List<string> _foldersInDirectory = new List<string>();
        private List<string> _filesInDirectory = new List<string>();

        private string startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

        private Action<bool> resultingAction;
        public string SelectedPath = "";

        private string[] _selectedExtensions = new string[0];

        private bool _selectingFolder;

        private string _lastPath;

        /// <summary>
        /// The current path
        /// </summary>
        /// <value></value>
        private string _currentPath
        {
            get => _lastPath;

            set
            {
                if (Directory.Exists(value))
                {
                    _lastPath = value;
                    _backStack.Push(value);
                    ShowDirectory();
                }
                else
                    throw new DirectoryNotFoundException("Cannot find the directory " + value);
            }
        }

        /// <summary>
        /// Accessible when hitting the back button
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        private Stack<string> _backStack = new Stack<string>();

        /// <summary>
        /// Show the current directory
        /// </summary>
        private void ShowDirectory()
        {

            FileAttributes attr = File.GetAttributes(_currentPath);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryInfo currentFilePathInfo = new DirectoryInfo(_currentPath);

                if (!currentFilePathInfo.Exists)
                    throw new DirectoryNotFoundException("Cannot find folder " + _currentPath);
            }
            else
            {
                FileInfo currentFilePathInfo = new FileInfo(_currentPath);

                if (!currentFilePathInfo.Directory.Exists)
                    throw new DirectoryNotFoundException("Cannot find folder " + _currentPath);
            }

            _foldersInDirectory.Clear();
            _filesInDirectory.Clear();

            _foldersInDirectory.AddRange(Directory.GetDirectories(_currentPath));
            _foldersInDirectory.Sort();
            _filesInDirectory.AddRange(Directory.GetFiles(_currentPath));
            _filesInDirectory.Sort();

            List<SlideWindow_Element_ContentInfo> content = StartingUI();

            foreach (var x in _foldersInDirectory)
            {
                DirectoryInfo info = new DirectoryInfo(x);
                content.Add(new SlideWindow_Element_ContentInfo(info.Name, 0, (y) =>
                {
                    _currentPath = info.FullName;
                }, FolderSprite));
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));

            foreach (var x in _filesInDirectory)
            {
                FileInfo info = new FileInfo(x);

                bool found = true;

                if (_selectedExtensions.Length > 0)
                {
                    found = _selectedExtensions.Contains(info.Extension);
                }

                if (found)
                    content.Add(new SlideWindow_Element_ContentInfo(info.Name, 0, (y) =>
                    {
                        SelectPath(info.FullName);
                    }, FileSprite));
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));
            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Select the document
        /// </summary>
        /// <param name="path">the path of the document selected</param>
        /// <returns></returns>
        public void Init(string[] selectedExtensions, bool selectingFolder, Action<bool> result)
        {

            SlideWindowsManager.main.CloseReference(true);

            _lastPath = startingPath;
            resultingAction = result;

            if (selectedExtensions != null)
            {
                _selectedExtensions = selectedExtensions;
            }

            _selectingFolder = selectingFolder;

            LeanTween.delayedCall(1, () =>
            {
                ShowDirectory();
            });
        }

        public void SetStartingPath(string path)
        {
            if (Directory.Exists(path))
            {
                _lastPath = path;
            }
        }

        /// <summary>
        /// The starting User Interface that happens every time the window gets refreshed
        /// </summary>
        /// <returns></returns>
        private List<SlideWindow_Element_ContentInfo> StartingUI()
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Exit", 0, (x) => { Exit(); }, ExitSprite));
            content.Add(new SlideWindow_Element_ContentInfo("Up", 0, (x) => { Up(); }, UpSprite));
            content.Add(new SlideWindow_Element_ContentInfo("Back", 0, (x) => { Back(); }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(10));
            content.Add(new SlideWindow_Element_ContentInfo("Desktop", 0, (x) => { Desktop(); }, FolderSprite));
            content.Add(new SlideWindow_Element_ContentInfo("Documents", 0, (x) => { Documents(); }, FolderSprite));
            content.Add(new SlideWindow_Element_ContentInfo("Path", _currentPath, (x) =>
            {
                if (x != _currentPath)
                    NewDirectory(x);
            }));

            if (_selectingFolder)
                content.Add(new SlideWindow_Element_ContentInfo("Select This Folder", 0, (x) =>
                {
                    SelectPath(_currentPath);
                }, null));

            content.Add(new SlideWindow_Element_ContentInfo(10));
            return content;
        }


        /// <summary>
        /// New Directory
        /// </summary>
        private void NewDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                _currentPath = path;
            }
            else
            {
                MessageBoxHandler.Print_Immediate(path + " does not exist.", "Error");
            }
        }

        /// <summary>
        /// The back button functionality
        /// </summary>
        private void Back()
        {
            if (_backStack.Count > 0)
            {
                string path = _backStack.Pop();
                _lastPath = path;

                ShowDirectory();
            }
        }

        /// <summary>
        /// Go up one directory
        /// </summary>
        /// <param name="path"></param>
        private void Up()
        {
            DirectoryInfo info = Directory.GetParent(_currentPath);
            if (info != null)
            {
                string newPath = info.FullName;
                _currentPath = newPath;
            }
        }

        /// <summary>
        /// Select the path
        /// </summary>
        /// <param name="path"></param>
        private void SelectPath(string path)
        {

            if ((_selectingFolder && Directory.Exists(path)) || (!_selectingFolder && File.Exists(path)))
            {
                startingPath = path;
                SelectedPath = path;
                ActiveSheetsDisplayer.main.ViewReferences();
                if (resultingAction != null)
                    resultingAction(true);
            }
            else
            {
                if (!_selectingFolder && Directory.Exists(path) && !File.Exists(path))
                {
                    MessageBoxHandler.Print_Immediate("You selected a folder, but you need a file", "Error");
                }

                if (_selectingFolder && !Directory.Exists(path) && File.Exists(path))
                {
                    MessageBoxHandler.Print_Immediate("You selected a file, but you need a folder", "Error");
                }
            }
        }

        private void Documents()
        {
            _currentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        }

        private void Desktop()
        {
            _currentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        }

        /// <summary>
        /// Exit the file picker and cancel the operation
        /// </summary>
        private void Exit()
        {
            MessageBoxHandler.Print_Immediate("Are you sure you want to Exit?", "Question", (x) =>
            {
                if (x)
                {
                    if (resultingAction != null)
                        resultingAction(false);
                    ActiveSheetsDisplayer.main.ViewReferences();
                }
            });
        }
    }
}
