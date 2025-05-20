using System;
using System.Collections.Generic;
using System.Linq;

namespace Oscillite.CleanRoom.LSM
{
	
public class TriggerList : List<Trigger>
    {
        public Trigger SelectedTrigger
        {
            get => _selectedTrigger;
            set => _selectedTrigger = this[value.Source];
        }
        private Trigger _selectedTrigger = new Trigger();

        public Trigger this[TriggerSource source]
        {
            get
            {
                return this.FirstOrDefault(t => t.Source.Value == source.Value);
            }
        }

        public TriggerList(Trigger trigger)
        {
            Add(trigger);
            _selectedTrigger = this[base.Count - 1];
        }

        public TriggerList(TraceList traces)
        {
            _selectedTrigger = null;
            for (int i = 0; i < traces.Count; i++)
            {
                Add(new Trigger(traces[i]));
            }
            // Had to add Cylinder before None which seems odd, but it worked
            Add(new Trigger(SourceType.Cylinder));
            Add(new Trigger(SourceType.None));
            _selectedTrigger = this[base.Count - 1];
        }
    }
}
