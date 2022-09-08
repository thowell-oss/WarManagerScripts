/* UnityCardBackgroundHandler.cs
 * Author: Taylor Howell
 */

using System;


using UnityEngine;

using WarManager.Cards;

namespace WarManager.Unity3D
{
	/// <summary>
	/// Handles communication from the background object
	/// </summary>
	public class UnityCardBackgroundHandler : MonoBehaviour
	{
		/// <summary>
		/// The parent object
		/// </summary>
		public GameObject ParentObject { get; set; }

		/// <summary>
		/// The card display script
		/// </summary>
		public UnityCardDisplay CardDisplay { get; set; }

		/// <summary>
		/// The current scale of the card background
		/// </summary>
		public Vector2 CurentScale
		{
			get
			{
				return sr.size;
			}
		}

		public RectTransform CardCanvas;

		/// <summary>
		/// activate a background that slowly fades opacity
		/// </summary>
		public bool FlashingBorder = false;

		/// <summary>
		/// Tuns on a background border with full opacity
		/// </summary>
		public bool SolidBorder = false;

		/// <summary>
		/// The border sprite
		/// </summary>
		public SpriteRenderer BorderObject;

		/// <summary>
		/// the size of the border
		/// </summary>
		public float borderSize = .2f;

		/// <summary>
		/// used for flashing purposes - internal only
		/// </summary>
		private bool flashDown;

		/// <summary>
		/// The backgrond sprite renderer
		/// </summary>
		private SpriteRenderer sr;

		public SpriteRenderer dataSetSprite;

		private BoxCollider2D srCol;

		private void Awake()
		{
			ParentObject = transform.parent.gameObject;
			CardDisplay = ParentObject.GetComponent<UnityCardDisplay>();
			BorderObject.gameObject.SetActive(false);
			sr = gameObject.GetComponent<SpriteRenderer>();
			srCol = GetComponent<BoxCollider2D>();
		}

		private void Update()
		{
			if(CardCanvas != null)
			{
				CardCanvas.sizeDelta = new Vector2(BorderObject.size.x * 100, BorderObject.size.y * 100);
			}

			if (FlashingBorder)
			{
				BorderObject.gameObject.SetActive(true);

				if (flashDown)
				{

					if (BorderObject.color.a > 0)
					{
						float x = -.9f * Time.deltaTime;
						AdjustBorderColorAlpha(x);
					}
					else
					{
						flashDown = false;
					}
				}
				else
				{
					if (BorderObject.color.a < 1)
					{
						float x = .9f * Time.deltaTime;
						AdjustBorderColorAlpha(x);
					}
					else
					{
						flashDown = true;
					}
				}

			}
			else if (SolidBorder)
			{
				BorderObject.gameObject.SetActive(true);
				BorderObject.color = new Color(BorderObject.color.r, BorderObject.color.g, BorderObject.color.b, 1);
			}
			else
			{
				BorderObject.gameObject.SetActive(false);
			}

		}

		/// <summary>
		/// The scales the card in 2D (x,y)
		/// </summary>
		/// <param name="scale">the given scale</param>
		public void ScaleCard(Vector2 scale)
		{
			if (sr != null)
			{
				sr.drawMode = SpriteDrawMode.Sliced;
				dataSetSprite.drawMode = SpriteDrawMode.Sliced;
				BorderObject.drawMode = SpriteDrawMode.Sliced;

				sr.size = scale;
				srCol.size = scale;

				dataSetSprite.size = new Vector2(10 * scale.x, 10 * scale.y);
				BorderObject.size = new Vector2(scale.x + borderSize, scale.y + borderSize);


			}
		}


		/// <summary>
		/// Set the color of the border object - ignores alpha (only rgb)
		/// </summary>
		/// <param name="c">the given color</param>
		public void SetBorderColor(Color c)
		{
			BorderObject.color = new Color(c.r, c.g, c.b, BorderObject.color.a);
		}

		/// <summary>
		/// Set the color of the background object - ignores alpha (only rgb)
		/// </summary>
		/// <param name="c">the given color</param>
		public void SetBackgroundColor(Color c)
		{
			sr.color = new Color(c.r, c.g, c.b, BorderObject.color.a);
		}

		/// <summary>
		/// Set the color of the dataset object - ignores alpha (only rgb)
		/// </summary>
		/// <param name="c">the new given color of the data set</param>
		public void SetDataSetColor(Color c)
		{
			dataSetSprite.color = new Color(c.r, c.g, c.b, BorderObject.color.a);

		}

		/// <summary>
		/// increments (or decrements) the alpha color by the given amount
		/// </summary>
		/// <param name="amt">given amount</param>
		private void AdjustBorderColorAlpha(float amt)
		{
			//amt = Mathf.Clamp(amt, 0, 1);

			Color c = new Color(BorderObject.color.r, BorderObject.color.g, BorderObject.color.b, BorderObject.color.a + amt);
			BorderObject.color = c;
		}

		/// <summary>
		/// Turns the border on, sets a thickness and changes the color
		/// </summary>
		/// <param name="thickness">a normalized thicknes between 0 and 1</param>
		/// <param name="color">the color of the border (alpha is ignored)</param>
		public void SetSolidBorder(float thickness, Color color)
		{
			FlashingBorder = false;
			borderSize = Mathf.Clamp(thickness, 0, 1) * .25f;
			SetBorderColor(color);
			SolidBorder = true;
		}

		private void OnMouseOver()
		{
			if (ToolsManager.SelectedTool != ToolTypes.None)
				CardDisplay.OnMouseOver();
		}

		private void OnMouseExit()
		{
			if (ToolsManager.SelectedTool != ToolTypes.None)
				CardDisplay.OnMouseExit();
		}

		private void OnMouseDrag()
		{
			if (ToolsManager.SelectedTool != ToolTypes.None)
				CardDisplay.OnMouseDrag();
		}

		private void OnMouseDown()
		{
			if (ToolsManager.SelectedTool != ToolTypes.None)
				CardDisplay.OnMouseDown();
		}

		private void OnMouseUp()
		{
			if (ToolsManager.SelectedTool != ToolTypes.None)
				CardDisplay.OnMouseUp();
		}
	}
}
