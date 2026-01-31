using Godot;

public enum GameState
{
	Start,
	Playing,
	Result,
}

public partial class Game : Node
{
	[Export] StartControl startControl;
	[Export] ResultControl resultControl;

	[Export] Cat cat;
	[Export] Hand hand;
	[Export] TV tv;

	[Export] ProgressBar timeBar;
	[Export] Label timeHint;
	[Export] CatSanUI catSanUI;

	[Export] double totalTime; // 单局时长，30秒
	double elapsedTime = 0;
	double RemainingTIme => totalTime - elapsedTime;

	public bool IsGameEnd => elapsedTime >= totalTime || !cat.IsAlive;

	public GameState CurrentGameState { get; private set; } = GameState.Start;

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

		if (startControl == null)
		{
			GD.PrintErr($"{nameof(Game)}: StartControl is null, set StartControl for Game.startControl in the editor.");
		}
		if (resultControl == null)
		{
			GD.PrintErr($"{nameof(Game)}: ResultControl is null, set ResultControl for Game.resultControl in the editor.");
		}
	}

	public override void _Ready()
	{
		base._Ready();
		elapsedTime = 0;
		timeBar.MaxValue = totalTime;
		UpdateTimeHint(RemainingTIme);
		handTransform = hand.Transform;

		resultControl.OkButton.Pressed += () =>
		{
			GD.Print($"{nameof(Game)}:重新开始");
			GetTree().ReloadCurrentScene();
		};
		resultControl.Visible = false;

		EnterState(GameState.Start);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		switch (CurrentGameState)
		{
			case GameState.Start:
				TickStart();
				break;
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
			case GameState.Start:
				HideUI();
				startControl.Visible = true;
				break;
			case GameState.Result:
				HideUI();
				resultControl.ShowResultUI(cat.IsAlive);
				break;
			case GameState.Playing:
				HideUI();
				timeBar.Visible = true;
				catSanUI.Visible = true;

				hand.Transform = handTransform;
				break;
		}
	}

	void TickGame(double delta)
	{
		if (!IsGameEnd)
		{
			elapsedTime += (float)delta;
			timeBar.Value = elapsedTime;
			UpdateTimeHint(RemainingTIme);
			UpdateCatSanUI();
		}
		else
		{
			EnterState(GameState.Result);
		}
	}

	void TickStart()
	{
		if (hand.IsTouchingTv && Input.IsKeyPressed(Key.Space))
		{
			GD.Print($"{nameof(Game)}:开始游戏");
			EnterState(GameState.Playing);
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
	}

	void UpdateCatSanUI()
	{
		if (catSanUI != null && cat != null)
		{
			catSanUI.UpdateProgress(cat.San);
		}
	}
}
