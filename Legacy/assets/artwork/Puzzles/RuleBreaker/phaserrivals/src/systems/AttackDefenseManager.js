export default class AttackDefenseManager {
  constructor(scene) {
    this.scene = scene;
    this.attackCharge = 0;
    this.defenseCharge = 0;
    this.ruleHoldReady = false;
    this.nullifyReady = false;
    this.adaptBoostReady = false;
    this.cooldowns = {
      forcedShift: 0,
      ruleFog: 0,
      inverseLaw: 0,
      chaosPulse: 0,
      ruleHold: 0,
      clarity: 0,
      adaptBoost: 0,
      nullify: 0
    };
  }

  tick(ms) {
    Object.keys(this.cooldowns).forEach(k => this.cooldowns[k] = Math.max(0, this.cooldowns[k] - ms));
  }

  addCharge(amount = 1) {
    this.attackCharge = Math.min(100, this.attackCharge + amount * 8);
    this.defenseCharge = Math.min(100, this.defenseCharge + amount * 8);
  }

  canUseAttack(cost, key) { return this.attackCharge >= cost && this.cooldowns[key] <= 0; }
  canUseDefense(cost, key) { return this.defenseCharge >= cost && this.cooldowns[key] <= 0; }

  spend(type, kind, cost, cooldownMs) {
    if (kind === 'attack') this.attackCharge -= cost;
    else this.defenseCharge -= cost;
    this.cooldowns[type] = cooldownMs;
  }
}
