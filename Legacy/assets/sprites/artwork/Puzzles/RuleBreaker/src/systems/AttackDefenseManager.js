export default class AttackDefenseManager {
  constructor(scene) {
    this.scene = scene;
    this.attackCharge = 0;
    this.defenseCharge = 0;
  }

  addCharge(amount = 1) {
    this.attackCharge += amount;
    this.defenseCharge += amount;
  }

  canUseAttack(cost = 3) {
    return this.attackCharge >= cost;
  }

  canUseDefense(cost = 3) {
    return this.defenseCharge >= cost;
  }

  useForcedShift(ruleManager) {
    if (!this.canUseAttack()) return false;
    this.attackCharge -= 3;
    ruleManager.shiftRule();
    return true;
  }

  useClarity(scene, rule) {
    if (!this.canUseDefense()) return false;
    this.defenseCharge -= 3;
    scene.showClarityOverlay(rule);
    return true;
  }
}