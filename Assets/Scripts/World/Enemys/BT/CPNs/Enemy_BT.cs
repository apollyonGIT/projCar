
namespace World.Enemys.BT
{
    public class Enemy_BT
    {
        public virtual string state { get; }
        public string last_state = "";

        //==================================================================================================

        public virtual void init(Enemy cell, params object[] prms)
        {
        }


        public virtual void load_outter_view(Enemy cell, EnemyView view)
        {
        }


        public virtual void tick(Enemy cell)
        {
            Enemy_BT_Core_CPN.valid_fsm(this, cell);

            cell.mover.move();

            Enemy_BT_Core_CPN.valid_speed_expt_change(cell);

            notify_on_set_face_dir(cell);
        }


        public virtual void notify_on_enter_die(Enemy cell)
        {
            notify_on_dead(cell);
        }


        public virtual void notify_on_dying(Enemy cell)
        { 
        }


        public virtual void notify_on_dead(Enemy cell)
        {
            Enemy_BT_VFX_CPN.set_death_vfx(cell);

            cell.fini();
        }


        public virtual void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_by_velocity_only_x(cell);
        }


        public virtual void notify_on_enter_flee(Enemy cell, string end_state)
        {
        }
    }
}

