using System.Collections;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace ProceduralGraph.Interface;

/// <summary>
/// GraphModelCollectionEditor class.
/// </summary>
[CustomEditor(typeof(ObservableCollection<GraphComponent>))]
internal sealed class GraphModelCollectionEditor : ListEditor
{
    private ObservableCollection<GraphComponent> Target => (Values[0] as ObservableCollection<GraphComponent>)!;

    public override int Count => Target.Count;

    protected override IList Allocate(int size)
    {
        return new ObservableCollection<GraphComponent>(size);
    }

    protected override IList CloneValues()
    {
        return Target.Clone();
    }

    protected override void Resize(int newSize)
    {
        Target.Resize(newSize);
    }
}
