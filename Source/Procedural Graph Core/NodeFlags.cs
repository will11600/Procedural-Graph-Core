using System;

namespace ProceduralGraph;

[Flags]
public enum NodeFlags : byte
{
    None = 0,
    PropagateUpwards = 1 << 0,
    PropagateDownwards = 1 << 1
}
