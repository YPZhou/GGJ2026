using System;
using Godot;

public enum TVStatus
{
	GOOD, // 正在播放好图片
	BAD, // 正在播放坏图片
	MOSAIC, // 正在播放马赛克
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

	[Export(PropertyHint.Range, "0, 100")]
	float imageGoodWeight = 20f;
	[Export(PropertyHint.Range, "0, 100")]
	float imageBadWeight = 20f;
	[Export(PropertyHint.Range, "0, 100")]
	float imageMosaicWeight = 60f;

	[Export]
	double imageMinInterval = 0.75f;
	[Export]
	double imageMaxInterval = 1.5f;
	double imageInterval = 0;

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
		if (mosaicImage == null)
		{
			GD.PrintErr("Mosaic image is null, set mosaic image for TV.mosaicImage in the editor.");
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		imageInterval -= delta;
		if (imageInterval <= 0)
		{

			imageInterval = GD.RandRange(imageMinInterval, imageMaxInterval);
			var nextStatus = RandomizeStatus();

			GD.Print($"{nameof(TV)}: Interval:" + imageInterval + " Change image to " + nextStatus);

			SetImage(nextStatus);
		}
	}

	private TVStatus RandomizeStatus()
	{
		float totalWeight = imageGoodWeight + imageBadWeight + imageMosaicWeight;
		if (totalWeight <= 0) return TVStatus.GOOD;

		double randomValue = GD.RandRange(0, totalWeight);
		TVStatus status;

		if (randomValue < imageGoodWeight)
		{
			status = TVStatus.GOOD;
		}
		else if (randomValue < imageGoodWeight + imageBadWeight)
		{
			status = TVStatus.BAD;
		}
		else
		{
			status = TVStatus.MOSAIC;
		}
		return status;
	}

	void SetImage(TVStatus status)
	{
		CurrentStatus = status;
		switch (status)
		{
			case TVStatus.GOOD:
				Texture = goodImage;
				break;
			case TVStatus.BAD:
				Texture = badImage;
				break;
			case TVStatus.MOSAIC:
				Texture = mosaicImage;
				break;
		}
	}

}
