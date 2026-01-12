using Spine;
using System.Collections.Generic;

namespace World.Helpers
{
    public class SkeletonAnim_Helper
    {
        public static void anim2bones(Skeleton sk, IEnumerable<string> bone_names, ref Dictionary<string, Bone> dic)
        {
            dic = new();

            foreach (var bone_name in bone_names)
            {
                dic.Add(bone_name, sk.FindBone(bone_name));
            }
        }
    }
}

