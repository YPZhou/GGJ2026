using Godot;

public partial class Game : Node
{
	[Export] PackedScene resultScene;

	[Export] Cat cat;
	[Export] Hand hand;
	[Export] TV tv;

	[Export] int totalTime; // 单局时长，30秒
	double elapsedTime = 0;

	bool IsGameEnd => elapsedTime >= totalTime;

	public override void _Ready()
	{
		base._Ready();
		elapsedTime = 0;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!IsGameEnd)
		{
			elapsedTime += delta;
			if (IsGameEnd)
			{
				if (resultScene != null)
				{
					GetTree().ChangeSceneToPacked(resultScene);
				}
				else
				{
					GD.PrintErr("Result scene not set in Game.resultScene");
				}
			}
		}
	}
}
