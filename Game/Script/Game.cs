using Godot;

public partial class Game : Node
{
	[Export] PackedScene resultScene;

	[Export] Cat cat;
	[Export] Hand hand;
	[Export] TV tv;

	[Export] ProgressBar timeBar;
	[Export] Label timeHint;

	[Export] double totalTime; // 单局时长，30秒
	double elapsedTime = 0;
	double RemainingTIme => totalTime - elapsedTime;

	bool IsGameEnd => elapsedTime >= totalTime;

	public override void _Ready()
	{
		base._Ready();
		elapsedTime = 0;
		timeBar.MaxValue = totalTime;
		UpdateTimeHint(RemainingTIme);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!IsGameEnd)
		{
			elapsedTime += (float)delta;
			timeBar.Value = elapsedTime;
			UpdateTimeHint(RemainingTIme);

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

	void UpdateTimeHint(double remainingTIme)
	{
		timeHint.Text = $"时间 {remainingTIme:F1}s";
	}
}
