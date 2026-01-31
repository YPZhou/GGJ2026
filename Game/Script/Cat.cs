using System;
using Godot;

public partial class Cat : Sprite2D
{
	[Export] int San; // 小猫san值
	[Export] double HeadMoveInterval; // 猫头瞬移间隔
	[Export] double HeadYOffsetA = -20.0; // 相对初始Y的偏移量
	[Export] double HeadYOffsetB = 0.0;
	[Export] double HeadYOffsetC = 20.0;

	public bool IsAlive => San > 0;

	public TVStatus CurrentStatus => TV.CurrentStatus;

	// 猫头瞬移状态
	private double headTimer = 0.0;
	private int headIndex = 0; // 0,1,2 循环
	private double initialY = 0.0;

	public override void _Ready()
	{
		base._Ready();
		initialY = Position.Y;
		headTimer = 0.0;
		headIndex = 1;
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

	void UpdateCatWhenWatchingGoodScene(double delta)
	{
		// 恢复回初始位置并重置计时器
		headTimer = 0.0;
		headIndex = 0;
		Position = new Vector2(Position.X, (float)initialY);
	}

	void UpdateCatWhenWatchingBadScene(double delta)
	{
		// 根据 HeadMoveInterval 计时，超过间隔后在三个偏移位置中切换
		headTimer += delta;
		if (HeadMoveInterval <= 0)
		{
			// 如果间隔为0或负数，保持在第一个位置
			Position = new Vector2(Position.X, (float)(initialY + HeadYOffsetA));
			return;
		}

		if (headTimer >= HeadMoveInterval)
		{
			headTimer = 0.0;
			int newIndex = Random.Shared.Next(3);
			// 保证移动（若随机到相同索引，则改为下一个）
			if (newIndex == headIndex)
				newIndex = (headIndex + 1) % 3;
			headIndex = newIndex;
		}

		double targetOffset = headIndex == 0 ? HeadYOffsetA : (headIndex == 1 ? HeadYOffsetB : HeadYOffsetC);
		Position = new Vector2(Position.X, (float)(initialY + targetOffset));
	}
}
