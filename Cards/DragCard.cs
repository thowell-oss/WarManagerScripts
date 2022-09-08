/* DragCard.cs
 * Author: Taylor Howell
 */

using System;

using UnityEngine;

namespace WarManager.Cards
{
	/// <summary>
	/// Handles dragging a card around the scene and shifting nearby objects 
	/// </summary>
	public class DragCard
	{
		/// <summary>
		/// Is the card being dragged?
		/// </summary>
		public bool IsDragging { get; private set; }

		/// <summary>
		/// The current card shift action
		/// </summary>
		private CardShift _currentShift;

		/// <summary>
		/// The card being dragged
		/// </summary>
		public Card CardBeingDragged { get; private set; }

		/// <summary>
		/// The original grid position of the card being dragged
		/// </summary>
		public Point OrigCardGridPosition { get; private set; }

		/// <summary>
		/// The closest grid position to the card being dragged
		/// </summary>
		public Point NearestCardGridPosition { get; private set; }

		/// <summary>
		/// The current world position that the card is at during the dragging phase
		/// </summary>
		public (float x, float y) CurrentDragPosition { get; private set; }

		/// <summary>
		/// The global offset determined by the GUI environment
		/// </summary>
		private (float x, float y) _globalOffset;

		/// <summary>
		/// the grid multipler determined by the GUI environment
		/// </summary>
		private (float x, float y) _gridMult;

		/// <summary>
		/// Has the dragging ended?
		/// </summary>
		private bool _hasEnded = false;

		/// <summary>
		/// The layer the shifts are taking place on
		/// </summary>
		private Layer _affectedLayer;

		/// <summary>
		/// Constructor - insert a location to find a card to drag
		/// </summary>
		/// <param name="loc">the location to start dragging</param>
		public DragCard(Point loc, Layer layer)
		{
			CardBeingDragged = CardUtility.GetCard(loc, layer);
		}

		/// <summary>
		/// Constructor - insert an id to find a card to drag
		/// </summary>
		/// <param name="id">the card to drag</param>
		public DragCard(string id)
		{
			CardBeingDragged = CardUtility.GetCard(id);
		}

		/// <summary>
		/// Constructor - insert an id to find a card to drag
		/// </summary>
		/// <param name="id">the card to drag</param>
		public DragCard(Card c)
		{
			if (c == null)
				throw new System.NullReferenceException("Card is null");

			InitSingle(c);
		}

		private void InitSingle(Card c)
		{
			// CardBeingDragged = c;
			// CardManager.RemoveCard(c);
			// OrigCardGridPosition = new Point(c.point.x, c.point.y);
		}

		/// <summary>
		/// Call in order to drag a card and interact with the scene
		/// </summary>
		/// <param name="dragPosition">the world location that the pointer is at</param>
		/// <param name="globalOffset">the global grid offset</param>
		/// <param name="gridMultiplier">the distance between the cards</param>
		public void Drag((float, float) dragPosition, Layer layer, (float, float) globalOffset, (float, float) gridMultiplier)
		{
			if (_hasEnded)
				throw new NotSupportedException("The dragging has ended. You need to instantiate a new DragCard() class");

			if (CardBeingDragged == null)
				throw new NullReferenceException("Cannot drag a null card. Try Instantiating a new DragCard() with a solid reference.");


			CurrentDragPosition = dragPosition;
			IsDragging = true;

			_globalOffset = globalOffset;
			_gridMult = gridMultiplier;

			CardBeingDragged.SetCardDrag(true);

			if (!ShiftCards(layer))
				if (_currentShift != null && _currentShift.ShiftGridPoint != CardLayout.GetCardGridLocation(dragPosition, (0, 0), globalOffset, gridMultiplier))
				{
					//Debug.Log("Canceling shift -- ");
					CancelShift();
				}
		}

		/// <summary>
		/// Shift the cards according to a specified point in the world. When (for example) the point is between two cards the cards will spread in order to make room
		/// </summary>
		///<returns>Returns true if the card was shifted, false if not</returns>
		private bool ShiftCards(Layer currentLayer)
		{
			Point nearestPoint = CardLayout.GetCardGridLocation(CurrentDragPosition, (0, 0), _globalOffset, _gridMult);

			Card c = CardUtility.GetCard(nearestPoint, currentLayer);

			if (c == null)
			{
				return false;
			}

			(float x, float y) nearestGlobalPoint = CardLayout.GetCardGlobalLocation(nearestPoint, (0, 0), _globalOffset, _gridMult);

			float x = nearestGlobalPoint.x - CurrentDragPosition.x;
			float y = nearestGlobalPoint.y - CurrentDragPosition.y;

			float absX = Math.Abs(x);
			float absY = Math.Abs(y);

			float xLimit = .3f * _gridMult.x;
			float yLimit = .2f * _gridMult.y;

			if ((absX > xLimit || absY > yLimit) && !(absX > xLimit && absY > yLimit))
			{

				Debug.DrawLine(Vector3.zero, new Vector2(nearestGlobalPoint.x, nearestGlobalPoint.y), Color.red);

				//to the left
				if (x > xLimit)
				{
					bool shiftCard = true;

					if (CardBeingDragged == c)
					{
						shiftCard = false;
					}

					if (shiftCard)
						CreateShift(c, new Point(1, 0));
				}

				//to the right
				if (-x > xLimit)
				{
					CreateShift(c, new Point(1, 0));
				}

				////below
				if (y > yLimit)
				{
					Debug.DrawLine(Vector3.zero, new Vector2(nearestGlobalPoint.x, nearestGlobalPoint.y), Color.green);

					CreateShift(c, new Point(0, -1));
				}
				else if (-y > yLimit) //above
				{
					Debug.DrawLine(Vector3.zero, new Vector2(nearestGlobalPoint.x, nearestGlobalPoint.y), Color.red);

					CreateShift(c, new Point(0, -1));
				}
			}

			NearestCardGridPosition = nearestPoint;

			return true;
		}


		/// <summary>
		/// Handles shifting of the cards if applicable
		/// </summary>
		/// <param name="shiftPoint">the grid location where the shift will take place</param>
		/// <param name="shiftVector">the shift direction of the card</param>
		private void CreateShift(Card affectedCard, Point shiftVector)
		{
			if (affectedCard == null)
				throw new NullReferenceException("Card cannot be null when making a shift");

			if (_currentShift != null && affectedCard != _currentShift.AffectedCard)
			{
				CancelShift();
				_currentShift = new CardShift(affectedCard, shiftVector);
			}
			else if(_currentShift == null)
			{
				_currentShift = new CardShift(affectedCard, shiftVector);
			}
			else
			{
				CancelShift();
			}

		}

		/// <summary>
		/// Move cards back to their origial spot (if applicable)
		/// </summary>
		private void CancelShift()
		{
			if (_currentShift != null)
			{
				//Debug.Log("shift canceled");

				_currentShift.ShiftCardsBack();
				_currentShift = null;
			}
		}

		/// <summary>
		/// End the drag and place the card in the new position
		/// </summary>
		/// <returns>returns the new location of the card</returns>
		public Point EndDrag()
		{
			End(false);
			//CardManager.MoveCard(CardBeingDragged, CardLayout.GetCardGridLocation(CurrentDragPosition, (0, 0), _globalOffset, _gridMult), _affectedLayer, CardBeingDragged.SheetID);

			//Debug.Log("End drag");

			// if (GeneralSettings.FillGapAfterShiftFromBottom && CardManager.GetCard(OrigCardGridPosition, _affectedLayer) == null)
			// {
			// 	CardManager.ShiftCards(new Point(OrigCardGridPosition.x, OrigCardGridPosition.y - 1), _affectedLayer, new Point(0, 1), CardBeingDragged.SheetID, false);
			// }

			// if(GeneralSettings.FillGapAfterShiftFromSide && CardManager.GetCard(OrigCardGridPosition, _affectedLayer) == null)
			// {
			// 	CardManager.ShiftCards(new Point(OrigCardGridPosition.x + 1, OrigCardGridPosition.y), _affectedLayer, new Point(-1, 0), CardBeingDragged.SheetID, false);
			// }

			return NearestCardGridPosition;
		}

		/// <summary>
		/// Cancel the drag and place the card in the original position
		/// </summary>
		public void CancelDrag()
		{
			End(true);
		}

		/// <summary>
		/// internal end of dragging the card
		/// </summary>
		private void End(bool cancelShift)
		{
			// CardManager.InsertCard(CardBeingDragged, new Point(0,0));
			IsDragging = false;

			if (cancelShift)
				CancelShift();

			_hasEnded = true;
			CardBeingDragged.SetCardDrag(false);
			//CardBeingDragged.Animator.SetCardHover();
		}
	}
}
