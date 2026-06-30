using Godot;
using System.Threading.Tasks;

namespace SpaceLockdown;

public partial class Main : Node2D
{
	private Player? _player;
	private Node2D? _currentSpaceNode;
	private PortalNode? _currentPortal;
	private bool _isTransitioning;
	private GameState _gameState = null!;
	private SceneManager _sceneManager = null!;

	public override void _Ready()
	{
		_gameState = GetNode<GameState>("/root/GameState");
		_sceneManager = GetNode<SceneManager>("/root/SceneManager");

		if (_gameState == null)
			GD.PushError("Main: GameState autoload not found!");

		if (_sceneManager == null)
			GD.PushError("Main: SceneManager autoload not found!");

		SpawnPlayer();
		LoadSpace("SC-00");
	}

	private void SpawnPlayer()
	{
		var scene = GD.Load<PackedScene>("res://scenes/player.tscn");
		if (scene == null)
		{
			GD.PushError("Main: 无法加载玩家场景");
			return;
		}
		_player = scene.Instantiate<Player>();
		_player.InteractPressed += OnPlayerInteract;
		AddChild(_player);
	}

	private async void OnPlayerInteract()
	{
		if (_currentPortal == null || _isTransitioning)
			return;

		_isTransitioning = true;
		SetPlayerActive(false);

		var portal = _currentPortal;
		_currentPortal = null; // 防止双击 E 触发两次

		// 1. 淡出
		await _sceneManager.FadeOut();

		// 2. 更新游戏状态，检查是否胜利
		portal.Activate();

		if (_gameState.IsWin)
		{
			await _sceneManager.FadeIn();
			SetPlayerActive(true);
			_isTransitioning = false;
			return;
		}

		// 3. 加载新空间
		LoadSpace(_gameState.CurrentSceneId);

		// 4. 淡入
		await _sceneManager.FadeIn();

		SetPlayerActive(true);
		_isTransitioning = false;
	}

	private void SetPlayerActive(bool active)
	{
		if (_player != null)
		{
			_player.SetProcess(active);
			_player.SetPhysicsProcess(active);
			_player.SetProcessInput(active);
		}
	}

	private void LoadSpace(string sceneId)
	{
		// 卸载旧空间
		if (_currentSpaceNode != null)
		{
			_currentSpaceNode.QueueFree();
			_currentSpaceNode = null;
			_currentPortal = null;
		}

		var path = GetScenePath(sceneId);
		var sceneRes = GD.Load<PackedScene>(path);
		if (sceneRes == null)
		{
			GD.PushError($"Main: 无法加载场景 {path}，回退到初始空间");
			sceneRes = GD.Load<PackedScene>("res://scenes/sc_00_start.tscn");
			sceneId = "SC-00";
		}

		_currentSpaceNode = sceneRes.Instantiate<Node2D>();
		AddChild(_currentSpaceNode);
		MoveChild(_currentSpaceNode, 0);

		_gameState.CurrentSceneId = sceneId;

		// 连接该空间中所有传送门的 PlayerNearby/PlayerLeft 信号
		ConnectPortalSignals(_currentSpaceNode);

		// 放置玩家到出生点
		SpawnPlayerAtMarker();
	}

	/// <summary>
	/// 连接场景中所有 PortalNode 的 PlayerNearby/PlayerLeft 信号。
	/// 每次 LoadSpace 时 PortalNode 都是新实例，不会重复绑定。
	/// </summary>
	private void ConnectPortalSignals(Node node)
	{
		var portals = FindPortals(node);
		foreach (var portal in portals)
		{
			portal.PlayerNearby += OnPortalPlayerNearby;
			portal.PlayerLeft += OnPortalPlayerLeft;
		}
	}

	/// <summary>
	/// 从节点递归查找所有 PortalNode 实例。
	/// </summary>
	private static PortalNode[] FindPortals(Node node)
	{
		var result = new System.Collections.Generic.List<PortalNode>();
		if (node is PortalNode pn)
			result.Add(pn);
		foreach (var child in node.GetChildren())
			result.AddRange(FindPortals(child));
		return result.ToArray();
	}

	private void OnPortalPlayerNearby(PortalNode portal)
	{
		_currentPortal = portal;
	}

	private void OnPortalPlayerLeft()
	{
		_currentPortal = null;
	}

	private void SpawnPlayerAtMarker()
	{
		if (_currentSpaceNode == null || _player == null)
			return;
		var marker = _currentSpaceNode.FindChild("SpawnPoint", true, false) as Marker2D;
		_player.Position = marker?.Position ?? new Vector2(400, 380);
	}

	private static string GetScenePath(string sceneId)
	{
		return sceneId switch
		{
			"SC-00" => "res://scenes/sc_00_start.tscn",
			"SC-01" => "res://scenes/sc_01_corridor.tscn",
			"SC-02" => "res://scenes/sc_02_vent.tscn",
			"SC-03" => "res://scenes/sc_03_server.tscn",
			"SC-10" => "res://scenes/sc_10_final.tscn",
			_       => "res://scenes/sc_00_start.tscn",
		};
	}
}
