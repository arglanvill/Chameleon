using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Memory
{
    class MemoryMapRegion<T>
    {
        public MemoryMapRegion(uint start, uint end, T memory)
        {
            Start = start;
            End = end;
            Memory = memory;
        }
        public bool Contains(MemoryMapRegion<T> region)
        {
            return Contains(region.Start) && Contains(region.End);
        }
        public bool Contains(uint address)
        {
            return (Start <= address && address <= End);
        }
        public uint Start { get; set; }
        public uint End { get; set; }
        public T Memory { get; set; }
    }

    class MemoryMap<T> where T : class
    {
        public void InsertRegion(uint start, uint end, T memory)
        {
            bool regionAdded = false;
            MemoryMapRegion<T> newRegion = new MemoryMapRegion<T>(start, end, memory);
            for (int i = 0; i < Regions.Count; i++)
            {
                MemoryMapRegion<T> region = Regions[i];
                if (region.Start > end)
                {
                    if (!regionAdded)
                        Regions.Insert(i, newRegion);
                    return;
                }
                else if (newRegion.Contains(region))
                {
                    // New region completely contains
                    if (!regionAdded)
                    {
                        Regions[i] = newRegion;
                        regionAdded = true;
                    } 
                    else
                        Regions.RemoveAt(i--);
                }
                else if (region.Contains(newRegion.Start) || region.Contains(newRegion.End))
                {
                    MemoryMapRegion<T> bottom = null;
                    // Split current region
                    uint bottomSize = newRegion.Start - region.Start;
                    if (bottomSize > 0)
                        bottom = new MemoryMapRegion<T>(region.Start, newRegion.Start-1, region.Memory);
                    MemoryMapRegion<T> top = null;
                    // Split current region
                    uint topSize = region.End - newRegion.End;
                    if (topSize > 0)
                        top = new MemoryMapRegion<T>(newRegion.End+1, region.End, region.Memory);

                    if (!regionAdded)
                        Regions[i] = newRegion;
                    
                    if (bottom != null)
                        Regions.Insert(i++, bottom);

                    if (top != null)
                    {
                        if (!regionAdded)
                            Regions.Insert(++i, top);
                        else
                            Regions[i] = top;
                    }
                    regionAdded = true;
                }
            }
            if (regionAdded == false)
                Regions.Add(newRegion);
        }

        public void RemoveRegion(T memory)
        {
            for (int i = 0; i < Regions.Count; i++)
            {
                MemoryMapRegion<T> region = Regions[i];
                if (region.Memory == memory)
                {
                    if (i > 0 && i < Regions.Count - 1)
                    {
                        MemoryMapRegion<T> below = Regions[i - 1];
                        MemoryMapRegion<T> above = Regions[i + 1];
                        if (below.Memory == above.Memory)
                        {
                            below.End = above.End;
                            Regions.RemoveAt(i + 1);
                        }
                    }
                    Regions.RemoveAt(i);
                }
            }
        }
        public MemoryMapRegion<T> GetMemoryMapRegion(uint address)
        {
            foreach (MemoryMapRegion<T> region in Regions)
            {
                if (region.Contains(address))
                    return region;
            }
            return null;
        }

        private readonly List<MemoryMapRegion<T>> Regions = new List<MemoryMapRegion<T>>();
    }
}
