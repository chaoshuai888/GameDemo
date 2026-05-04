using System.Collections.Generic;
using LawnDefense.Enemies;
using UnityEngine;

namespace LawnDefense.Combat
{
    public sealed class LaneTargetService : MonoBehaviour
    {
        private readonly List<Enemy> enemies = new List<Enemy>();

        public void Register(Enemy enemy)
        {
            if (enemy != null && !enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
        }

        public void Unregister(Enemy enemy)
        {
            enemies.Remove(enemy);
        }

        public Enemy FindFirstEnemyInLane(int lane, float minX, float maxX)
        {
            Prune();

            Enemy closest = null;
            float closestX = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || enemy.Lane != lane)
                {
                    continue;
                }

                float x = enemy.transform.position.x;
                if (x < minX || x > maxX || x >= closestX)
                {
                    continue;
                }

                closest = enemy;
                closestX = x;
            }

            return closest;
        }

        public int GetAliveEnemyCount()
        {
            Prune();
            int count = 0;

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null && enemies[i].IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private void Prune()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i] == null || !enemies[i].IsAlive)
                {
                    enemies.RemoveAt(i);
                }
            }
        }
    }
}
