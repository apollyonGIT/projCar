using AutoCodes;
using Commons;
using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Characters.CharacterStates;

namespace World.Characters
{
    public interface ICharacterMgrView : IModelView<CharacterMgr>
    {
        void notify_add_character(Character c);
        void notify_remove_character(Character c);
        void notify_on_tick();
        void notify_select_character(Character c);
        void notify_cancel_select_character();
        void notify_character_is_working(Character c, bool working);
        void notify_update_character(Character c);
        void notify_backup_character();
    }


    public class CharacterMgr : Model<CharacterMgr, ICharacterMgrView>, IMgr
    {
        public List<Character> characters = new List<Character>();
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public Character select_character;

        public CharacterBackUp backup = new();

        public Action<Character> add_character_action;
        public Action<Character> remove_character_action;

        public Vector2 character_focus_pos = Vector2.zero;

        public Vector2 character_focus_velocity = Vector2.zero;

        public Vector2 character_focus_aceleration = Vector2.zero;

        private Vector2 v_record = Vector2.zero;

        //==================================================================================================

        public void tick()
        {
            #region focus_position
            var v_car = WorldContext.instance.caravan_velocity;
            var a_car = (v_car - v_record) * Commons.Config.PHYSICS_TICKS_PER_SECOND;

            v_record = v_car;

            character_focus_aceleration.x = - character_focus_pos.x * Config.current.role_damp_px - character_focus_velocity.x * Config.current.role_damp_vx;
            if (character_focus_pos.y >= 0)
            {
                character_focus_aceleration.y = - character_focus_pos.y * Config.current.role_damp_py1 - character_focus_velocity.y * Config.current.role_damp_vy + Config.current.gravity;
            }
            else
            {
                character_focus_aceleration.y = - character_focus_pos.y * Config.current.role_damp_py2 - character_focus_velocity.y * Config.current.role_damp_vy + Config.current.gravity;
            }

            character_focus_aceleration -= a_car;

            character_focus_velocity += character_focus_aceleration / Config.PHYSICS_TICKS_PER_SECOND;

            character_focus_pos += character_focus_velocity / Config.PHYSICS_TICKS_PER_SECOND;

            character_focus_pos.x = Mathf.Clamp(character_focus_pos.x, -10, 10);
            character_focus_pos.y = Mathf.Clamp(character_focus_pos.y, -10, 10);

            #endregion

            foreach (var c in characters)
            {
                if (c != null)
                {
                    if(c.state == null)
                    {
                        var ay = Mathf.Abs(character_focus_aceleration.y);
                        if(ay > c.desc.coma_g)
                        {
                            c.coma_process += ay;
                        }
                        else
                        {
                            c.coma_process = 0;
                        }

                        if(c.coma_process >= 100)
                        {
                            c.state = new Coma()
                            {
                                duration = (int)(1800 *(1 - Mathf.Exp(-ay * 0.002f)))
                            };

                            c.coma_process = 0;
                        }
                    }
                    c.tick();
                }
            }

            foreach (var view in views)
            {
                view.notify_on_tick();
            }
        }

        public void tick1()
        {

        }

        public CharacterMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }
        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }

        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);
            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);

            character_focus_pos = Vector2.zero;
        }

        public Character AddCharacter(int id)
        {
            var c = new Character();
            roles.TryGetValue(id.ToString(), out role rc);

            c.Init(rc);
            c.Start();

            characters.Add(c);
            foreach (var view in views)
            {
                view.notify_add_character(c);
            }

            return c;
        }

        public void AddCharacter(Character c)
        {
            characters.Add(c);
            foreach (var view in views)
            {
                view.notify_add_character(c);
            }
        }

        public Character RemoveCharacter(Character c)
        {
            SetCharacterWorking(c, false);
            characters.Remove(c);
            foreach (var view in views)
            {
                view.notify_remove_character(c);
            }
            return c;
        }

        public void SelectCharacter(Character c)
        {
            if (select_character != null)
            {
                CancelSelectCharacter();
            }

            select_character = c;
            foreach (var view in views)
            {
                view.notify_select_character(c);
            }
        }

        public void CancelSelectCharacter()
        {
            select_character = null;
            foreach (var view in views)
            {
                view.notify_cancel_select_character();
            }
        }

        public void SetCharacterWorking(Character c, bool working)
        {
            if (working)
            {
                c.StartWork();
            }
            else
            {
                c.EndWork();
            }

            foreach (var view in views)
            {
                view.notify_character_is_working(c, working);
            }
        }

        public void UpdateBackup()
        {
            foreach (var view in views)
            {
                view.notify_backup_character();
            }
        }

        public void AddState(CharacterState cs)
        {
            var r = UnityEngine.Random.Range(0, characters.Count);
            characters[r].state = cs;
            characters[r].state.start();
        }
    }
}





