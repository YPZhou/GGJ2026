using System;
using System.Collections.Generic;
using Godot;

public enum TVStatus
{
	GOOD, // 正在播放好图片
	Fool, // 正在播放Fool
	MOSAIC, // 正在播放马赛克
}

public partial class TV : Sprite2D
{
	public static TVStatus CurrentStatus { get; private set; }

	[Export]
	Game game;
	[Export]
	Texture2D goodImage;
	[Export]
	Texture2D foolImage;
	[Export]
	Texture2D mosaicImage;

	[Export(PropertyHint.Range, "0, 100")]
	float imageGoodWeight = 20f;
	[Export(PropertyHint.Range, "0, 100")]
	float imageFoolWeight = 20f;
	[Export(PropertyHint.Range, "0, 100")]
	float imageMosaicWeight = 60f;

	[Export]
	double imageMinInterval = 0.75f;
	[Export]
	double imageMaxInterval = 1.5f;

	double imageInterval = 0;

	[Export]
	int maxGoodCount = 4;
	[Export]
	int maxFoolCount = 4;
	[Export]
	int maxMosaicCount = 6;

	TVStatusCounter tVStatusCounter;

	List<WeightedObject<TVStatus>> weightedStates;

	public override void _Ready()
	{
		base._Ready();

		CurrentStatus = TVStatus.Fool;
		tVStatusCounter = new TVStatusCounter();
		weightedStates = new List<WeightedObject<TVStatus>>()
		{
			new WeightedObject<TVStatus>(TVStatus.GOOD, imageGoodWeight),
			new WeightedObject<TVStatus>(TVStatus.Fool, imageFoolWeight),
			new WeightedObject<TVStatus>(TVStatus.MOSAIC, imageMosaicWeight),
		};


		CheckRes();

		SetImage(CurrentStatus);
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
		if (foolImage == null)
		{
			GD.PrintErr("Fool image is null, set fool image for TV.foolImage in the editor.");
		}
		if (mosaicImage == null)
		{
			GD.PrintErr("Mosaic image is null, set mosaic image for TV.mosaicImage in the editor.");
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (game != null && !game.IsGameEnd)
		{
			GameProcess(delta);
		}
		else
		{
			StandbyProcess(delta);
		}
	}

	void GameProcess(double delta)
	{
		imageInterval -= delta;
		if (imageInterval <= 0)
		{

			imageInterval = GD.RandRange(imageMinInterval, imageMaxInterval);
			var nextStatus = RandomizeStatus();

			GD.Print($"{nameof(TV)}: Interval:" + imageInterval.ToString("F2") + " Change image to " + nextStatus);

			SetImage(nextStatus);
		}
	}

	void StandbyProcess(double delta)
	{
		SetImage(TVStatus.Fool);
	}

	private TVStatus RandomizeStatus()
	{
		TVStatus status = TVStatus.GOOD;

		var candidataes = new List<WeightedObject<TVStatus>>();
		if (!tVStatusCounter.IsLimitReached(TVStatus.GOOD, maxGoodCount))
		{
			candidataes.Add(new WeightedObject<TVStatus>(TVStatus.GOOD, imageGoodWeight));
		}

		if (!tVStatusCounter.IsLimitReached(TVStatus.Fool, maxFoolCount))
		{
			candidataes.Add(new WeightedObject<TVStatus>(TVStatus.Fool, imageFoolWeight));
		}

		if (!tVStatusCounter.IsLimitReached(TVStatus.MOSAIC, maxMosaicCount))
		{
			candidataes.Add(new WeightedObject<TVStatus>(TVStatus.MOSAIC, imageMosaicWeight));
		}

		float totalWeight = 0f;
		foreach (var obj in candidataes)
		{
			totalWeight += obj.Weight;
		}

		var random = GD.RandRange(0f, totalWeight);
		float cumulativeWeight = 0f;
		foreach (var obj in candidataes)
		{
			cumulativeWeight += obj.Weight;
			if (random < cumulativeWeight)
			{
				status = obj.Status;
				break;
			}
		}



		if (status != CurrentStatus)
		{
			tVStatusCounter.Clear();
		}
		tVStatusCounter.Counts[status]++;

		return status;
	}

	void SetImage(TVStatus status)
	{
		CurrentStatus = status;
		var mat = Material as ShaderMaterial;
		switch (status)
		{
			case TVStatus.GOOD:
				if (mat != null)
				{
					mat.SetShaderParameter("scanline_count", 0f);
					mat.SetShaderParameter("static_intensity", 0f);
					mat.SetShaderParameter("scanline_opacity", 0f);
					mat.SetShaderParameter("boost", 1.0f); // 正常亮度
					mat.SetShaderParameter("roll_speed", 0f);
				}
				Texture = goodImage;
				break;
			case TVStatus.Fool:
				if (mat != null)
				{
					mat.SetShaderParameter("scanline_count", 6f);
					mat.SetShaderParameter("static_intensity", 0.2f);
					mat.SetShaderParameter("scanline_opacity", 0.1f);
					mat.SetShaderParameter("boost", 1.2f);
					mat.SetShaderParameter("roll_speed", 0.2f);
				}
				Texture = foolImage;
				break;
			case TVStatus.MOSAIC:
				if (mat != null)
				{
					mat.SetShaderParameter("scanline_count", 6f);
					mat.SetShaderParameter("static_intensity", 0.2f);
					mat.SetShaderParameter("scanline_opacity", 0.4f);
					mat.SetShaderParameter("boost", 1.4f);
					mat.SetShaderParameter("roll_speed", 0.4f);
				}
				Texture = mosaicImage;
				break;
		}
	}

}

class WeightedObject<T> where T : struct
{
	public T Status { get; private set; }
	public float Weight { get; private set; }

	public WeightedObject(T status, float weight)
	{
		Status = status;
		Weight = weight;
	}
}

public class TVStatusCounter
{
	Dictionary<TVStatus, int> counts;

	public IDictionary<TVStatus, int> Counts => counts;

	public TVStatusCounter()
	{
		counts = new Dictionary<TVStatus, int>()
		{
			{ TVStatus.GOOD, 0 },
			{ TVStatus.Fool, 0 },
			{ TVStatus.MOSAIC, 0 },
		};
	}

	public void Clear()
	{
		foreach (var key in counts.Keys)
		{
			counts[key] = 0;
		}
	}

	public bool IsLimitReached(TVStatus status, int maxCount)
	{
		return counts[status] >= maxCount;
	}

}