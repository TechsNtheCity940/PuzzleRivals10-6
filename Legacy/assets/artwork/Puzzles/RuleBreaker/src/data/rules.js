const rules = [
  {
    id: 'color',
    name: 'Match by Color',
    description: 'Select 3 tiles of the same color.',
    durationMin: 15000,
    durationMax: 25000
  },
  {
    id: 'shape',
    name: 'Match by Shape',
    description: 'Select 3 tiles of the same shape.',
    durationMin: 15000,
    durationMax: 25000
  },
  {
    id: 'sum',
    name: 'Match by Sum',
    description: 'Select tiles that add up to 10.',
    durationMin: 15000,
    durationMax: 25000
  },
  {
    id: 'adjacency',
    name: 'Adjacent Chain',
    description: 'Select 3 adjacent tiles in a connected chain.',
    durationMin: 15000,
    durationMax: 25000
  },
  {
    id: 'mirror',
    name: 'Mirror Match',
    description: 'Select mirrored tiles across the center.',
    durationMin: 15000,
    durationMax: 25000
  }
];

export default rules;