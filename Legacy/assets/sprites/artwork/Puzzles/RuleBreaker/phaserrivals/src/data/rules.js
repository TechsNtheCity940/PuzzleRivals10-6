const rules = [
  { id: 'color', name: 'Chromalink', description: 'Match 3 tiles sharing the same color band.', themeColor: 0xff5fb2, short: 'COLOR' },
  { id: 'family', name: 'Sigil Sync', description: 'Match 3 tiles from the same icon family.', themeColor: 0x5bd8ff, short: 'SIGIL' },
  { id: 'sum', name: 'Volt Sum', description: 'Select tiles whose values add exactly to 10.', themeColor: 0xffd166, short: 'SUM' },
  { id: 'adjacency', name: 'Chain Route', description: 'Select 3 tiles in a connected orthogonal chain.', themeColor: 0x87ffb6, short: 'CHAIN' },
  { id: 'mirror', name: 'Mirror Lock', description: 'Select 2 tiles mirrored across the row center.', themeColor: 0x9b7dff, short: 'MIRROR' }
];

export const TILE_FAMILIES = [
  { id: 'pulse', iconKey: 'familyPulse', badge: 'PLS' },
  { id: 'nova', iconKey: 'familyNova', badge: 'NVA' },
  { id: 'flux', iconKey: 'familyFlux', badge: 'FLX' },
  { id: 'vector', iconKey: 'familyVector', badge: 'VTR' },
  { id: 'prism', iconKey: 'familyPrism', badge: 'PRM' }
];

export default rules;
