using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class FlexibleLayoutGroup : LayoutGroup
{
	public enum FitType
	{
		Uniform,
		Width,
		Height,
		FixedRows,
		FixedColumns
	}
	public FitType fitType;

	public int columns;
	public int rows;
	public Vector2 cellsize;
	public Vector2 spacing;

	bool fitX, fitY;

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();

		if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
		{

			fitX = true;
			fitY = true;

			float sqrt = Mathf.Sqrt(transform.childCount);
			rows = Mathf.CeilToInt(sqrt);
			columns = Mathf.CeilToInt(sqrt);
		}

		if(fitType == FitType.Width || fitType == FitType.FixedColumns)
		{
			rows = Mathf.CeilToInt(transform.childCount / (float)columns);
		}

		if(fitType == FitType.Height || fitType == FitType.FixedRows)
		{
			columns = Mathf.CeilToInt(transform.childCount / (float)rows);
		}

		float parentWidth = rectTransform.rect.width;
		float parentHeight = rectTransform.rect.height;

		float cellWidth = parentWidth / (float)columns - ((spacing.x / (float)columns) * 2) - (padding.left / (float)columns) - (padding.right / (float)columns);
		float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * 2) - (padding.top / (float)rows) - (padding.bottom / (float)rows);

		cellsize.x = fitX ? cellWidth : cellsize.x;
		cellsize.y = fitY ? cellHeight : cellsize.y;



		int columnCount = 0;
		int rowCount = 0;

		for (int i = 0; i < rectChildren.Count; i++)
		{
			rowCount = i / columns;
			columnCount = i % columns;

			var item = rectChildren[i];

			var xPos = (cellsize.x * columnCount) + (spacing.x * columnCount) + padding.left;
			var yPos = (cellsize.y * rowCount) + (spacing.y * rowCount) + padding.top;

			SetChildAlongAxis(item, 0, xPos, cellsize.x);
			SetChildAlongAxis(item, 1, yPos, cellsize.y);
		}

	}

	public override void CalculateLayoutInputVertical()
	{

	}

	public override void SetLayoutHorizontal()
	{

	}

	public override void SetLayoutVertical()
	{

	}
}
