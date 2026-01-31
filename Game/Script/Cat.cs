using System;
using Godot;

public partial class Cat : Sprite2D
{
	[Export] int San; // 小猫san值
	[Export] double HeadMoveInterval; // 猫头瞬移间隔
	[Export] double[] HeadYOffsets; // 可在 Inspector 中编辑
	[Export] double HeadXRange; // X 轴随机偏移范围（±）
	[Export] Sprite2D catHead; // 猫头节点

	public bool IsAlive => San > 0;

	public TVStatus CurrentStatus => TV.CurrentStatus;

	// 猫头瞬移状态
	double headTimer;
	int headIndex; // 索引到 HeadYOffsets
	double initialY;
	double initialX;
	double currentX;
	Line2D headCurve;

	public override void _Ready()
	{
		base._Ready();
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
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (IsAlive)
		{
			if (CurrentStatus == TVStatus.GOOD)
			{
				UpdateCatWhenWatchingGoodScene(delta);
			}
			else if (CurrentStatus == TVStatus.BAD)
			{
				UpdateCatWhenWatchingBadScene(delta);
			}
		}
	}

	void UpdateCatWhenWatchingGoodScene(double _)
	{
		// 恢复回初始位置并重置计时器
		headTimer = 0.0;
		headIndex = 0;
		currentX = initialX;
		catHead.Position = new Vector2((float)currentX, (float)initialY);
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
				currentX = initialX + (Random.Shared.NextDouble() * 2.0 - 1.0) * HeadXRange;
			}

			double targetOffset = (HeadYOffsets != null && HeadYOffsets.Length > 0) ? HeadYOffsets[Math.Clamp(headIndex, 0, HeadYOffsets.Length - 1)] : 0.0;
			catHead.Position = new Vector2((float)currentX, (float)(initialY + targetOffset));
			// 生成并绘制贝塞尔曲线（从身体到猫头）
			GenerateAndDrawBezier(new Vector2(0, 0), catHead.Position);
		}
	}

	// 生成并绘制一条从 body 到 head 的贝塞尔曲线（在本节点坐标系内）
	private void GenerateAndDrawBezier(Vector2 from, Vector2 to)
	{
		if (headCurve == null)
			return;

		headCurve.Points = GenerateBezierPointsWithOptionalCrossings(from, to, 24);
	}

	private Vector2[] GenerateBezierPointsWithOptionalCrossings(Vector2 from, Vector2 to, int segments)
	{
		// 尝试随机生成 0 或 1 个交叉的贝塞尔曲线点
		int desiredCross = Random.Shared.Next(2);
		int attempts = 8;
		Vector2[] best = null;
		int bestDiff = int.MaxValue;

		for (int attempt = 0; attempt < attempts; attempt++)
		{
			float dx = to.X - from.X;
			float dy = to.Y - from.Y;
			float dist = (to - from).Length();
			float v1 = (float)(0.2 + Random.Shared.NextDouble() * 0.8);
			float v2 = (float)(0.2 + Random.Shared.NextDouble() * 0.8);
			float dir = Random.Shared.NextDouble() > 0.5 ? 1f : -1f;

			Vector2 cp1 = from + new Vector2(dx * 0.25f + (float)(Random.Shared.NextDouble() - 0.5) * dist * 0.4f, from.Y + dy * 0.5f);
			Vector2 cp2 = from + new Vector2(dx * 0.75f + (float)(Random.Shared.NextDouble() - 0.5) * dist * 0.4f, from.Y + dy * 0.8f);

			Vector2[] pts = new Vector2[segments + 1];
			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments;
				float u = 1f - t;
				float b0 = u * u * u;
				float b1 = 3f * u * u * t;
				float b2 = 3f * u * t * t;
				float b3 = t * t * t;
				pts[i] = from * b0 + cp1 * b1 + cp2 * b2 + to * b3;
			}

			int crosses = CountSelfIntersections(pts);
			int diff = Math.Abs(crosses - desiredCross);
			if (diff < bestDiff)
			{
				bestDiff = diff;
				best = pts;
			}
			if (crosses == desiredCross)
			{
				return pts;
			}
		}

		return best ?? new Vector2[segments + 1];
	}

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
}

