### Input schema

```json
{
  "id": "some-id",
  "gameTimeMS": 120000000000000000000,
  "championIds": [1, 2, 3],
  "baronKillsTeam1": 12,
  "baronKillsTeam2": 13,
  "dragonKillsTeam1": 2,
  "dragonKillsTeam2": 4,
  "towerKillsTeam1": 3,
  "towerKillsTeam2": 6,
  "goldTeam1": 42000,
  "goldTeam2": 34000,
  "killsTeam1": 18,
  "killsTeam2": 4,
  "deathsTeam1": 12,
  "deathsTeam2": 8,
  "assistsTeam1": 7,
  "assistsTeam2": 3,
  "champLvlsTeam1": 82,
  "champLvlsTeam2": 68,
  "minionKillsTeam1": 3600,
  "minionKillsTeam2": 1400,
  "minLeagueTeam1": "DIAMOND",
  "minLeagueTeam2": "DIAMOND",
  "maxLeagueTeam1": "DIAMOND",
  "maxLeagueTeam2": "DIAMOND"
}
```

### Output schema

```json
{
  "id": "some-id",
  "team1": 0.92,
  "team2": 0.08
}
```
