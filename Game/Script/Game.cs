using Godot;

public partial class Game : Node
{
	[Export] ResultControl resultControl;

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

		resultControl.OkButton.Pressed += () =>
		{
			GD.Print($"{nameof(Game)}:重新开始");
			GetTree().ReloadCurrentScene();
		};
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!IsGameEnd)
		{
			elapsedTime += (float)delta;
			timeBar.Value = elapsedTime;
			UpdateTimeHint(RemainingTIme);
		}
		else
		{
			if (resultControl != null)
			{
				resultControl.Visible = true;
			}
			else
			{
				GD.PrintErr("Result control not set in Game.resultControl");
			}
		}
	}

	void UpdateTimeHint(double remainingTIme)
	{
		timeHint.Text = $"时间 {remainingTIme:F1}s";
	}
}
