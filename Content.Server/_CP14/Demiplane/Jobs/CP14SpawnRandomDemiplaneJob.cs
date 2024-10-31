using System.Threading;
using System.Threading.Tasks;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Procedural;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Jobs;

public sealed class CP14SpawnRandomDemiplaneJob : Job<bool>
{
    private readonly IEntityManager _entManager;
    //private readonly IGameTiming _timing;
    private readonly IMapManager _mapManager;
    private readonly IPrototypeManager _prototypeManager;
    //private readonly AnchorableSystem _anchorable;
    private readonly DungeonSystem _dungeon;
    private readonly MetaDataSystem _metaData;
    //private readonly SharedTransformSystem _xforms;
    private readonly SharedMapSystem _map;

    private readonly ProtoId<CP14DemiplaneLocationPrototype> _config;
    private readonly int _seed;

    public readonly EntityUid DemiplaneMapUid;
    private readonly MapId _demiplaneMapId;

    private readonly ISawmill _sawmill;

    public CP14SpawnRandomDemiplaneJob(
        double maxTime,
        IEntityManager entManager,
        ILogManager logManager,
        IMapManager mapManager,
        IPrototypeManager protoManager,
        DungeonSystem dungeon,
        MetaDataSystem metaData,
        SharedMapSystem map,
        EntityUid demiplaneMapUid,
        MapId demiplaneMapId,
        ProtoId<CP14DemiplaneLocationPrototype> config,
        int seed,
        CancellationToken cancellation = default) : base(maxTime, cancellation)
    {
        _entManager = entManager;
        _mapManager = mapManager;
        _prototypeManager = protoManager;
        _dungeon = dungeon;
        _metaData = metaData;
        _map = map;
        DemiplaneMapUid = demiplaneMapUid;
        _demiplaneMapId = demiplaneMapId;
        _config = config;
        _seed = seed;

        _sawmill = logManager.GetSawmill("cp14_expedition_job");
    }

    protected override async Task<bool> Process()
    {
        _sawmill.Debug("cp14_expedition", $"Spawning expedition mission with seed {0}");
        var grid = _mapManager.CreateGridEntity(DemiplaneMapUid);

        MetaDataComponent? metadata = null;

        _metaData.SetEntityName(DemiplaneMapUid, "TODO: MAP Expedition name generation");
        _metaData.SetEntityName(grid, "TODO: GRID Expedition name generation");

        //Spawn island config
        var expeditionConfig = _prototypeManager.Index(_config);
        var locationConfig = _prototypeManager.Index(expeditionConfig.LocationConfig);
        _dungeon.GenerateDungeon(locationConfig,
                grid,
                grid,
                Vector2i.Zero,
                _seed); //Not async, because dont work with biomespawner boilerplate

        //Add map components
        _entManager.AddComponents(DemiplaneMapUid, expeditionConfig.Components);

        //Setup gravity
        var gravity = _entManager.EnsureComponent<GravityComponent>(DemiplaneMapUid);
        gravity.Enabled = true;
        _entManager.Dirty(DemiplaneMapUid, gravity, metadata);

        // Setup default atmos
        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        moles[(int) Gas.Oxygen] = 21.824779f;
        moles[(int) Gas.Nitrogen] = 82.10312f;
        var mixture = new GasMixture(moles, Atmospherics.T20C);
        _entManager.System<AtmosphereSystem>().SetMapAtmosphere(DemiplaneMapUid, false, mixture);

        _mapManager.DoMapInitialize(_demiplaneMapId);
        _mapManager.SetMapPaused(_demiplaneMapId, false);

        //Dungeon

        return true;
    }
}
