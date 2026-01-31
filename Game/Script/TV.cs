using Godot;

public enum TVStatus
{
	GOOD, // 正在播放好图片
	BAD, // 正在播放坏图片
}

public partial class TV : Sprite2D
{
	public static TVStatus CurrentStatus { get; set; }

	[Export]
	Game game;
	[Export]
	Texture2D goodImage;
	[Export]
	Texture2D badImage;
	[Export]
	Texture2D mosaicImage;

	public override void _Ready()
	{
		base._Ready();

		CurrentStatus = TVStatus.BAD;

		CheckRes();

		SetImage(TVStatus.GOOD);
	}

	void CheckRes()
	{
		if (game == null)
		{
			GD.PrintErr("Game is null, set Game for TV.game in the editor.");
		}
		if (goodImage == null)
		{
			GD.PrintErr("Good image is null, set good image for TV.goodImage in the editor.");
		}
		if (badImage == null)
		{
			GD.PrintErr("Bad image is null, set bad image for TV.badImage in the editor.");
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		
	}

	void SetImage(TVStatus status)
	{
		if (status == CurrentStatus)
		{
			return;
		}

		CurrentStatus = status;
		switch (status)
		{
			case TVStatus.GOOD:
				Texture = goodImage;
				break;
			case TVStatus.BAD:
				Texture = badImage;
				break;
		}
	}

}
