using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VEF.AnimalGenes;
using Verse;

namespace VanillaRanchingExpanded;

public sealed class CompProperties_GroupedEggGenes : CompProperties
{
    public CompProperties_GroupedEggGenes()
    {
        compClass = typeof(CompGroupedEggGenes);
    }
}

public sealed class CompGroupedEggGenes : CompStackByGenes
{

    private List<EggGeneGroup> groups;

    public override bool AllowStackWith(Thing other)
    {
        if (other.TryGetComp<CompGroupedEggGenes>() != null && parent.TryGetComp<CompAnimalGenes>() is { } target && other.TryGetComp<CompAnimalGenes>() is { } source)
        {
            return target.HasAssignedGenes == source.HasAssignedGenes;
        }     
        return base.AllowStackWith(other);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode != LoadSaveMode.Saving || groups != null)
        {
            Scribe_Collections.Look(ref groups, "vreEggGeneGroups", LookMode.Deep);
        }          
    }

    public override void PreAbsorbStack(Thing otherStack, int count)
    {
        base.PreAbsorbStack(otherStack, count);
        if (count <= 0 || otherStack == parent ||
            otherStack.TryGetComp<CompGroupedEggGenes>() is not { } source ||
            parent.TryGetComp<CompAnimalGenes>() is not { HasAssignedGenes: true } targetGenes ||
            otherStack.TryGetComp<CompAnimalGenes>() is not { HasAssignedGenes: true } sourceGenes)
            return;

        if (groups == null && source.groups == null && targetGenes.genes.SequenceEqual(sourceGenes.genes))
            return;

        var destination = GroupsForCount(parent.stackCount, targetGenes);
        var remaining = source.GroupsForCount(otherStack.stackCount, sourceGenes);
        foreach (var moved in TakeTail(remaining, count))
            AppendGroup(destination, moved);

        SetGroups(destination, targetGenes);
        source.SetGroups(remaining, sourceGenes);
    }

    public override void PostSplitOff(Thing piece)
    {
        base.PostSplitOff(piece);
        if (piece == parent || groups == null ||
            piece.TryGetComp<CompGroupedEggGenes>() is not { } split ||
            parent.TryGetComp<CompAnimalGenes>() is not { } sourceGenes ||
            piece.TryGetComp<CompAnimalGenes>() is not { } splitGenes)
            return;

        var remaining = GroupsForCount(parent.stackCount + piece.stackCount, sourceGenes);
        split.SetGroups(TakeTail(remaining, piece.stackCount), splitGenes);
        SetGroups(remaining, sourceGenes);
    }

    private List<EggGeneGroup> GroupsForCount(int count, CompAnimalGenes genes)
    {
        if (groups == null)
            return new List<EggGeneGroup> { new EggGeneGroup(count, genes.genes.ToList()) };

        int recordedCount = groups.Sum(group => group.Count);
        if (recordedCount > count)
            TakeTail(groups, recordedCount - count);
        else if (recordedCount < count)
            AppendGroup(groups, new EggGeneGroup(count - recordedCount, genes.genes.ToList()));
        return groups;
    }

    private void SetGroups(List<EggGeneGroup> updated, CompAnimalGenes genes)
    {
        groups = updated.Count > 1 ? updated : null;
        if (updated.Count > 0 && !genes.genes.SequenceEqual(updated[0].Genes))
            genes.SetInheritedGenes(updated[0].Genes);
    }

    private static void AppendGroup(List<EggGeneGroup> destination, EggGeneGroup group)
    {
        var last = destination.Count == 0 ? null : destination[^1];
        if (last != null && last.Genes.SequenceEqual(group.Genes))
            last.Count += group.Count;
        else
            destination.Add(group);
    }

    private static List<EggGeneGroup> TakeTail(List<EggGeneGroup> source, int count)
    {
        var moved = new List<EggGeneGroup>();
        while (count > 0)
        {
            var last = source[^1];
            if (last.Count > count)
            {
                moved.Add(last.WithCount(count));
                last.Count -= count;
                break;
            }

            source.RemoveAt(source.Count - 1);
            moved.Add(last);
            count -= last.Count;
        }
        moved.Reverse();
        return moved;
    }

    internal static HatchCursor StartHatch(Thing egg)
    {
        if (egg.AnimalGenes is not CompAnimalGenes { HasAssignedGenes: true } comp)
            return default;
        return new HatchCursor(comp.genes, egg.TryGetComp<CompGroupedEggGenes>()?.groups);
    }

    internal struct HatchCursor
    {
        private readonly IReadOnlyList<AnimalGeneDef> fallback;
        private readonly List<EggGeneGroup> groups;
        private int index;
        private int used;

        internal HatchCursor(IReadOnlyList<AnimalGeneDef> fallback, List<EggGeneGroup> groups)
        {
            this.fallback = fallback;
            this.groups = groups;
            index = 0;
            used = 0;
        }

        internal IReadOnlyList<AnimalGeneDef> TakeNext()
        {
            if (groups == null || index >= groups.Count)
                return fallback;

            var group = groups[index];
            if (++used == group.Count)
            {
                index++;
                used = 0;
            }
            return group.Genes;
        }
    }

    public sealed class EggGeneGroup : IExposable
    {
        private List<AnimalGeneDef> genes = new List<AnimalGeneDef>();
        public int Count;
        public IReadOnlyList<AnimalGeneDef> Genes => genes;

        public EggGeneGroup()
        {
        }

        internal EggGeneGroup(int count, List<AnimalGeneDef> genes)
        {
            Count = count;
            this.genes = genes;
        }

        internal EggGeneGroup WithCount(int count) => new EggGeneGroup(count, genes);

        public void ExposeData()
        {
            Scribe_Values.Look(ref Count, "count");
            Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
        }
    }
}
