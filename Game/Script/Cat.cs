using System;
using System.Collections.Generic;
using Godot;

public partial class Cat : Sprite2D
{
	[Export] public double San { get; private set; } // 小猫san值
	[Export] double HeadMoveInterval; // 猫头瞬移间隔
	[Export] double[] HeadYOffsets; // 可在 Inspector 中编辑
	[Export] double HeadXRange; // X 轴随机偏移范围（±）
	[Export] Node2D catHead; // 猫头节点
	[Export] Node catNecksParent; // 猫脖子父节点
	[Export] Texture2D catNeckTexture; // 猫脖子图片
	[Export] Game game;

	[Export] Node2D NeckStart;
	[Export] Node2D NeckEnd;

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
	Vector2 neckEndOffset;

	[Export] float headSmoothingSpeed = 5.0f;
	[Export] int neckSegments = 60; // Now used as a fallback or limit
	[Export] float curveTolerance = 2.0f;

	[Export]
    Area2D interactArea;

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
		if ( NeckStart == null || NeckEnd == null)
		{
		 	GD.PrintErr($"{nameof(Cat)}: Neck start or end is null, set NeckStart and NeckEnd for Cat in the editor.");
		}
	}

	public override void _Ready()
	{
		base._Ready();

		// Calculate offset from Head to NeckEnd
		if (catHead != null && NeckEnd != null)
			neckEndOffset = NeckEnd.GlobalPosition - catHead.GlobalPosition;
		else
			neckEndOffset = Vector2.Zero;

		currentHeadTargetPos = catHead.Position;
		
		// Initial straight CPs
		Vector2 from = (NeckStart != null) ? ToLocal(NeckStart.GlobalPosition) : Vector2.Zero;
		Vector2 to = (NeckEnd != null) ? ToLocal(NeckEnd.GlobalPosition) : catHead.Position;
		float dx = to.X - from.X;
		float dy = to.Y - from.Y;
		targetCp1 = from + new Vector2(dx * 0.25f, dy * 0.5f);
		targetCp2 = from + new Vector2(dx * 0.75f, dy * 0.8f);
		currentCp1 = targetCp1;
		currentCp2 = targetCp2;

		initialY = catHead.Position.Y;
		initialX = catHead.Position.X;
		currentX = initialX;
		headTimer = 0.0;
		headIndex = 0;
		// 初始化或创建用于绘制贝塞尔曲线的 Line2D 节点
		headCurve = GetNodeOrNull<Line2D>("HeadCurve");
		if (headCurve == null)
		{
			headCurve = new Line2D();
			headCurve.Name = "HeadCurve";
			AddChild(headCurve);
		}

		San = 100;
		
		// Ensure neck is drawn at start
		DrawCurrentNeck();

		CheckRes();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (IsAlive && game.CurrentGameState == GameState.Playing)
		{
			if (CurrentStatus == TVStatus.GOOD)
			{
				UpdateCatWhenWatchingGoodScene(delta);
			}
			else if (CurrentStatus == TVStatus.Fool)
			{
				UpdateCatWhenWatchingBadScene(delta);
			}

			if (MoveHead(delta))
			{
				DrawCurrentNeck();
			}
			
			TickSan(delta, CurrentStatus);
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
		double randomX = initialX + (Random.Shared.NextDouble() * 2.0 - 1.0) * HeadXRange;
		return new Vector2((float)randomX, (float)(initialY + targetOffset));
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

	private void DrawCurrentNeck()
	{
		Vector2 start = (NeckStart != null) ? ToLocal(NeckStart.GlobalPosition) : Vector2.Zero;
		Vector2 end = (NeckEnd != null) ? ToLocal(NeckEnd.GlobalPosition) : catHead.Position;
		DrawNeck(start, end);
	}

	void UpdateCatWhenWatchingGoodScene(double _)
	{
		// 恢复回初始位置并重置计时器
		headTimer = 0.0;
		headIndex = 0;
		currentX = initialX;
		currentHeadTargetPos = new Vector2((float)currentX, (float)initialY);
		
		// Reset CPs (straight line approximate)
		Vector2 from = (NeckStart != null) ? ToLocal(NeckStart.GlobalPosition) : Vector2.Zero;
		Vector2 to = currentHeadTargetPos + neckEndOffset; // Approximated target
		float dx = to.X - from.X;
		float dy = to.Y - from.Y;
		targetCp1 = from + new Vector2(dx * 0.25f, dy * 0.5f);
		targetCp2 = from + new Vector2(dx * 0.75f, dy * 0.8f);

		// 清除贝塞尔曲线（例如隐藏或清空点）
		if (headCurve != null)
			headCurve.Points = new Vector2[0];
	}

	void UpdateCatWhenWatchingBadScene(double delta)
	{
		// 根据 HeadMoveInterval 计时，超过间隔后在五个偏移位置中切换，X 轴在范围内随机变化
		headTimer += delta;
		if (headTimer >= HeadMoveInterval)
		{
			var booster = Booster.GetBooster(game.ElapsedTime);
			headTimer = 0.0;
			if (HeadYOffsets != null && HeadYOffsets.Length > 0)
			{
				int newIndex = Random.Shared.Next(HeadYOffsets.Length);
				// 保证移动（若随机到相同索引，则改为下一个）
				if (newIndex == headIndex)
				{
					newIndex = (headIndex + 1) % HeadYOffsets.Length;
				}
				headIndex = newIndex;
				// 在 Y 改变时更新 X
				currentX = initialX + (Random.Shared.NextDouble() * 2.0 - 1.0) * HeadXRange * booster;
			}

			double targetOffset = (HeadYOffsets != null && HeadYOffsets.Length > 0) 
			? HeadYOffsets[Math.Clamp(headIndex, 0, HeadYOffsets.Length - 1)] * booster
			: 0.0;
			
			currentHeadTargetPos = new Vector2((float)currentX, (float)(initialY + targetOffset));
			
			Vector2 from = (NeckStart != null) ? ToLocal(NeckStart.GlobalPosition) : Vector2.Zero;
			Vector2 to = currentHeadTargetPos + neckEndOffset;
			CalculateTargetBezierControlPoints(from, to);
		}
	}

	// 生成并绘制一条从 body 到 head 的贝塞尔曲线（在本节点坐标系内）
	private void DrawNeck(Vector2 from, Vector2 to)
	{
		if (headCurve == null)
			return;
		var points = GeneratePointsForCPs(from, to, currentCp1, currentCp2, neckSegments);
		PlaceCatNeckAlongPoints(points, catNeckTexture);
		catHead.GetParent().MoveChild(catHead, -1);
	}

	[Obsolete]
	private void CalculateTargetBezierControlPoints(Vector2 from, Vector2 to)
	{
		// 尝试随机生成 0 或 1 个交叉的贝塞尔曲线点
		int desiredCross = Random.Shared.Next(2);
		int attempts = 8;
		int bestDiff = int.MaxValue;

		// Default fallback
		float dx = to.X - from.X;
		float dy = to.Y - from.Y;
		targetCp1 = from + new Vector2(dx * 0.25f, dy * 0.5f);
		targetCp2 = from + new Vector2(dx * 0.75f, dy * 0.8f);

		for (int attempt = 0; attempt < attempts; attempt++)
		{
			dx = to.X - from.X;
			dy = to.Y - from.Y;
			float dist = (to - from).Length();

			Vector2 cp1 = from + new Vector2(dx * 0.25f + (float)(Random.Shared.NextDouble() - 0.5) * dist * 0.4f, from.Y + dy * 0.5f);
			Vector2 cp2 = from + new Vector2(dx * 0.75f + (float)(Random.Shared.NextDouble() - 0.5) * dist * 0.4f, from.Y + dy * 0.8f);

			Vector2[] pts = GeneratePointsForCPs(from, to, cp1, cp2, neckSegments);
			
			int crosses = CountSelfIntersections(pts);
			int diff = Math.Abs(crosses - desiredCross);
			if (diff < bestDiff)
			{
				bestDiff = diff;
				targetCp1 = cp1;
				targetCp2 = cp2;
			}
			if (crosses == desiredCross)
			{
				break;
			}
		}
	}

	[Obsolete]
	private Vector2[] GeneratePointsForCPs(Vector2 from, Vector2 to, Vector2 cp1, Vector2 cp2, int segments)
	{
		List<Vector2> points = new List<Vector2>();
		points.Add(from);
		RecursiveBezier(points, from, cp1, cp2, to, curveTolerance * curveTolerance, 0);
		return points.ToArray();
	}

	[Obsolete]
	private void RecursiveBezier(List<Vector2> points, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float tolSq, int level)
	{
		// 递归深度限制 (2^10 = 1024 segments)
		if (level > 10)
		{
			points.Add(p3);
			return;
		}

		// 计算平直度 (Flatness)
		float ux = p3.X - p0.X;
		float uy = p3.Y - p0.Y;
		float lenSq = ux * ux + uy * uy;

		bool isFlat = true;

		if (lenSq < 1e-5f)
		{
			// 起点终点几乎重合，检查控制点距离
			if (p0.DistanceSquaredTo(p1) > tolSq || p0.DistanceSquaredTo(p2) > tolSq)
				isFlat = false;
		}
		else
		{
			// 直线方程距离公式 (叉乘) / 长度
			float cross1 = (p1.X - p0.X) * uy - (p1.Y - p0.Y) * ux; 
			float cross2 = (p2.X - p0.X) * uy - (p2.Y - p0.Y) * ux;

			if ((cross1 * cross1) > tolSq * lenSq || (cross2 * cross2) > tolSq * lenSq)
				isFlat = false;
		}

		if (isFlat)
		{
			points.Add(p3);
		}
		else
		{
			// De Casteljau subdivision
			Vector2 p01 = (p0 + p1) * 0.5f;
			Vector2 p12 = (p1 + p2) * 0.5f;
			Vector2 p23 = (p2 + p3) * 0.5f;
			Vector2 p012 = (p01 + p12) * 0.5f;
			Vector2 p123 = (p12 + p23) * 0.5f;
			Vector2 p0123 = (p012 + p123) * 0.5f;

			RecursiveBezier(points, p0, p01, p012, p0123, tolSq, level + 1);
			RecursiveBezier(points, p0123, p123, p23, p3, tolSq, level + 1);
		}
	}

	[Obsolete]
	void PlaceCatNeckAlongPoints(Vector2[] bezierPoints, Texture2D catNeckTexture, float scale = 1.0f)
	{
		if (bezierPoints == null || bezierPoints.Length < 2 || catNeckTexture == null)
			return;

		foreach (var child in catNecksParent.GetChildren())
		{
			child.QueueFree();
		}

		float totalLength = 0f;
		for (int i = 0; i < bezierPoints.Length - 1; i++)
		{
			totalLength += bezierPoints[i].DistanceTo(bezierPoints[i + 1]);
		}

		float currentDist = 0f;
		float textureWidth = catNeckTexture.GetWidth();
		float textureHeight = catNeckTexture.GetHeight();

		for (int i = 0; i < bezierPoints.Length - 1; i++)
		{
			Vector2 pointA = bezierPoints[i];
			Vector2 pointB = bezierPoints[i + 1];
			float segLen = pointA.DistanceTo(pointB);

			// 计算线段的中点作为图片位置
			Vector2 position = (pointA + pointB) * 0.5f;

			// 计算线段方向作为图片旋转
			Vector2 direction = (pointB - pointA).Normalized();
			float rotation = direction.Angle();

			float uStart = (currentDist / totalLength) * textureWidth;
			float uEnd = ((currentDist + segLen) / totalLength) * textureWidth;

			currentDist += segLen;

			CreateSegmentWithCustomUV(position, rotation, catNeckTexture, scale, segLen, uStart, uEnd, textureHeight);
		}
	}

	[Obsolete]
	void CreateSegmentWithCustomUV(Vector2 position, float rotation, Texture2D texture, float thicknessScale, float length, float uStart, float uEnd, float texHeight)
	{
		float halfLength = length * 0.5f;
		float halfThickness = texture.GetHeight() * thicknessScale * 0.5f;

		Polygon2D segment = new Polygon2D();
		segment.Texture = texture;

		float cosR = Mathf.Cos(rotation);
		float sinR = Mathf.Sin(rotation);

		// Local: X is along length, Y is along thickness
		Vector2[] vertices = new Vector2[4];

		// TL (Top Left in texture space -> uStart, 0) -> Local (-halfLength, -halfThickness)
		vertices[0] = new Vector2(
			position.X + (-halfLength) * cosR - (-halfThickness) * sinR,
			position.Y + (-halfLength) * sinR + (-halfThickness) * cosR
		);

		// BL (Bottom Left in texture space -> uStart, texHeight) -> Local (-halfLength, +halfThickness)
		vertices[1] = new Vector2(
			position.X + (-halfLength) * cosR - (halfThickness) * sinR,
			position.Y + (-halfLength) * sinR + (halfThickness) * cosR
		);

		// BR (Bottom Right in texture space -> uEnd, texHeight) -> Local (+halfLength, +halfThickness)
		vertices[2] = new Vector2(
			position.X + (halfLength) * cosR - (halfThickness) * sinR,
			position.Y + (halfLength) * sinR + (halfThickness) * cosR
		);

		// TR (Top Right in texture space -> uEnd, 0) -> Local (+halfLength, -halfThickness)
		vertices[3] = new Vector2(
			position.X + (halfLength) * cosR - (-halfThickness) * sinR,
			position.Y + (halfLength) * sinR + (-halfThickness) * cosR
		);

		segment.Polygon = vertices;

		segment.UV = new Vector2[]
		{
			new Vector2(uStart, 0),
			new Vector2(uStart, texHeight),
			new Vector2(uEnd, texHeight),
			new Vector2(uEnd, 0)
		};

		catNecksParent.AddChild(segment);
	}

	[Obsolete]
	private int CountSelfIntersections(Vector2[] pts)
	{
		int n = pts.Length;
		int count = 0;
		for (int i = 0; i < n - 1; i++)
		{
			for (int j = i + 2; j < n - 1; j++)
			{
				if (i == 0 && j == n - 2) continue;
				if (SegmentsIntersect(pts[i], pts[i + 1], pts[j], pts[j + 1])) count++;
			}
		}
		return count;
	}

	private bool SegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
	{
		float eps = 1e-6f;
		float o1 = Cross(a2 - a1, b1 - a1);
		float o2 = Cross(a2 - a1, b2 - a1);
		float o3 = Cross(b2 - b1, a1 - b1);
		float o4 = Cross(b2 - b1, a2 - b1);

		if (Math.Abs(o1) < eps && OnSegment(a1, a2, b1)) return true;
		if (Math.Abs(o2) < eps && OnSegment(a1, a2, b2)) return true;
		if (Math.Abs(o3) < eps && OnSegment(b1, b2, a1)) return true;
		if (Math.Abs(o4) < eps && OnSegment(b1, b2, a2)) return true;

		return (o1 * o2 < 0f) && (o3 * o4 < 0f);
	}

	private float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

	private bool OnSegment(Vector2 a, Vector2 b, Vector2 p)
	{
		return Math.Min(a.X, b.X) - 1e-6f <= p.X && p.X <= Math.Max(a.X, b.X) + 1e-6f &&
			Math.Min(a.Y, b.Y) - 1e-6f <= p.Y && p.Y <= Math.Max(a.Y, b.Y) + 1e-6f;
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
}
