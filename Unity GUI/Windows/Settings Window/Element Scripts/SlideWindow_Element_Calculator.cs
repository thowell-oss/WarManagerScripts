

using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

using WarManager.Cards;


namespace WarManager.Unity3D.Windows
{
    public class SlideWindow_Element_Calculator : MonoBehaviour, ISlideWindow_Element
    {
        public SlideWindow_Element_ContentInfo info { get; set; }
        public GameObject targetGameObject => gameObject;

        public string SearchContent
        {
            get
            {
                return inputField.text + info.ElementType.ToString();
            }
        }

        decimal a;
        decimal b;
        decimal result;

        public bool focused = true;

        public TMP_InputField inputField;

        private CalculatorEntry _strat;

        private bool started = false;

        private List<Card> selectedCards = new List<Card>();

        private List<int> cardActionSelection = new List<int>();

        void Start() => UpdateElement();

        public void UpdateElement()
        {
            if (!started)
            {
                StartCoroutine(UpdateSelectList());
            }
        }

        IEnumerator UpdateSelectList()
        {
            started = true;

            while (true)
            {
                yield return new WaitForSeconds(.25f);

                selectedCards = SheetsCardSelectionManager.Main.GetCurrentSelectedCards();
                WindowContentQueue newContent = new WindowContentQueue();
                for (int i = 0; i < selectedCards.Count; i++)
                {
                    newContent.EnqeueContent(new SlideWindow_Element_ContentInfo("Card Name", i, (x) => SelectedCard(x)));
                }
                SlideWindowsManager.main.Properties.AddContent(newContent);
            }
        }

        void Update()
        {
            if (!focused)
                return;

            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                Add();
            }

            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                Subtract();
            }

            if (Input.GetKeyDown(KeyCode.Asterisk) || Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                Multiply();
            }

            if (Input.GetKeyDown(KeyCode.Slash) || Input.GetKeyDown(KeyCode.KeypadDivide))
            {
                Divide();
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadEquals))
            {
                ButtonEquals();
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                BackSpace();
            }
        }

        private void SelectedCard(int cardSelected)
        {

        }

        public void AddText(string text)
        {
            if (inputField.text.Length < inputField.characterLimit)
                inputField.text += text;
        }

        public void Add()
        {
            Entry(CalculatorEntry.add);
        }

        public void Subtract()
        {
            Entry(CalculatorEntry.subtract);
        }

        public void Multiply()
        {
            Entry(CalculatorEntry.multiply);
        }

        public void Divide()
        {
            Entry(CalculatorEntry.divide);
        }

        public void CE()
        {
            Entry(CalculatorEntry.clearEntry);
        }

        public void C()
        {
            Entry(CalculatorEntry.clear);
        }

        public void BackSpace()
        {
            if (inputField.text.Length > 0)
            {
                inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            }
            else
            {
                inputField.text = "";
            }
        }

        public void NegativePositiveNumberToggle()
        {

            if (inputField.text.Length == 0 || !inputField.text.Contains("-"))
            {
                inputField.text = "-" + inputField.text;
            }
            else
            {
                if (inputField.text.Length > 1)
                {
                    inputField.text = inputField.text.Remove(0, 1);
                }
                else
                {
                    inputField.text = "";
                }
            }
        }

        public void ButtonEquals()
        {
            Entry(CalculatorEntry.equals);
        }

        public void Entry(CalculatorEntry entry)
        {
            switch (entry)
            {
                case CalculatorEntry.add:
                    GetFirst(inputField.text, CalculatorEntry.add);
                    break;

                case CalculatorEntry.subtract:
                    GetFirst(inputField.text, CalculatorEntry.subtract);
                    break;

                case CalculatorEntry.multiply:
                    GetFirst(inputField.text, CalculatorEntry.multiply);
                    break;

                case CalculatorEntry.divide:
                    GetFirst(inputField.text, CalculatorEntry.divide);
                    break;

                case CalculatorEntry.equals:
                    Equals(inputField.text);
                    break;

                case CalculatorEntry.clearEntry:
                    ClearEntry();
                    break;

                case CalculatorEntry.clear:
                    Clear();
                    break;

                default:

                    break;
            }
        }

        private void GetFirst(string aText, CalculatorEntry strat)
        {
            _strat = strat;

            if (decimal.TryParse(aText, out decimal res))
            {
                a = res;
                inputField.text = "";
            }
            else
            {
                inputField.text = "Error!";
            }
        }

        private void Clear()
        {
            a = 0;
            b = 0;
            inputField.text = "";
        }
        private void ClearEntry()
        {
            inputField.text = "";
        }

        private void Equals(string bText)
        {
            if (decimal.TryParse(bText, out decimal res))
            {
                b = res;

                switch (_strat)
                {
                    case CalculatorEntry.subtract:
                        result = a - b;
                        break;

                    case CalculatorEntry.multiply:
                        result = a * b;
                        break;

                    case CalculatorEntry.divide:
                        result = a / b;
                        break;

                    default:
                        result = a + b;
                        break;
                }

                inputField.text = result.ToString();
                b = 0;
                a = result;
            }
            else
            {
                inputField.text = "Error!";
            }
        }
    }

    public enum CalculatorEntry
    {
        add,
        subtract,
        multiply,
        divide,
        equals,
        clearEntry,
        clear,
        none,
    }
}
