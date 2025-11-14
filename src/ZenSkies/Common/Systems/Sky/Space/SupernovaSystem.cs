using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ZenSkies.Core.ModCall;
using ZenSkies.Core.Net;
using static ZenSkies.Common.Systems.Sky.Space.StarSystem;
using static ZenSkies.Core.Net.NetMessageHooks;

namespace ZenSkies.Common.Systems.Sky.Space;

public sealed class SupernovaSystem : ModSystem, IPacketHandler
{
    #region Loading

    public override void Load() =>
        OnSyncWorldData += WorldDataSupernovae;

    #endregion

    #region Spawning

    [ModCall("TriggerRandomSupernova", "ExplodeRandomStar", "SpawnRandomSupernova")]
    public static void CreateRandomSupernovae(bool shouldSync = true)
    {
        AddStarModifier(s => new Supernova(s));

        if (shouldSync &&
            Main.netMode != NetmodeID.SinglePlayer)
            PacketSystem.Send<SupernovaSystem>(ignoreClient: Main.myPlayer);
    }

    #endregion

    #region Saving and Syncing

    public override void OnWorldLoad() =>
        ResetSupernovae();

    public override void OnWorldUnload() =>
        ResetSupernovae();

    public override void ClearWorld() =>
        ResetSupernovae();

    #region World Data

    public override void SaveWorldData(TagCompound tag)
    {
        int count = StarModifiersCount<Supernova>();

        tag["ActiveCount"] = count;

        int index = 0;

        ForStarModifiers((int i, Supernova s) =>
        {
            tag["Supernovae" + index] = i;

            tag[nameof(Supernova.NebulaColor) + index] = s.NebulaColor.PackedValue;

            tag[nameof(Supernova.Contract) + index] = s.Contract;
            tag[nameof(Supernova.Expand) + index] = s.Expand;

            index++;
        });
    }

    public override void LoadWorldData(TagCompound tag)
    {
        try
        {
            int count = tag.Get<int>("ActiveCount");

            for (int i = 0; i < count; i++)
            {
                int index = tag.Get<int>("Supernovae" + i);

                    // Load the color from the packed value.
                Color nebulaColor = new(tag.Get<uint>(nameof(Supernova.NebulaColor) + i));

                    // Create a new active supernova.
                Supernova s = new(Stars[index], nebulaColor);

                float contract = tag.Get<float>(nameof(Supernova.Contract) + i);
                float expand = tag.Get<float>(nameof(Supernova.Expand) + i);

                s.Contract = contract;
                s.Expand = expand;

                AddStarModifier(su => s, index);
            }
        }
        catch (Exception ex)
        {
            Mod.Logger.Error($"Failed to load supernovae: {ex.Message}");
        }
    }

    #endregion

    #region Net Syncing

    private void WorldDataSupernovae(int toClient, int ignoreClient) =>
        PacketSystem.Send<SupernovaSystem>(toClient, ignoreClient);

    void IPacketHandler.Write(BinaryWriter writer)
    {
        if (!Mod.IsNetSynced)
            return;

        int count = StarModifiersCount<Supernova>();

        writer.Write7BitEncodedInt(count);

        ForStarModifiers((int i, Supernova s) =>
        {
            writer.Write7BitEncodedInt(i);

            writer.Write(s.NebulaColor.PackedValue);

            writer.Write(s.Contract);
            writer.Write(s.Expand);
        });
    }

    void IPacketHandler.Receive(BinaryReader reader)
    {
        if (!Mod.IsNetSynced)
            return;

        try
        {
            int count = reader.Read7BitEncodedInt();

            for (int i = 0; i < count; i++)
            {
                int index = reader.Read7BitEncodedInt();

                    // Load the color from the packed value.
                Color nebulaColor = new(reader.ReadUInt32());

                    // Create a new active supernova.
                Supernova s = new(Stars[index], nebulaColor);

                float contract = reader.ReadSingle();
                float expand = reader.ReadSingle();
                float decay = reader.ReadSingle();

                s.Contract = contract;
                s.Expand = expand;

                AddStarModifier(su => s, index);
            }
        }
        catch (Exception ex)
        {
            Main.NewText($"Failed to sync stars: {ex.Message}", Color.Red);
            Mod.Logger.Error($"Failed to sync stars: {ex.Message}");
        }
    }

    #endregion

    #endregion

    #region Public Methods

    [ModCall("ResetSupernova", "ResetSupernovas",
        "ClearSupernova", "ClearSupernovas", "ClearSupernovae")]
    public static void ResetSupernovae() =>
        RemoveStarModifiers<Supernova>();

    #endregion
}
