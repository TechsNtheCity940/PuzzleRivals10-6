export default class ScoreManager {
  constructor() {
    this.score = 0;
    this.streak = 0;
  }

  addValidSolve(adaptBonus = false) {
    let points = 10;
    if (adaptBonus) points += 15;

    this.score += points;
    this.streak += 1;

    return points;
  }

  breakStreak() {
    this.streak = 0;
  }
}