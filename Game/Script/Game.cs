using Godot;

public partial class Game : Node
{
	[Export] Cat cat;
	[Export] Hand hand;
	[Export] TV tv;

	[Export] int totalTime; // 单局时长，30秒

	public override void _Ready()
	{
		base._Ready();
	}
}
