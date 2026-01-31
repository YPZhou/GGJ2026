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
	Vector2 neckEndOffset;
	
	// 脖子曲线系统
	Curve2D neckCurve;
	List<Sprite2D> neckSegmentNodes = new List<Sprite2D>();
	bool needsNeckUpdate = true;

	[Export] float headSmoothingSpeed = 5.0f;
	[Export] int neckSegments = 60; // Now used as a fallback or limit
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

		// Calculate offset from Head to NeckEnd
		neckEndOffset = Vector2.Zero;

		currentHeadTargetPos = catHead.Position;
		
		// 初始化曲线系统
		neckCurve = new Curve2D();
		
		// 初始化控制点（身体到头部的贝塞尔曲线）
		Vector2 bodyPos = catNecksParent.Position; // 身体位置（相对于Cat节点）
		Vector2 headPos = catHead.Position;
		Vector2 diff = headPos - bodyPos;
		
		// 设置初始控制点（1/3和2/3位置，稍微偏移形成曲线）
		currentCp1 = bodyPos + diff * 0.33f + new Vector2(20, 0);
		currentCp2 = bodyPos + diff * 0.66f + new Vector2(-20, 0);
		targetCp1 = currentCp1;
		targetCp2 = currentCp2;

		San = 100;

		CheckRes();

		// 初始生成脖子
		GenerateNeck();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (IsAlive && game.CurrentGameState == GameState.Playing)
		{
			if (CurrentStatus == TVStatus.GOOD)
			{
				
			}
			else if (CurrentStatus == TVStatus.Fool)
			{
				
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

	Vector2 GenerateRandomCatHeadPosition()
	{
		double targetOffset = 0.0;
		if (HeadYOffsets != null && HeadYOffsets.Length > 0)
		{
			int index = Random.Shared.Next(HeadYOffsets.Length);
			targetOffset = HeadYOffsets[index];
		}
		double randomX = initialX + (Random.Shared.NextDouble() * 2.0 - 1.0) * HeadXRange;
		Vector2 newPos = new Vector2((float)randomX, (float)(initialY + targetOffset));
		
		// 更新目标控制点
		Vector2 bodyPos = catNecksParent.Position;
		Vector2 diff = newPos - bodyPos;
		targetCp1 = bodyPos + diff * 0.33f + new Vector2(20, 0);
		targetCp2 = bodyPos + diff * 0.66f + new Vector2(-20, 0);
		
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
		
		Vector2 bodyPos = catNecksParent.Position; // 身体位置
		Vector2 headPos = catHead.Position + neckEndOffset; // 头部位置
		
		// 添加曲线点（起点、控制点、终点）
		neckCurve.AddPoint(bodyPos, Vector2.Zero, currentCp1 - bodyPos);
		neckCurve.AddPoint(headPos, currentCp2 - headPos, Vector2.Zero);
		
		// 2. 采样曲线上的点
		float curveLength = neckCurve.GetBakedLength();
		int segmentCount = Math.Max(1, Math.Min(neckSegments, (int)(curveLength / 10.0f))); // 根据长度自适应
		
		// 3. 调整节段数量
		while (neckSegmentNodes.Count > segmentCount)
		{
			var lastNode = neckSegmentNodes[neckSegmentNodes.Count - 1];
			lastNode.QueueFree();
			neckSegmentNodes.RemoveAt(neckSegmentNodes.Count - 1);
		}
		
		while (neckSegmentNodes.Count < segmentCount)
		{
			var sprite = new Sprite2D();
			sprite.Texture = catNeckTexture;
			sprite.Centered = true;
			catNecksParent.AddChild(sprite);
			neckSegmentNodes.Add(sprite);
		}
		
		// 4. 更新每个节段的位置、旋转和缩放
		for (int i = 0; i < segmentCount; i++)
		{
			float t = (i + 0.5f) / segmentCount; // 在节段中心采样
			
			// 采样位置
			Vector2 position = neckCurve.SampleBaked(t * curveLength);
			
			// 计算切线方向（用于旋转）- 使用前后点计算方向
			float offset = 1.0f; // 采样偏移距离
			Vector2 tangentPoint1 = neckCurve.SampleBaked(Mathf.Max(0, t * curveLength - offset));
			Vector2 tangentPoint2 = neckCurve.SampleBaked(Mathf.Min(curveLength, t * curveLength + offset));
			Vector2 tangent = (tangentPoint2 - tangentPoint1).Normalized();
			
			// 计算旋转角度（假设贴图原始方向是水平向右，如果是竖直向上则加Mathf.Pi/2）
			float rotation = tangent.Angle() + Mathf.Pi / 2; // 如果贴图是竖直的，改为: tangent.Angle() + Mathf.Pi / 2
			
			// 计算缩放（从身体到头部逐渐变细）
			float scaleRatio = Mathf.Lerp(1.2f, 0.8f, t); // 身体端粗，头部端细
			
			// 应用变换
			var segment = neckSegmentNodes[i];
			segment.Position = position;
			segment.Rotation = rotation;
			segment.Scale = new Vector2(scaleRatio, scaleRatio);
		}
		
		// 5. 调试可视化
		if (gizmo != null)
		{
			var points = new List<Vector2>();
			int debugSamples = 50;
			for (int i = 0; i <= debugSamples; i++)
			{
				float t = i / (float)debugSamples;
				points.Add(neckCurve.SampleBaked(t * curveLength));
			}
			gizmo.Points = points.ToArray();
		}
	}
}
