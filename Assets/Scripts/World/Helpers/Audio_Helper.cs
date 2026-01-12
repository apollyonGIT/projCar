using Commons;
using Foundations;
using Foundations.Tickers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.Helpers
{
    public class Audio_Helper
    {
        public static void play_bgm()
        {
            //Share_DS.instance.try_get_value(Game_Mgr.key_world_id, out string world_id);
            //AutoCodes.game_worlds.TryGetValue(world_id, out var r_game_world);

            var clip_name = WorldContext.instance.r_scene.bgm;
            if (clip_name == null || clip_name == "") return;

            Addrs.Addressable_Utility.try_load_asset(clip_name, out AudioClip audio_clip);

            var audio_source = WorldSceneRoot.instance.bgm_source;
            audio_source.clip = audio_clip;

            //规则：渐入
            Request req = new("bgm_fade_in",
                (req) => { return req.countdown <= 0; },
                (req) =>
                {
                    audio_source.volume = 0;
                    req.countdown = 3 * Config.PHYSICS_TICKS_PER_SECOND;
                },
                (_) => {
                    audio_source.volume = 1f;
                },
                (req) =>
                {
                    audio_source.volume += 1 / (3f * Config.PHYSICS_TICKS_PER_SECOND);
                    req.countdown--;
                });

            audio_source.Play();
            Debug.Log($"========  已加载音乐：{clip_name}  ========");
        }


        public static void stop_bgm()
        {
            var audio_source = WorldSceneRoot.instance.bgm_source;
            if (audio_source == null) return;

            //规则：渐出
            Request req = new("bgm_fade_out",
                (req) => { return req.countdown <= 0; },
                (req) =>
                {
                    req.countdown = 2 * Config.PHYSICS_TICKS_PER_SECOND;
                },
                (_) => {
                    audio_source.volume = 0;
                    audio_source.Stop();
                    Debug.Log($"========  已结束音乐  ========");
                },
                (req) =>
                {
                    audio_source.volume -= 1 / (2f * Config.PHYSICS_TICKS_PER_SECOND);
                    req.countdown--;
                });
        }
    }
}

