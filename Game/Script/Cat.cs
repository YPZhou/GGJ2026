using Godot;

public partial class Cat : Node
{
	[Export] int San; // 小猫san值

	public bool IsAlive => San > 0;

	public TVStatus CurrentStatus => TV.CurrentStatus;

	public override void _Ready()
	{
		base._Ready();
	}


	public override void _Process(double delta)
	{
		base._Process(delta);

		if (IsAlive)
		{
			if (CurrentStatus == TVStatus.GOOD)
			{
				UpdateCatWhenWatchingGoodScene();
			}
			else if (CurrentStatus == TVStatus.BAD)
			{
				UpdateCatWhenWatchingBadScene();
			}
		}
	}

	void UpdateCatWhenWatchingGoodScene()
	{
		
	}

	void UpdateCatWhenWatchingBadScene()
	{
		
	}
}
