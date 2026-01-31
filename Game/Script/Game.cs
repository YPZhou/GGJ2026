using Godot;

public enum GameState
{
	Playing,
	Result,
}

public partial class Game : Node
{
	[Export] ResultControl resultControl;

	[Export] Cat cat;
	[Export] Hand hand;
	[Export] TV tv;

	[Export] ProgressBar timeBar;
	[Export] Label timeHint;
	[Export] CatSanUI catSanUI;

	[Export] double totalTime; // 单局时长，30秒
	public double ElapsedTime  { get; private set; } = 0;
	double RemainingTIme => totalTime - ElapsedTime;

	public bool IsGameEnd => ElapsedTime >= totalTime || !cat.IsAlive;

	public GameState CurrentGameState { get; private set; } = GameState.Playing;

	Transform2D handTransform;

	void CheckRes()
	{
		if (cat == null)
		{
			GD.PrintErr($"{nameof(Game)}: Cat is null, set Cat for Game.cat in the editor.");
		}
		if (hand == null)
		{
			GD.PrintErr($"{nameof(Game)}: Hand is null, set Hand for Game.hand in the editor.");
		}
		if (tv == null)
		{
			GD.PrintErr($"{nameof(Game)}: TV is null, set TV for Game.tv in the editor.");
		}
		if (resultControl == null)
		{
			GD.PrintErr($"{nameof(Game)}: ResultControl is null, set ResultControl for Game.resultControl in the editor.");
		}
	}

	public override void _Ready()
	{
		base._Ready();
		ElapsedTime = 0;
		timeBar.MaxValue = totalTime;
		UpdateTimeHint(RemainingTIme);
		handTransform = hand.Transform;
		resultControl.Visible = false;

		EnterState(GameState.Playing);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		switch (CurrentGameState)
		{
			case GameState.Result:
				break;
			case GameState.Playing:
				TickGame(delta);
				break;
		}
	}

	void EnterState(GameState state)
	{
		CurrentGameState = state;
		switch (state)
		{
			case GameState.Result:
				HideUI();
				resultControl.ShowResultUI(cat.IsAlive);
				break;
			case GameState.Playing:
				HideUI();
				timeBar.Visible = true;
				catSanUI.Visible = true;
				hand.Visible = true;
				hand.Transform = handTransform;
				break;
		}
	}

	void TickGame(double delta)
	{
		if (!IsGameEnd)
		{
			ElapsedTime += delta;
			timeBar.Value = ElapsedTime;
			UpdateTimeHint(RemainingTIme);
			UpdateCatSanUI();
		}
		else
		{
			EnterState(GameState.Result);
		}
	}

	void UpdateTimeHint(double remainingTIme)
	{
		timeHint.Text = $"时间 {remainingTIme:F1}s";
	}

	void HideUI()
	{
		timeBar.Visible = false;
		catSanUI.Visible = false;
		resultControl.Visible = false;
		hand.Visible = false;
	}

	void UpdateCatSanUI()
	{
		if (catSanUI != null && cat != null)
		{
			catSanUI.UpdateProgress(cat.San);
		}
	}
}
