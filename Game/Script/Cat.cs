using System;
using Godot;

public partial class Cat : Sprite2D
{
	[Export] public double San { get; private set; } // 小猫san值
	[Export] double HeadMoveInterval; // 猫头瞬移间隔
	[Export] double[] HeadYOffsets; // 可在 Inspector 中编辑
	[Export] double HeadXRange; // X 轴随机偏移范围（±）
	[Export] Node2D catHead; // 猫头节点
	[Export] Node2D catNecksParent; // 猫脖子父节点
	[Export] Texture2D catNeckTexture; // 猫脖子图片
	[Export] Game game;

	bool IsMaosked = false;

	public bool IsAlive => San > 0;

	public TVStatus CurrentStatus => TV.CurrentStatus;

	// 猫头瞬移状态
	double headTimer;
	int headIndex; // 索引到 HeadYOffsets
	double initialY;
	double initialX;
	double currentX;
	Line2D headCurve;

	Vector2 currentHeadTargetPos;
	Vector2 currentCp1, currentCp2;
	Vector2 targetCp1, targetCp2;
	
	// 脖子曲线系统
	Curve2D neckCurve;
	bool needsNeckUpdate = true;

	[Export] float headSmoothingSpeed = 5.0f;
	[Export] float curveTolerance = 2.0f;

	[Export] Area2D interactArea;
	[Export] Gizmo gizmo;

	void CheckRes()
	{
		if (catHead == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Cat head is null, set Cat head for Cat.catHead in the editor.");
		}
		if (catNecksParent == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Cat necks parent is null, set Cat necks parent for Cat.catNecksParent in the editor.");
		}
		if (catNeckTexture == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Cat neck texture is null, set Cat neck texture for Cat.catNeckTexture in the editor.");
		}
		if (interactArea == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Interact area is null, set Interact area for Cat.interactArea in the editor.");
		}
		if (gizmo == null)
		{
			GD.PrintErr($"{nameof(Cat)}: Gizmo is null, set Gizmo for Cat.gizmo in the editor.");
		}
	}

	public override void _Ready()
	{
		base._Ready();

		currentHeadTargetPos = catHead.Position;
		
		// 初始化曲线系统
		neckCurve = new Curve2D();
		
		// 初始化控制点（身体到头部的贝塞尔曲线）
		Vector2 bodyPos = catNecksParent.Position; // 身体位置（相对于Cat节点）
		Vector2 headPos = catHead.Position;
		Vector2 diff = headPos - bodyPos;
		
		// 设置初始控制点（1/3和2/3位置，稍微偏移形成曲线）
		currentCp1 = bodyPos + diff * 0.33f + new Vector2(-20, 0);
		currentCp2 = bodyPos + diff * 0.66f + new Vector2(20, 0);
		targetCp1 = currentCp1;
		targetCp2 = currentCp2;

		San = 100;

		CheckRes();
		headTimer = 0;
		currentHeadTargetPos = GenerateRandomCatHeadPosition();

		// 初始生成脖子
		GenerateNeck();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (IsAlive && game.CurrentGameState == GameState.Playing)
		{
			switch (CurrentStatus)
			{
				case TVStatus.MOSAIC:
				case TVStatus.Fool:
				TickCatHeadPosGen(delta);
					
					break;
				default:
				case TVStatus.GOOD:
					break;
			}

			if (MoveHead(delta))
			{
				needsNeckUpdate = true;
			}
			
			// 更新脖子节段
			if (needsNeckUpdate)
			{
				GenerateNeck();
				needsNeckUpdate = false;
			}
			
			TickSan(delta, CurrentStatus);
		}
	}

	void TickCatHeadPosGen(double delta){
		headTimer += delta;
		if (headTimer >= HeadMoveInterval)
		{
			currentHeadTargetPos = GenerateRandomCatHeadPosition();
			GD.Print($"{nameof(Cat)}: New head position: {currentHeadTargetPos}");
			headTimer = 0;

            AudioManager.Instance.PlaySFX(AudioManager.EAudioSFX.CatMove);
        }
	}

	Vector2 GenerateRandomCatHeadPosition()
	{
		double targetOffset = 0.0;
		if (HeadYOffsets != null && HeadYOffsets.Length > 0)
		{
			int index = Random.Shared.Next(HeadYOffsets.Length);
			targetOffset = HeadYOffsets[index];
		}
		double randomX = initialX + (Random.Shared.NextDouble() * 2.0 - 1.5) * HeadXRange;
		Vector2 newPos = new Vector2((float)randomX, (float)(initialY + targetOffset));
		
		// 更新目标控制点
		Vector2 bodyPos = catNecksParent.Position;
		Vector2 diff = newPos - bodyPos;
		targetCp1 = bodyPos + diff * 0.33f + new Vector2(-20, 0);
		targetCp2 = bodyPos + diff * 0.66f + new Vector2(20, 0);
		
		return newPos;
	}

	private bool MoveHead(double delta)
	{
		if (catHead.Position.DistanceSquaredTo(currentHeadTargetPos) > 0.1f)
		{
			float t = (float)delta * headSmoothingSpeed;
			catHead.Position = catHead.Position.Lerp(currentHeadTargetPos, t);
			currentCp1 = currentCp1.Lerp(targetCp1, t);
			currentCp2 = currentCp2.Lerp(targetCp2, t);
            return true;
		}
		else if (catHead.Position != currentHeadTargetPos)
		{
			catHead.Position = currentHeadTargetPos;
			currentCp1 = targetCp1;
			currentCp2 = targetCp2;
			return true;
		}
		return false;
	}

	void TickSan(double delta, TVStatus tVStatus)
	{
		double deltaSan = 0;
		switch (tVStatus)
		{
			case TVStatus.GOOD:
				deltaSan = delta * 5;
				break;
			case TVStatus.MOSAIC:
				deltaSan = delta * -10;
				break;
			case TVStatus.Fool:
				deltaSan = 0d;
				break;
		}

		if (IsMaosked)
		{
			deltaSan *= 0;
		}

		San = Math.Max(0, San + deltaSan);
		San = Math.Min(100, San);
	}

	public void SetMasked(bool masked)
	{
		IsMaosked = masked;
	}
	
	/// <summary>
	/// 生成脖子节段，沿贝塞尔曲线从身体到头部
	/// </summary>
	void GenerateNeck()
	{
		if (catNecksParent == null || catNeckTexture == null || neckCurve == null)
			return;
			
		// 1. 构建贝塞尔曲线
		neckCurve.ClearPoints();
		
		var bodyPos = catNecksParent.Position; // 身体位置
		var headPos = catHead.Position; // 头部位置
		
		// 添加曲线点（起点、控制点、终点）
		neckCurve.AddPoint(bodyPos, Vector2.Zero, currentCp1 - bodyPos);
		neckCurve.AddPoint(headPos, currentCp2 - headPos, Vector2.Zero);
		
		// 2. 沿曲线绘制多边形
		var curveLength = neckCurve.GetBakedLength();
		if (curveLength > 0)
		{
			foreach (var neckPolygon in catNecksParent.GetChildren())
			{
				neckPolygon.QueueFree();
			}

			var segmentCount = 10;
			var prevPosition = Vector2.Zero;
			for (var i = 1; i <= segmentCount; i++)
			{
				// 采样位置
				var t = ((float)i) / segmentCount * curveLength;
				var position = neckCurve.SampleBaked(t) - catNecksParent.Position;
				BuildNeckPolygon(position, prevPosition);
				prevPosition = position;
			}
			
			// 5. 调试可视化
			// if (gizmo != null)
			// {
			// 	var points = new List<Vector2>();
			// 	for (int i = 0; i <= segmentCount; i++)
			// 	{
			// 		var t = i / (float)segmentCount;
			// 		points.Add(neckCurve.SampleBaked(t * curveLength));
			// 	}
			// 	gizmo.Points = [.. points];
			// }
		}
	}

	void BuildNeckPolygon(Vector2 topPosition, Vector2 bottomPosition)
	{
		// Magic number，猫身体与脖子连接处的斜边方向
		var normal = new Vector2(125, -24).Normalized();

		float halfWidth = 40f * 1.5f; // 节段半宽度（可调整）
		var p1 = topPosition - normal * halfWidth;
		var p2 = bottomPosition - normal * halfWidth;
		var p3 = bottomPosition + normal * halfWidth;
		var p4 = topPosition + normal * halfWidth;

		var neckPolygon = new Polygon2D
		{
			Texture = catNeckTexture,
			Polygon = [p1, p2, p3, p4],
			UV =
			[
				Vector2.Zero,
				new Vector2(0, 31),
				new Vector2(111, 31),
				new Vector2(111, 0),
			]
		};

		catNecksParent.AddChild(neckPolygon);
	}
}
