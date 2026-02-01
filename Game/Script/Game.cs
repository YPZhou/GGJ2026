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

	[Export] TextureProgressBar timeBar;
	[Export] Label timeHint;
	[Export] CatSanUI catSanUI;

	[Export] Sprite2D tvFace;

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
		Input.SetCustomMouseCursor(null);

		SetupFadeInTween(tvFace);
		EnterState(GameState.Playing);
	}

	void SetupFadeInTween(Node2D node)
	{
		// 淡入并滑入效果：先设置为透明并将节点从右侧偏移，然后同时 Tween 透明度和 X 位置到目标值
		if (node != null)
		{
			var c = node.Modulate;
			c.A = 0f;
			node.Modulate = c;
			node.Visible = true;

			var tween = CreateTween();
			// 透明度淡入
			tween.TweenProperty(node, "modulate:a", 1.0f, 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);

			// 保存目标位置，设置起始位置为向右偏移一定像素（例如 150px）以实现向左移入效果
			var targetPos = node.Position;
			float offsetX = 150f;
			node.Position = new Vector2(targetPos.X + offsetX, targetPos.Y);
			
			// 同步 X 轴移动回目标位置（向左移入）
			tween = CreateTween();
			tween.TweenProperty(node, "position:x", targetPos.X, 0.5f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);
		}
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

				AudioManager.Instance.PauseBGM();
                break;
			case GameState.Playing:
				HideUI();
				timeBar.Visible = true;
				catSanUI.Visible = true;
				hand.Visible = true;
				hand.Transform = handTransform;

				AudioManager.Instance.PlayBGM();
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
