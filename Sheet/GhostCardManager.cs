
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;

using WarManager.Unity3D;
using WarManager.Unity3D.Windows;
using StringUtility;

namespace WarManager
{
    public static class GhostCardManager
    {
        private static GhostCardBehavior GhostCard
        {
            get
            {
                return GhostCardBehavior.Main;
            }
        }

        /// <summary>
        /// Is the ghost card visible?
        /// </summary>
        /// <value></value>
        public static bool Visible
        {
            get
            {
                return GhostCard.GhostCardVisible;
            }
            set
            {
                GhostCard.GhostCardVisible = value;
            }
        }

        public static List<Point> SelectedGridPoints { get; private set; }

        public delegate void ghostCardAction_delegate(Rect ghostCardBounds);
        public static event ghostCardAction_delegate OnGhostCardUpdate;

        private static Rect _bounds;

        /// <summary>
        /// Were some cards being dragged?
        /// </summary>
        /// <value></value>
        public static bool WasDragging { get; set; }

        public static void Results(Rect bounds, int button)
        {
            if (bounds == null)
            {
                return;
            }

            _bounds = bounds;

            List<Point> spaces = bounds.SpacesTaken();
            spaces.Sort(delegate (Point a, Point b)
            {
                return Point.SortByDirection(a, b, CardUtility.DefaultShiftDirection);
            });

            if (button == 0 || (button == 0 && ToolsManager.SelectedTool == ToolTypes.Highlight))
            {
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    SheetsCardSelectionManager.Main.DeselectCurrent();
                }

                if (SheetsManager.TryGetCurrentSheet(out var sheet))
                {
                    if (spaces.Count > 1)
                    {
                        foreach (var space in spaces)
                        {
                            var card = CardUtility.GetCard(space, sheet.CurrentLayer, sheet.ID);

                            if (card != null)
                            {
                                card.Select(true);
                            }
                        }
                    }
                    else if (spaces.Count == 1)
                    {
                        var card = CardUtility.GetCard(spaces[0], sheet.CurrentLayer, sheet.ID);

                        if (card != null)
                        {
                            if (!card.Selected)
                            {
                                card.Select(true);
                            }
                            else
                            {
                                card.Deselect();
                            }
                        }
                    }
                }

                return;
            }

            // Debug.Log("ghost card manager " + button + " " + ToolsManager.SelectedTool);

            if (button == 1 && ToolsManager.SelectedTool == ToolTypes.Edit)
            {
                if (WasDragging && InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                {
                    WasDragging = false;
                    return;
                }

                // Debug.Log("ghost card manager");

                if (SheetsManager.TryGetCurrentSheet(out var sheet))
                {
                    // Debug.Log("ghost card manager");

                    List<(string, System.Action, bool)> buttonActions = new List<(string, System.Action, bool)>();
                    if (spaces.Count == 1 && spaces[0] != null)
                    {

                        var card = CardUtility.GetCard(spaces[0], sheet.CurrentLayer, sheet.ID);

                        bool cardLocked = false;

                        if (card != null && !card.DataSet.SelectedView.CanViewCard)
                        {
                            card = null;
                            cardLocked = true;
                        }

                        // Debug.Log("ghost card manager");

                        var cards = SheetsCardSelectionManager.Main.GetSelectedCards(SheetsManager.CurrentSheetID);
                        var columnInfos = CardUtility.GetCommonColumnInfoFromCards(cards, SearchType.valueType, ColumnInfo.GetValueTypeOfPhone);


                        if (card == null)
                        {
                            buttonActions.Add(("Add...", () => { GetNewCardSetup(bounds); }, !cardLocked));

                            if (SheetsManager.TryGetCurrentSheet(out var currentSheet))
                            {

                                buttonActions.Add(("Add Title", () =>
                                {
                                    CardUtility.TryDropTitleCard(currentSheet, spaces[0], sheet.CurrentLayer, out var id);
                                }, true
                                ));

                                buttonActions.Add(("Add Note", () =>
                                {
                                    CardUtility.TryDropNoteCard(currentSheet, spaces[0], sheet.CurrentLayer, out var id);
                                }, true
                                ));
                            }
                            

                            buttonActions.Add(("Paste", () => { CopyPasteDuplicate.Main.Paste(); LeanTween.delayedCall(.25f, CopyPasteDuplicate.Main.CheckVisiblity); }, CopyPasteDuplicate.Main.CanPaste && !cardLocked));
                            buttonActions.Add(("Message (SMS)", () =>
                            {

                                List<string> phoneNumbers = new List<string>();

                                foreach (var x in columnInfos)
                                {
                                    if (x.Entry.TryGetValueWithHeader(x.HeaderName, out DataValue value))
                                    {
                                        //Debug.Log(value.HeaderName + " " + value.CellLocation + " " + value.Value.ToString());

                                        if (value.ValueType == value.ParseToPhone())
                                        {
                                            //phoneNumbers.Add("+19137497477");
                                            phoneNumbers.Add(value.ParseToPhone().FullNumberUS);
                                        }
                                    }
                                }

                                EditTextMessageBoxController.OpenModalWindow("", "Edit Message (SMS)", (x) =>
                                {
                                    if (x != null && x.Length > 0)
                                    {
                                        var sms = new WarManager.Sharing.TwilioSMSHandler();
                                        foreach (var y in phoneNumbers)
                                        {
                                            if (y != null && y.Length > 0)
                                            {
                                                sms.SendMessage(x, y, false);
                                            }
                                        }

                                        LeanTween.delayedCall(1, () =>
                                        {
                                            MessageBoxHandler.Print_Immediate("Done", "Note");
                                        });
                                    }
                                });

                            }, columnInfos.Count > 0));

                            buttonActions.Add(("Set Drop Point", () => { WarManager.SheetDropPointManager.SetDropPoint(spaces[0]); }, !cardLocked));

                        }
                        else
                        {

                            if (!card.Selected)
                            {
                                buttonActions.Add(("Select", () =>
                            {
                                card.Deselect();
                                card.Select(false);

                                // Debug.Log("ghost card manager");

                            }, true));
                            }
                            else
                            {
                                buttonActions.Add(("Deselect", () =>
                                {
                                    card.Deselect();
                                }, true));
                            }

                            buttonActions.Add(("See More", () =>
                            {
                                try
                                {
                                    DataSetViewer.main.ShowDataEntryInfo(cards[0].Entry, () =>
                                    {
                                        if (cards[0] != null)
                                            DataSetViewer.main.ShowDataSet(cards[0].Entry.DataSet.ID, ActiveSheetsDisplayer.main.ViewReferences);
                                    }, cards[0].Entry.DataSet.DatasetName.SetStringQuotes());
                                }
                                catch (System.Exception ex)
                                {
                                    MessageBoxHandler.Print_Immediate("Could not show card details " + ex.Message, "Error");
                                }

                            }, true));

                            if (SheetsCardSelectionManager.Main.CardTotal > 0)
                            {
                                buttonActions.Add(("Copy", () =>
                                {
                                    CopyPasteDuplicate.Main.CopySelectedCards(spaces[0]);
                                }, true));
                            }
                            else
                            {
                                buttonActions.Add(("Copy", () =>
                                {
                                    CopyPasteDuplicate.Main.CopyCard(card, spaces[0]);
                                }, true));
                            }

                            List<string> phoneNumbers = new List<string>();

                            foreach (var x in columnInfos)
                            {
                                if (x.Entry.TryGetValueWithHeader(x.HeaderName, out DataValue value))
                                {
                                    //Debug.Log(value.HeaderName + " " + value.CellLocation + " " + value.Value.ToString());
                                    phoneNumbers.Add(value.ParseToPhone().FullNumberUS);
                                }
                            }

                            buttonActions.Add(("Message (SMS)", () =>
                                {


                                    EditTextMessageBoxController.OpenModalWindow("", "Edit Message (SMS)", (x) =>
                                    {
                                        if (x != null && x.Length > 0)
                                        {
                                            var sms = new WarManager.Sharing.TwilioSMSHandler();
                                            foreach (var y in phoneNumbers)
                                            {
                                                if (y != null && y.Length > 0)
                                                {
                                                    sms.SendMessage(x, y, false);
                                                }
                                            }

                                            LeanTween.delayedCall(1, () =>
                                            {
                                                MessageBoxHandler.Print_Immediate("Done", "Note");
                                            });
                                        }
                                    });

                                }, phoneNumbers.Count > 0));



                            System.Action datasetViewer = () =>
                            {
                                DataSetViewer.main.ShowDataSetProperties(() =>
                                        {
                                            ActiveSheetsDisplayer.main.ViewReferences();
                                            SlideWindowsManager.main.OpenReference(true);
                                        }, "References", card.DataSet);
                            };

                            buttonActions.Add(("Change Views...", () =>
                            {
                                DataSetViewer.main.ShowViews(datasetViewer, card.DataSet.DatasetName + " Properties", card.DataSet);
                            }, card.DataSet.Views.Count > 1 && cards.Count == 1));

                            buttonActions.Add(($"Isolate {card.DataSet.DatasetName}", () =>
                            {
                                CardUtility.IsolateDataSetOnSheet(card.DataSet);
                            }, true));



                            bool canShiftDown = CardUtility.CanShift(cards, Point.down, 1);

                            buttonActions.Add(("Shift Down", () =>
                            {
                                //bool x = CardUtility.TryShiftCard(card, Point.down, 1, true);
                                CardUtility.TryShiftCards(cards, Point.down, 1);
                            }, canShiftDown && card.DataSet.SelectedView.CanEditCard && card.DataSet.SelectedView.CanViewCard));


                            bool canshiftRight = CardUtility.CanShift(cards, Point.right, 1);

                            buttonActions.Add(("Shift Right", () =>
                            {
                                //bool x = CardUtility.TryShiftCard(card, Point.right, 1, true);
                                CardUtility.TryShiftCards(cards, Point.right, 1);
                            }, canshiftRight && card.DataSet.SelectedView.CanEditCard && card.DataSet.SelectedView.CanViewCard));


                            if (SheetsCardSelectionManager.Main.CardTotal > 0 && card.Selected)
                            {
                                buttonActions.Add(("Remove Selected", () =>
                                    {
                                        var cards = SheetsCardSelectionManager.Main.GetCurrentSelectedCards();

                                        for (int i = cards.Count - 1; i >= 0; i--)
                                        {
                                            if (cards[i].CanRemove)
                                            {
                                                cards[i].Remove();
                                            }
                                        }
                                    }, true));
                            }
                            else
                            {
                                buttonActions.Add(("Remove", () =>
                                {
                                    if (card.CanRemove)
                                    {
                                        card.Remove();
                                    }
                                }, card.CanRemove));
                            }

                        }

                        buttonActions.Add(("", () => { }, false));

                        buttonActions.Add(("New Sheet", ActiveSheetsDisplayer.main.NewSheet, true));
                        buttonActions.Add(("Undo", SimpleUndoRedoManager.main.Undo, SimpleUndoRedoManager.main.UndoCount > 0));
                        buttonActions.Add(("Redo", SimpleUndoRedoManager.main.Redo, SimpleUndoRedoManager.main.RedoCount > 0));

                        // Debug.Log("ghost card manager");

                        WarManager.Unity3D.PickMenu.PickMenuManger.main.OpenPickMenu(buttonActions);

                        if (OnGhostCardUpdate != null)
                        {
                            OnGhostCardUpdate(bounds);
                        }
                        // Debug.Log("ghost card manager");
                    }
                    else
                    {
                        buttonActions.Add(("Add Cards", () => { GetNewCardSetup(bounds); }, true));
                        WarManager.Unity3D.PickMenu.PickMenuManger.main.OpenPickMenu(buttonActions);

                    }
                    return;

                    // var dataset = GetRandomDataSet();
                    // DataPiece p = dataset.GetData((int)GetRandomRepIDStr(dataset));

                    // Card c = new Card(spaces[0], System.Guid.NewGuid().ToString(), SheetsManager.CurrentSheetID, "d", dataset.ID, GetRandomRepIDStr(dataset).ToString());

                    // if (spaces.Count > 1 && c.CanStretch)
                    // {
                    //     CardManager.TryAddStretchCard(c, bounds);
                    // }
                    // else if (spaces.Count == 1)
                    // {
                    //     CardManager.TryAddCard(c);
                    // }              
                }
            }
        }

        private static void SelectCard()
        {
            var spaces = SelectedGridPoints;

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                if (spaces.Count > 1)
                {
                    foreach (var space in spaces)
                    {
                        var card = CardUtility.GetCard(space, sheet.CurrentLayer, sheet.ID);

                        if (card != null)
                        {
                            card.Select(true);
                        }
                    }
                }
                else if (spaces.Count == 1)
                {
                    var card = CardUtility.GetCard(spaces[0], sheet.CurrentLayer, sheet.ID);

                    if (card != null)
                    {
                        if (!card.Selected)
                        {
                            card.Select(true);
                        }
                        else
                        {
                            card.Deselect();
                        }
                    }
                }
            }
        }

        private static void GetNewCardSetup(Rect bounds)
        {
            if (SheetsManager.CurrentSheetID == null || SheetsManager.CurrentSheetID == string.Empty)
                return;

            List<Point> dropPoints = bounds.SpacesTaken();

            DataSetViewer.main.DropCardMainMenu(SheetsManager.CurrentSheetID, GetCardCallBack, dropPoints);
            // SheetDropPointManager.SetNewCustomPoints(SheetsManager.CurrentSheetID, dropPoints);

            // Pointf worldCenter = bounds.GetWorldCenter(SheetsManager.GetWarGrid(SheetsManager.CurrentSheetID));
            // WarManagerCameraController.MainController.MoveCamera(worldCenter);
        }

        public static void NewCard()
        {

        }

        private static void GetCardCallBack(string dataSetId, string repId)
        {
            // Debug.Log("Added Card " + repId);
        }

        /// <summary>
        /// Get a random repId from a dataset
        /// </summary>
        /// <returns></returns>
        private static long GetRandomRepIDStr(DataSet set)
        {
            // Debug.Log(set.DataCount);
            long repId = Random.Range(0, set.DataCount);
            return repId;
        }
    }
}
