using System.Collections;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace ProceduralGraph.Interface;

/// <summary>
/// ObservableModelCollectionEditor class.
/// </summary>
[CustomEditor(typeof(ObservableCollection<Model>))]
internal sealed class ObservableModelCollectionEditor : CollectionEditor
{
    private ObservableCollection<Model> Target => (Values[0] as ObservableCollection<Model>)!;

    public override int Count => Target.Count;

    protected override IList Allocate(int size)
    {
        return new ObservableCollection<Model>(size);
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
