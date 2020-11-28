using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catacumba.LevelGen
{
    class LevelGenAlgoWalkers : ILevelGenAlgo 
    {
        public IEnumerator Run(Level l, System.Action<Level> updateVis=null)
        {
            List<BaseWalker> walkers = GenerateWalkers(l);
            while (walkers.Count > 0)
            {
                for (int i = 0; i < walkers.Count; i++)
                {
                    BaseWalker w = walkers[i];
                    if (w.Walk())
                        i--;
                }

                updateVis?.Invoke(l);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private List<BaseWalker> GenerateWalkers(Level l)
        {
            int hip = Mathf.Max(l.Size.x, l.Size.y);

            int        nWalkers         = Mathf.RoundToInt(hip / 2.5f);
            int        walkerLife       = nWalkers;
            float      walkerTurnChance = Mathf.Max(0.15f, .25f/(hip/25));
            Vector2Int walkerRoomSz     = (l.Size*2) / nWalkers;

            LevelGeneration.ECellCode[] rooms = GenerateRooms(l, nWalkers);
            List<BaseWalker> walkers = new List<BaseWalker>();

            System.Action<BaseWalker> OnDeathCallback = delegate (BaseWalker w) { walkers.Remove(w); };

            int walkerUpperBound = nWalkers - 1;
            for (int i = 0; i < walkerUpperBound; i++)
            {
                BaseWalker walker = CreateWalker(l, 
                                                 walkerLife, 
                                                 walkerTurnChance, 
                                                 walkerRoomSz, 
                                                 rooms[i],
                                                 OnDeathCallback);
                walkers.Add(walker);
            }

            walkers.Add(CreateWalker(l, 
                                     walkerLife, 
                                     walkerTurnChance, 
                                     walkerRoomSz, 
                                     LevelGeneration.ECellCode.BossRoom,
                                     OnDeathCallback));
            return walkers;
        }

        private static BaseWalker CreateWalker(Level l, 
                                               int walkerLife, 
                                               float walkerTurnChance, 
                                               Vector2Int walkerRoomSz, 
                                               LevelGeneration.ECellCode room,
                                               System.Action<BaseWalker> OnDeathCallback)
        {
            int x = Random.Range((int)(l.Size.x * 0.3f), (int)(l.Size.x * 0.7f));
            int y = Random.Range((int)(l.Size.y * 0.3f), (int)(l.Size.y * 0.7f));

            Vector2Int pos = new Vector2Int(x, y);
            Vector2Int sz = walkerRoomSz;
            BaseWalker walker = new KamikazeWalker(
                l.BaseSector,
                pos,
                Random.Range(Mathf.Max(2, walkerLife / 2), walkerLife + 1),
                walkerTurnChance + Random.Range(-0.05f, 0.1f),
                new Vector2Int(Random.Range(sz.x / 2, sz.x), Random.Range(sz.y / 2, sz.y)),
                room
            );
            if (OnDeathCallback != null)
                walker.OnDeath += OnDeathCallback;
            return walker;
        }

        private LevelGeneration.ECellCode[] GenerateRooms(Level level, int nWalkers)
        {
            Utilities.ShuffleBag<LevelGeneration.ECellCode> bag = new Utilities.ShuffleBag<LevelGeneration.ECellCode>();
            bag.Add(LevelGeneration.ECellCode.RoomEnemies, 50);
            bag.Add(LevelGeneration.ECellCode.RoomItem, 10);
            bag.Add(LevelGeneration.ECellCode.RoomKillChallenge, 10);
            bag.Add(LevelGeneration.ECellCode.RoomBloodOath, 5);
            bag.Add(LevelGeneration.ECellCode.RoomDice, 5);
            bag.Add(LevelGeneration.ECellCode.RoomChase, 5);
            return bag.Next(nWalkers);
        }
    }
}