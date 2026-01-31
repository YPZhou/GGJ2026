using System;
using System.Collections.Generic;
using Godot;

public partial class Cat : Sprite2D
{
	[Export] public double San { get; private set; } // 小猫san值
	[Export] double HeadMoveInterval; // 猫头瞬移间隔
	[Export] double[] HeadYOffsets; // 可在 Inspector 中编辑
	[Export] double HeadXRange; // X 轴随机偏移范围（±）
	[Export] Node2D catHead; // 猫头节点
	[Export] Node catNecksParent; // 猫脖子父节点
	[Export] Texture2D catNeckTexture; // 猫脖子图片
	[Export] Game game;

	bool IsMaosked = false;

	public bool IsAlive => San > 0;

	public TVStatus CurrentStatus => TV.CurrentStatus;

	// 猫头瞬移状态
	double headTimer;
	int headIndex; // 索引到 HeadYOffsets
	double initialY;
	double initialX;
	double currentX;
	Line2D headCurve;

	Vector2 currentHeadTargetPos;
	Vector2 currentCp1, currentCp2;
	Vector2 targetCp1, targetCp2;
	Vector2 neckEndOffset;

	[Export] float headSmoothingSpeed = 5.0f;
	[Export] int neckSegments = 60; // Now used as a fallback or limit
	[Export] float curveTolerance = 2.0f;

	[Export] Area2D interactArea;
	[Export] Gizmo gizmo;

	void CheckRes()
	{
		if (catHead == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Cat head is null, set Cat head for Cat.catHead in the editor.");
		}
		if (catNecksParent == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Cat necks parent is null, set Cat necks parent for Cat.catNecksParent in the editor.");
		}
		if (catNeckTexture == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Cat neck texture is null, set Cat neck texture for Cat.catNeckTexture in the editor.");
		}
		if (interactArea == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Interact area is null, set Interact area for Cat.interactArea in the editor.");
		}
		if (gizmo == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Gizmo is null, set Gizmo for Cat.gizmo in the editor.");
		}
	}

	public override void _Ready()
	{
		base._Ready();

		// Calculate offset from Head to NeckEnd
		if (catHead != null && NeckEnd != null)
			neckEndOffset = NeckEnd.GlobalPosition - catHead.GlobalPosition;
		else
			neckEndOffset = Vector2.Zero;

		currentHeadTargetPos = catHead.Position;
		

		San = 100;

		CheckRes();

		
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (IsAlive && game.CurrentGameState == GameState.Playing)
		{
			if (CurrentStatus == TVStatus.GOOD)
			{
				
			}
			else if (CurrentStatus == TVStatus.Fool)
			{
				
			}

			if (MoveHead(delta))
			{
				
			}
			
			TickSan(delta, CurrentStatus);
		}
	}

	Vector2 GenerateRandomCatHeadPosition()
	{
		double targetOffset = 0.0;
		if (HeadYOffsets != null && HeadYOffsets.Length > 0)
		{
			int index = Random.Shared.Next(HeadYOffsets.Length);
			targetOffset = HeadYOffsets[index];
		}
		double randomX = initialX + (Random.Shared.NextDouble() * 2.0 - 1.0) * HeadXRange;
		return new Vector2((float)randomX, (float)(initialY + targetOffset));
	}

	private bool MoveHead(double delta)
	{
		if (catHead.Position.DistanceSquaredTo(currentHeadTargetPos) > 0.1f)
		{
			float t = (float)delta * headSmoothingSpeed;
			catHead.Position = catHead.Position.Lerp(currentHeadTargetPos, t);
			currentCp1 = currentCp1.Lerp(targetCp1, t);
			currentCp2 = currentCp2.Lerp(targetCp2, t);
			return true;
		}
		else if (catHead.Position != currentHeadTargetPos)
		{
			catHead.Position = currentHeadTargetPos;
			currentCp1 = targetCp1;
			currentCp2 = targetCp2;
			return true;
		}
		return false;
	}

	void TickSan(double delta, TVStatus tVStatus)
	{
		double deltaSan = 0;
		switch (tVStatus)
		{
			case TVStatus.GOOD:
				deltaSan = delta * 5;
				break;
			case TVStatus.MOSAIC:
				deltaSan = delta * -10;
				break;
			case TVStatus.Fool:
				deltaSan = 0d;
				break;
		}

		if (IsMaosked)
		{
			deltaSan *= 0;
		}

		San = Math.Max(0, San + deltaSan);
		San = Math.Min(100, San);
	}

	public void SetMasked(bool masked)
	{
		IsMaosked = masked;
	}
}
