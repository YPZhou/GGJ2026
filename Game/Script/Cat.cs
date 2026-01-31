using System;
using Godot;

public partial class Cat : Sprite2D
{
	[Export] int San; // 小猫san值
	[Export] double HeadMoveInterval; // 猫头瞬移间隔
	[Export] double[] HeadYOffsets; // 可在 Inspector 中编辑
	[Export] double HeadXRange; // X 轴随机偏移范围（±）
	[Export] Sprite2D catHead; // 猫头节点

	public bool IsAlive => San > 0;

	public TVStatus CurrentStatus => TV.CurrentStatus;

	// 猫头瞬移状态
	double headTimer;
	int headIndex; // 索引到 HeadYOffsets
	double initialY;
	double initialX;
	double currentX;

	public override void _Ready()
	{
		base._Ready();
		initialY = catHead.Position.Y;
		initialX = catHead.Position.X;
		currentX = initialX;
		headTimer = 0.0;
		headIndex = 0;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (IsAlive)
		{
			if (CurrentStatus == TVStatus.GOOD)
			{
				UpdateCatWhenWatchingGoodScene(delta);
			}
			else if (CurrentStatus == TVStatus.BAD)
			{
				UpdateCatWhenWatchingBadScene(delta);
			}
		}
	}

	void UpdateCatWhenWatchingGoodScene(double _)
	{
		// 恢复回初始位置并重置计时器
		headTimer = 0.0;
		headIndex = 0;
		currentX = initialX;
		catHead.Position = new Vector2((float)currentX, (float)initialY);
	}

	void UpdateCatWhenWatchingBadScene(double delta)
	{
		// 根据 HeadMoveInterval 计时，超过间隔后在五个偏移位置中切换，X 轴在范围内随机变化
		headTimer += delta;
		if (headTimer >= HeadMoveInterval)
		{
			headTimer = 0.0;
			if (HeadYOffsets != null && HeadYOffsets.Length > 0)
			{
				int newIndex = Random.Shared.Next(HeadYOffsets.Length);
				// 保证移动（若随机到相同索引，则改为下一个）
				if (newIndex == headIndex)
				{
					newIndex = (headIndex + 1) % HeadYOffsets.Length;
				}
				headIndex = newIndex;
				// 在 Y 改变时更新 X
				currentX = initialX + (Random.Shared.NextDouble() * 2.0 - 1.0) * HeadXRange;
			}

			double targetOffset = (HeadYOffsets != null && HeadYOffsets.Length > 0) ? HeadYOffsets[Math.Clamp(headIndex, 0, HeadYOffsets.Length - 1)] : 0.0;
			catHead.Position = new Vector2((float)currentX, (float)(initialY + targetOffset));
		}
	}
}
