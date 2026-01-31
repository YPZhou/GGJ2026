using Godot;

public enum TVStatus
{
	GOOD, // 正在播放好图片
	BAD, // 正在播放坏图片
}

public partial class TV : Node
{
	public static TVStatus CurrentStatus { get; set; }

	public override void _Ready()
	{
		base._Ready();

		CurrentStatus = TVStatus.BAD;
	}
}
