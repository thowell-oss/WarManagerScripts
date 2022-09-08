using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    [Notes.Author("quick action data to front end")]
    public class QuickActionData
    {
        /// <summary>
        /// name of the action
        /// </summary>
        /// <value></value>
        public string Name { get; private set; }

        /// <summary>
        /// The Similarity Score
        /// </summary>
        /// <value></value>
        public double Score { get; private set; }

        /// <summary>
        /// the primary action 
        /// </summary>
        /// <value></value>
        public Action PrimaryAction { get; private set; }

        /// <summary>
        /// the type of action icon
        /// </summary>
        /// <value></value>
        public Sprite TypeIcon { get; private set; }

        /// <summary>
        /// the type of action color
        /// </summary>
        /// <value></value>
        public Color TypeColor { get; private set; }

        /// <summary>
        /// list of secondary actions
        /// </summary>
        /// <value></value>
        private List<(string name, Action action, bool enabled)> _secondaryActions { get; set; }

        /// <summary>
        /// The description of the action
        /// </summary>
        /// <value></value>
        public string Description { get; private set; }

        /// <summary>
        /// the count of secondary actions
        /// </summary>
        /// <value></value>
        public int SecondaryActionsCount
        {
            get => _secondaryActions.Count;
        }

        /// <summary>
        /// enumerate through the list of secondary actions
        /// </summary>
        /// <value></value>
        public IEnumerable<(string name, Action action, bool enabled)> SecondaryActions
        {
            get => _secondaryActions;
        }


        /// <summary>
        /// The secondary actions array
        /// </summary>
        /// <value></value>
        public (string name, Action action, bool enabled)[] SecondaryActionsArray
        {
            get => _secondaryActions.ToArray();
        }

        /// <summary>
        /// Place in the suggestion stack in the quick actions UI
        /// </summary>
        /// <value></value>
        public QuickActionData SuggestionData { get; set; } = null;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">the name of the action</param>
        /// <param name="primaryAction">the primary action</param>
        /// <param name="typeIcon">the type icon</param>
        /// <param name="secondaryActions">list of secondary actions</param>
        public QuickActionData(double score, string name, string description, Action primaryAction, Sprite typeIcon, Color typeColor, List<(string name, Action action, bool enabled)> secondaryActions)
        {

            if (name == null)
                throw new NullReferenceException("the name cannot be null");

            if (name == string.Empty)
                name = "<empty>";

            if (primaryAction == null)
                throw new NullReferenceException("the primary action cannot be null");

            if (typeIcon == null)
                throw new NullReferenceException("the type icon cannot be null");

            if (typeColor == null)
                throw new NullReferenceException("the type color is null");

            if (secondaryActions == null)
                throw new NullReferenceException("the secondary actions cannot be null");

            if (description == null)
                throw new NullReferenceException("the description is null");

            if (description == string.Empty)
                description = "<empty description>";

            Name = name;
            PrimaryAction = primaryAction;
            TypeIcon = typeIcon;
            TypeColor = typeColor;
            _secondaryActions = secondaryActions;

            Description = description;

            Score = score;
        }

        /// <summary>
        /// Adds an action to all primary and secondary actions
        /// </summary>
        /// <param name="action">the action</param>
        public void AddAction(Action action)
        {
            PrimaryAction = () => { PrimaryAction(); action(); };

            for (int i = 0; i < SecondaryActionsCount; i++)
            {
                SecondaryActionsArray[i].action = () => { SecondaryActionsArray[i].action(); action(); };
            }
        }
    }
}
