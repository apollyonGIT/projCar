using UnityEngine;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public interface IAttack
    {
        float Damage_Increase { get; set; }
        float Knockback_Increase { get; set; }
        int Attack_Interval { get; set; }
        int Current_Interval { get; set; }
        float Attack_Interval_Factor { get; set; }
        public void TryToAutoAttack();
    }

    public interface ILoad
    {
        int Default_Ammo { get; set; }
        int Max_Ammo { get; set; }
        int Current_Ammo { get; set; }
        float Reloading_Process { get; set; }
        float Reload_Speed { get; set; }
        float Reload_Speed_Factor { get; set; }
        int Reload_Per_Ammo { get; set; }
        public void TryToAutoLoad();
    }

    public interface IRecycle
    {
        int Recycle_Interval { get; set; }
        int Current_Recycle_Interval { get; set; }
        public void TryToAutoRecycle();
    }

    public interface IRotate
    {
        float turn_angle { get; }

        public void Rotate(float angle);
        public void TryToAutoRotate();
    }

    public interface IShield
    {
        float ShieldEnergy_Deduct_By_Blocking { get; set; }
        int Shield_Blocking_Interval_Current {  get; set; }
        int Shield_Blocking_Interval_Max { get; set; }

        Vector2 Def_Dir { get; set; }
        float Def_Range { get; set; }
        Vector2 Shield_Dir { get; }
        bool Try_Rebound_Projectile(Projectile proj, Vector2 proj_v)
        {
            return false;
        }
    }

    public interface ISharp
    {
        float Sharpness_Current { get; set; }
        float Sharpness_Min { get; set; }
        float Sharpness_Loss { get; set; }
        float Sharpness_Recover { get; set; }

        public void TryToAutoSharp();
    }

    public interface IShoot
    {
        float Proj_Spread { get; set; }  //子弹散射角度
        float Proj_Speed { get; set; }
        int Proj_Num { get; set; }  //子弹数量
    }

    public interface IEnergy
    {
        float Default_Energy { get; set; }
        float Current_Energy { get; set; }            //当前能量
        float Max_Energy { get; set; }                //最大能量
        float Energy_Recover { get; set; }          //能量恢复速度
        float Energy_Recover_Factor { get; set; } //能量恢复速度因子
        int Energy_FreezeTick_Current { get; set; }   //冻结tick
        int Energy_FreezeTick_Max { get; set; }       //冻结tick最大值
        public void TryToAccelerate();
    }


    public interface IAttack_New
    {
        public void TryToAutoAttack();
    }

    public interface IReload_New
    {
        public void TryToAutoReload();
    }

    public interface IRotate_New
    {
        public void TryToAutoRotate();
    }

    public interface ICharge
    {
        public void TryToAutoCharge();
    }

    public interface ICarry
    {
        public void TryToAutoCarry();
    }

    public interface ISharpNew
    {
        public void TryToAutoSharp();
    }
}
