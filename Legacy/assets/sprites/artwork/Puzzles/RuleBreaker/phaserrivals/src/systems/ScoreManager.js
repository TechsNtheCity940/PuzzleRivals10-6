export default class ScoreManager {
  constructor() {
    this.score = 0;
    this.streak = 0;
  }

  addValidSolve(adaptBonus = false, extraBoost = false, skillBonus = 0) {
    let points = 10 + skillBonus;
    if (adaptBonus) points += 15;
    if (extraBoost) points += 8;
    this.score += points;
    this.streak += 1;
    return points;
  }

  breakStreak() { this.streak = 0; }
}
